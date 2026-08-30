using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Adapters;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Ports;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Items;
using SoulsLike.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public sealed class Character : MonoBehaviour, IInitializable,
        ICharacterActionExecutor,
        IMovementPresentationSink,
        IAnimationStateSink,
        IRootMotionSink,
        IEquipmentLoadoutSink,
        IDisposable
    {
        private const float NORMAL_ATTACK_SPEED = 1.0f;

        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private AnimatorComponent animatorComponent;
        [SerializeField] private CharacterAudioComponent characterAudioComponent;
        [SerializeField] private EquipmentComponent equipmentComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private InventoryComponent inventoryComponent;
        [SerializeField] private EquipmentPresentation equipmentPresentation;
        [SerializeField] private Transform cameraTarget;

        [Header("Aim Settings")]
        [SerializeField, Min(0.1f)] private float aimTargetDistance = 100f;
        [SerializeField] private LayerMask aimLayerMask;

        private AttackComponent _attackComponent;
        private CharacterRuntime _runtime;
        private CharacterAnimationAdapter _animationAdapter;
        private EquipmentSwapCoordinator _equipmentSwapCoordinator;
        private ItemCatalog _itemCatalog;
        private IEntityLocator _entityLocator;
        private ICombatStateNotifier _combatStateNotifier;
        private CharacterData _characterData;
        private int _heldCurrency;

        public Transform CameraTarget => cameraTarget;
        public bool IsGrounded => movementComponent.Model.Grounded;
        public float VerticalVelocity => movementComponent.VerticalVelocity;
        public InventoryComponent InventoryComponent => inventoryComponent;
        public HealthStats HealthStats => healthComponent.Stats;
        public int HeldCurrency => _heldCurrency;
        public CharacterAttributeStats Attributes => _characterData.Attributes;
        public bool IsInputBlocked => _runtime.IsInputBlocked;
        public CharacterActionStateId CurrentActionState => _runtime.ActionState;
        public bool IsEquipmentActionInProgress => _equipmentSwapCoordinator.IsActive;

        [Inject]
        public void ConfigureRuntime(
            AttackComponent attackComponent,
            CharacterRuntime runtime,
            CharacterAnimationAdapter animationAdapter,
            EquipmentSwapCoordinator equipmentSwapCoordinator,
            EquipmentPresentation presentation,
            ItemCatalog itemCatalog,
            IEntityLocator entityLocator,
            ICombatStateNotifier combatStateNotifier,
            CharacterData characterData)
        {
            _attackComponent = attackComponent;
            _runtime = runtime;
            _animationAdapter = animationAdapter;
            _equipmentSwapCoordinator = equipmentSwapCoordinator;
            equipmentPresentation = presentation;
            _itemCatalog = itemCatalog;
            _entityLocator = entityLocator;
            _combatStateNotifier = combatStateNotifier;
            _characterData = characterData;
            _heldCurrency = characterData.StartingCurrency;
        }

        public void Initialize()
        {
            healthComponent.Model.OnDamageApplied += OnDamageApplied;
            animatorComponent.SetHandMode(equipmentComponent.Model.ActiveHandMode);
            ApplyEquipmentLoadout(equipmentComponent.BuildLoadout());
            Cursor.lockState = CursorLockMode.Locked;
            _runtime.SetInputBlocked(true);
            animatorComponent.TriggerSpawn();
        }

        public void Dispose()
        {
            healthComponent.Model.OnDamageApplied -= OnDamageApplied;
        }

        public void Tick(in CharacterInputBatch input)
        {
            _attackComponent.SetStrongAttackHeld(input.ControlFrame.StrongAttackHeld);
            if (!input.ControlFrame.StrongAttackHeld)
            {
                animatorComponent.SetChargedAttackSpeed(NORMAL_ATTACK_SPEED);
            }

            _runtime.Tick(input, this);
            ApplyRuntimeAnimationRequests();
            MovementPolicy policy = _runtime.ResolveMovementPolicy(false);
            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            bool blockRequested = input.ControlFrame.GuardHeld
                && policy.GuardAllowed
                && movementComponent.Model.Grounded;
            bool shieldBlock = blockRequested
                && loadout.HandMode == HandMode.OneHanded
                && loadout.EffectiveLeft != null
                && _itemCatalog.GetItem(loadout.EffectiveLeft.ItemId).ItemType == ItemType.Shield;
            bool weaponBlock = blockRequested
                && loadout.EffectiveLeft == null
                && loadout.EffectiveRight != null
                && _itemCatalog.GetItem(loadout.EffectiveRight.ItemId).ItemType == ItemType.Weapon;
            MovementModel movementModel = movementComponent.Model;
            bool combatSprintDrainsStamina =
                _combatStateNotifier.CurrentCombatState == CombatState.Combat
                && input.ControlFrame.SprintHeld
                && !input.ControlFrame.CrouchHeld;
            float sprintStaminaCost =
                movementModel.CombatSprintStaminaDrainPerSecond * Time.deltaTime;
            bool sprintAllowed = !combatSprintDrainsStamina
                || healthComponent.CanConsumeStamina(
                    sprintStaminaCost,
                    movementModel.CombatSprintStaminaStartThreshold);
            movementComponent.SetMovementBlocked(policy.MovementBlocked);
            movementComponent.Move(
                input.ControlFrame.MoveInput,
                input.ControlFrame.CameraYaw,
                input.ControlFrame.SprintHeld && sprintAllowed,
                input.ControlFrame.CrouchHeld);
            characterAudioComponent.Tick(
                movementComponent.IsMoving,
                input.ControlFrame.SprintHeld
                && sprintAllowed
                && !input.ControlFrame.CrouchHeld);

            if (combatSprintDrainsStamina
                && sprintAllowed
                && movementComponent.IsMoving)
            {
                healthComponent.TryConsumeStamina(
                    sprintStaminaCost,
                    movementModel.CombatSprintStaminaStartThreshold);
            }

            animatorComponent.SetShieldBlock(shieldBlock);
            animatorComponent.SetWeaponBlock(weaponBlock);
            healthComponent.TickStaminaRecovery(Time.deltaTime, shieldBlock || weaponBlock);
        }

        public CharacterCommandDisposition Submit(CharacterCommand command) =>
            _runtime.Submit(command, this);

        public CharacterCommandExecutionStatus TryStartAttack(in AttackRequest request)
        {
            bool canInterrupt = _runtime.ActionState is CharacterActionStateId.Attack
                or CharacterActionStateId.Roll;
            if (!movementComponent.Model.Grounded
                || _runtime.MovementGate.IsSet(MovementGateReason.Manual)
                || _runtime.MovementGate.IsSet(MovementGateReason.Spawn)
                || (_runtime.MovementGate.IsSet(MovementGateReason.Animation) && !canInterrupt))
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            if (request.Intent == AttackIntent.Special
                && _runtime.ActionState == CharacterActionStateId.Roll)
            {
                return CharacterCommandExecutionStatus.Invalid;
            }

            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            if (request.Intent == AttackIntent.Special
                && loadout.HandMode == HandMode.OneHanded
                && loadout.EffectiveLeft != null
                && _itemCatalog.GetItem(loadout.EffectiveLeft.ItemId).ItemType == ItemType.Shield)
            {
                animatorComponent.TriggerParry();
                _runtime.SetParryLocked(true);
                return CharacterCommandExecutionStatus.Executed;
            }

            ItemId? rightWeaponId = ResolveAttackWeaponId(loadout, false);
            ItemId? leftWeaponId = ResolveAttackWeaponId(loadout, true);
            bool hasRightWeapon = rightWeaponId.HasValue;
            bool hasLeftWeapon = leftWeaponId.HasValue;
            if ((request.IsLeftHand && !hasLeftWeapon)
                || (!request.IsLeftHand && !hasRightWeapon))
            {
                return CharacterCommandExecutionStatus.Invalid;
            }

            ItemId? weaponId = request.IsLeftHand
                ? leftWeaponId
                : rightWeaponId;
            if (weaponId.HasValue)
            {
                CombatProfile combatProfile = _itemCatalog.GetWeapon(weaponId.Value).CombatProfile;
                float staminaCost = ResolveAttackStaminaCost(request, combatProfile);
                float staminaStartThreshold = ResolveAttackStaminaStartThreshold(request, combatProfile);
                if (!healthComponent.CanConsumeStamina(staminaCost, staminaStartThreshold))
                {
                    return CharacterCommandExecutionStatus.TemporarilyBlocked;
                }

                healthComponent.ConsumeStamina(staminaCost);
            }

            AttackExecutionContext context = _attackComponent.CurrentExecutionContext;
            AttackResolution resolution = _attackComponent.ResolveAttack(request, context);
            animatorComponent.SetChargedAttackSpeed(resolution.ChargedSpeed);
            animatorComponent.PlayAttack(
                resolution.AttackType,
                resolution.IsLeftHandAttack);
            return CharacterCommandExecutionStatus.Executed;
        }

        public CharacterCommandExecutionStatus TryStartRoll(in RollRequest request)
        {
            bool canInterrupt = _runtime.ActionState != CharacterActionStateId.Neutral;
            MovementModel movementModel = movementComponent.Model;
            float staminaCost = movementModel.RollStaminaCost;
            if (!healthComponent.CanConsumeStamina(
                    staminaCost,
                    movementModel.RollStaminaStartThreshold))
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            if (!movementComponent.TryStartRoll(
                    request.MoveInput,
                    request.CameraYaw,
                    true,
                    canInterrupt))
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            healthComponent.ConsumeStamina(staminaCost);
            return CharacterCommandExecutionStatus.Executed;
        }

        public CharacterCommandExecutionStatus TryStartJump(in JumpRequest request)
        {
            MovementModel movementModel = movementComponent.Model;
            float staminaCost = movementModel.JumpStaminaCost;
            if (!healthComponent.CanConsumeStamina(
                    staminaCost,
                    movementModel.JumpStaminaStartThreshold))
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            if (!movementComponent.TryStartJump(true, request.IsSprinting))
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            healthComponent.ConsumeStamina(staminaCost);
            return CharacterCommandExecutionStatus.Executed;
        }

        public CharacterCommandExecutionStatus TryStartEquipmentAction(
            in EquipmentActionRequest request)
        {
            switch (request.Kind)
            {
                case EquipmentActionKind.SwitchRightWeapon:
                    return _equipmentSwapCoordinator.StartSwap(
                        EquipmentSlotGroup.RightHandArmament,
                        equipmentComponent,
                        animatorComponent,
                        equipmentPresentation);
                case EquipmentActionKind.SwitchLeftWeapon:
                    return _equipmentSwapCoordinator.StartSwap(
                        EquipmentSlotGroup.LeftHandArmament,
                        equipmentComponent,
                        animatorComponent,
                        equipmentPresentation);
                case EquipmentActionKind.SwitchQuickItem:
                    equipmentComponent.SwitchActive(EquipmentSlotGroup.QuickItem);
                    return CharacterCommandExecutionStatus.Executed;
                case EquipmentActionKind.UseQuickItem:
                    return TryUseActiveQuickItem()
                        ? CharacterCommandExecutionStatus.Executed
                        : CharacterCommandExecutionStatus.Invalid;
                case EquipmentActionKind.ToggleHandMode:
                    if (!movementComponent.Model.Grounded
                        || _runtime.MovementGate.IsSet(MovementGateReason.Manual)
                        || _runtime.MovementGate.IsSet(MovementGateReason.Animation)
                        || _runtime.MovementGate.IsSet(MovementGateReason.Spawn))
                    {
                        return CharacterCommandExecutionStatus.TemporarilyBlocked;
                    }

                    return equipmentComponent.TrySwitchHandMode(out _)
                        ? CharacterCommandExecutionStatus.Executed
                        : CharacterCommandExecutionStatus.Invalid;
                default:
                    return CharacterCommandExecutionStatus.Invalid;
            }
        }

        public CharacterCommandExecutionStatus TryAdvanceEquipmentAction() =>
            _equipmentSwapCoordinator.IsActive
                ? CharacterCommandExecutionStatus.TemporarilyBlocked
                : CharacterCommandExecutionStatus.Executed;

        public void OnAnimationStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);
            if (_equipmentSwapCoordinator.IsActive)
            {
                _equipmentSwapCoordinator.HandleAnimationState(
                    state,
                    equipmentComponent,
                    animatorComponent,
                    equipmentPresentation);
            }

            if (state.StateMachineName == StateMachineName.Spawn)
            {
                if (state.State == StateMachineState.Enter) _runtime.SetInputBlocked(true);
                else if (state.State == StateMachineState.Exit) _runtime.SetInputBlocked(false);
            }

            if (state.StateMachineName == StateMachineName.Parry)
            {
                if (state.State == StateMachineState.Enter) _runtime.SetParryLocked(true);
                else if (state.State == StateMachineState.Exit) _runtime.SetParryLocked(false);
            }

            if (state.State == StateMachineState.Progress
                && state.StateMachineName is StateMachineName.HeavyAttack
                    or StateMachineName.HeavyAttackAlt)
            {
                animatorComponent.SetChargedAttackSpeed(NORMAL_ATTACK_SPEED);
            }

            if (_animationAdapter.TryAdapt(state, out CharacterAnimationSignal signal))
            {
                if (!_runtime.HandleAnimation(signal, this))
                {
                    Debug.LogWarning(
                        $"Ignoring {signal.ActionState} animation signal while runtime is in "
                        + $"{_runtime.ActionState}.",
                        this);
                }

                ApplyRuntimeAnimationRequests();
            }
        }

        private void ApplyRuntimeAnimationRequests()
        {
            if (_runtime.TryConsumeRollSprintInterrupt())
            {
                animatorComponent.InterruptRollForSprint();
            }
        }

        public void SetMovementBlocked(bool blocked)
        {
            _runtime.SetMovementBlocked(blocked);
            movementComponent.SetMovementBlocked(_runtime.MovementGate.IsBlocked);
        }

        public void SetAnimationMotionContract(bool movementBlocked, bool useRootMotion)
        {
            _runtime.SetAnimationMotionContract(movementBlocked, useRootMotion);
            movementComponent.SetMovementBlocked(_runtime.MovementGate.IsBlocked);
        }

        public void ApplyRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            if (_runtime.CanApplyRootMotion)
            {
                movementComponent.ApplyAnimationMovement(deltaPosition, deltaRotation);
            }
        }

        public void SetLocomotion(float speed, Vector2 blendDirection) =>
            animatorComponent.SetLocomotion(speed, blendDirection);
        public void SetTurn(float turnAmount) => animatorComponent.SetTurn(turnAmount);
        public void SetGrounded(bool grounded) => animatorComponent.SetGrounded(grounded);
        public void NotifyLand() => characterAudioComponent.NotifyLand();
        public void SetAirborneMotion(float velocity, LandingType landingType) =>
            animatorComponent.SetAirborneMotion(velocity, landingType);
        public void PlayJump() => animatorComponent.SetJump();
        public void PlayRoll(Vector2 direction) => animatorComponent.TriggerRoll(direction);
        public void PlayBackStep() => animatorComponent.TriggerBackStep();
        public void SetCrouch(bool crouching) => animatorComponent.SetCrouch(crouching);

        private void OnDamageApplied(DamageResult damage)
        {
            if (damage.HealthDamageAmount <= 0f)
            {
                return;
            }

            characterAudioComponent.NotifyHit();
            if (!damage.Killed)
            {
                animatorComponent.TriggerHit();
            }
        }

        public void Heal(float amount) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateHeal(healthComponent.Stats, amount));

        public void GrantCurrency(int amount)
        {
            _heldCurrency = checked(_heldCurrency + amount);
        }

        public void Revive(float health) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateRevive(healthComponent.Stats, health));

        public void SetLockOnTarget(bool isLockedOn, long? lockOnTargetEntityId)
        {
            Transform lockOnTarget = null;
            if (isLockedOn)
            {
                IEntity targetEntity = _entityLocator.GetEntity(lockOnTargetEntityId.Value);
                targetEntity.TryGetComponent(out TargetingCommand targetingCommand);
                lockOnTarget = targetingCommand.TargetTransform;
            }

            movementComponent.SetLockOnTarget(isLockedOn, lockOnTarget);
            animatorComponent.SetLockOn(isLockedOn);
        }

        public void ApplyEquipmentLoadout(EquipmentLoadout loadout)
        {
            equipmentPresentation.ApplyLoadout(loadout);
            ItemId? rightWeaponId = ResolveAttackWeaponId(loadout, false);
            ItemId? leftWeaponId = ResolveAttackWeaponId(loadout, true);
            bool hasRightEquippedWeapon = rightWeaponId.HasValue
                && rightWeaponId != ItemId.Fist;
            bool hasLeftEquippedWeapon = leftWeaponId.HasValue
                && leftWeaponId != ItemId.Fist;
            AnimationProfile profile = hasRightEquippedWeapon
                ? _itemCatalog.GetWeapon(rightWeaponId.Value).AnimationProfile
                : hasLeftEquippedWeapon
                    ? _itemCatalog.GetWeapon(leftWeaponId.Value).AnimationProfile
                    : null;
            if (profile == null) animatorComponent.ResetAnimationProfile();
            else animatorComponent.ApplyAnimationProfile(
                profile,
                hasRightEquippedWeapon,
                hasLeftEquippedWeapon);

            animatorComponent.TransitionHandMode(loadout.HandMode);
            _attackComponent.SetActiveWeapons(
                rightWeaponId,
                equipmentPresentation.ActiveRightWeaponRuntime,
                leftWeaponId,
                equipmentPresentation.ActiveLeftWeaponRuntime,
                loadout.HandMode);
        }

        private bool TryUseActiveQuickItem()
        {
            EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
            if (quickItem == null) return false;
            ItemDefinition item = _itemCatalog.GetItem(quickItem.ItemId);
            if (item.ItemType != ItemType.Consumable)
            {
                throw new InvalidOperationException(
                    $"Quick-item slot contains non-consumable '{item.DisplayName}'.");
            }

            ConsumableDefinition consumable = _itemCatalog.GetConsumable(quickItem.ItemId);

            switch (consumable.UseType)
            {
                case ItemUseType.Heal:
                    Heal(consumable.EffectAmount);
                    break;
                case ItemUseType.GrantCurrency:
                    GrantCurrency(Mathf.RoundToInt(consumable.EffectAmount));
                    break;
                case ItemUseType.InfuseActiveWeapon:
                    WeaponRuntime runtime = equipmentPresentation.ActiveRightWeaponRuntime;
                    if (runtime == null) return false;
                    runtime.ApplyLightningInfusion(
                        consumable.EffectAmount,
                        consumable.DurationSeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(consumable.UseType), consumable.UseType, null);
            }

            inventoryComponent.Consume(quickItem.Entry.EntryId);
            return true;
        }

        private ItemId? GetWeaponId(EquippedItemContext context)
        {
            if (context == null)
            {
                return null;
            }

            return _itemCatalog.GetItem(context.ItemId).ItemType == ItemType.Weapon
                ? context.ItemId
                : null;
        }

        private ItemId? ResolveAttackWeaponId(
            EquipmentLoadout loadout,
            bool isLeftHand)
        {
            ItemId? leftWeaponId = GetWeaponId(loadout.EffectiveLeft);
            if (isLeftHand)
            {
                return leftWeaponId;
            }

            ItemId? rightWeaponId = GetWeaponId(loadout.EffectiveRight);
            return rightWeaponId.HasValue
                ? rightWeaponId
                : loadout.EffectiveRight == null && !leftWeaponId.HasValue
                    ? ItemId.Fist
                    : null;
        }

        private static float ResolveAttackStaminaCost(
            in AttackRequest request,
            CombatProfile combatProfile)
        {
            float baseCost = request.Intent == AttackIntent.Heavy
                || request.Intent == AttackIntent.Special
                    ? combatProfile.HeavyAttackStaminaCost
                    : combatProfile.LightAttackStaminaCost;
            return baseCost * combatProfile.StaminaCostMultiplier;
        }

        private static float ResolveAttackStaminaStartThreshold(
            in AttackRequest request,
            CombatProfile combatProfile)
        {
            return request.Intent == AttackIntent.Heavy
                || request.Intent == AttackIntent.Special
                    ? combatProfile.HeavyAttackStaminaStartThreshold
                    : combatProfile.LightAttackStaminaStartThreshold;
        }
    }
}
