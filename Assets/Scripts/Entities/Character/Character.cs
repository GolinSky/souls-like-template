using System;
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
        IEquipmentLoadoutSink
    {
        private const float NORMAL_ATTACK_SPEED = 1.0f;

        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private AnimatorComponent animatorComponent;
        [SerializeField] private EquipmentComponent equipmentComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private InventoryComponent inventoryComponent;
        [SerializeField] private EquipmentPresentation equipmentPresentation;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CharacterAttributeStats attributes;

        [Header("Aim Settings")]
        [SerializeField, Min(0.1f)] private float aimTargetDistance = 100f;
        [SerializeField] private LayerMask aimLayerMask;

        private AttackComponent _attackComponent;
        private CharacterRuntime _runtime;
        private CharacterAnimationAdapter _animationAdapter;
        private EquipmentSwapCoordinator _equipmentSwapCoordinator;

        public Transform CameraTarget => cameraTarget;
        public InventoryComponent InventoryComponent => inventoryComponent;
        public HealthStats HealthStats => healthComponent.Stats;
        public int HeldCurrency { get; private set; }
        public CharacterAttributeStats Attributes => attributes;
        public bool IsInputBlocked => _runtime.IsInputBlocked;
        public CharacterActionStateId CurrentActionState => _runtime.ActionState;
        public bool IsEquipmentActionInProgress => _equipmentSwapCoordinator.IsActive;

        [Inject]
        public void ConfigureRuntime(
            AttackComponent attackComponent,
            CharacterRuntime runtime,
            CharacterAnimationAdapter animationAdapter,
            EquipmentSwapCoordinator equipmentSwapCoordinator,
            EquipmentPresentation presentation)
        {
            _attackComponent = attackComponent;
            _runtime = runtime;
            _animationAdapter = animationAdapter;
            _equipmentSwapCoordinator = equipmentSwapCoordinator;
            equipmentPresentation = presentation;
        }

        public void Initialize()
        {
            animatorComponent.SetHandMode(equipmentComponent.Model.ActiveHandMode);
            ApplyEquipmentLoadout(equipmentComponent.BuildLoadout());
            Cursor.lockState = CursorLockMode.Locked;
            _runtime.SetInputBlocked(true);
            animatorComponent.TriggerSpawn();
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
            _runtime.SetEquipmentSwapActive(_equipmentSwapCoordinator.IsActive);
            MovementPolicy policy = _runtime.ResolveMovementPolicy(false);
            movementComponent.SetMovementBlocked(policy.MovementBlocked);
            movementComponent.Move(
                input.ControlFrame.MoveInput,
                input.ControlFrame.CameraYaw,
                input.ControlFrame.SprintHeld,
                input.ControlFrame.CrouchHeld);

            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            bool blockRequested = input.ControlFrame.GuardHeld
                && policy.GuardAllowed
                && movementComponent.Model.Grounded;
            bool shieldBlock = blockRequested
                && loadout.HandMode == HandMode.OneHanded
                && loadout.EffectiveLeft?.Definition is ShieldDefinition;
            bool weaponBlock = blockRequested
                && loadout.EffectiveLeft == null
                && loadout.EffectiveRight?.Definition is WeaponDefinition;
            animatorComponent.SetShieldBlock(shieldBlock);
            animatorComponent.SetWeaponBlock(weaponBlock);
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
                || _runtime.MovementGate.IsSet(MovementGateReason.EquipmentSwap)
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
            bool hasRightWeapon = loadout.EffectiveRight?.Definition is WeaponDefinition;
            bool hasLeftWeapon = loadout.EffectiveLeft?.Definition is WeaponDefinition;
            if ((request.IsLeftHand && !hasLeftWeapon)
                || (!request.IsLeftHand && !hasRightWeapon && hasLeftWeapon))
            {
                return CharacterCommandExecutionStatus.Invalid;
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
            return movementComponent.TryStartRoll(
                request.MoveInput,
                request.CameraYaw,
                true,
                canInterrupt)
                ? CharacterCommandExecutionStatus.Executed
                : CharacterCommandExecutionStatus.TemporarilyBlocked;
        }

        public CharacterCommandExecutionStatus TryStartJump(in JumpRequest request) =>
            movementComponent.TryStartJump(true, request.IsSprinting)
                ? CharacterCommandExecutionStatus.Executed
                : CharacterCommandExecutionStatus.TemporarilyBlocked;

        public CharacterCommandExecutionStatus TryStartEquipmentAction(
            in EquipmentActionRequest request)
        {
            switch (request.Kind)
            {
                case EquipmentActionKind.SwitchRightWeapon:
                    return _equipmentSwapCoordinator.StartRightHandSwap(
                        equipmentComponent,
                        animatorComponent);
                case EquipmentActionKind.SwitchLeftWeapon:
                    equipmentComponent.SwitchActive(EquipmentSlotGroup.LeftHandArmament);
                    return CharacterCommandExecutionStatus.Executed;
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
            _equipmentSwapCoordinator.TryAdvance(
                equipmentComponent,
                animatorComponent);

        public void OnAnimationStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);
            if (_equipmentSwapCoordinator.IsActive)
            {
                _equipmentSwapCoordinator.HandleAnimationState(state);
            }

            if (state.StateMachineName == StateMachineName.Spawn)
            {
                if (state.State == StateMachineState.Enter) _runtime.SetInputBlocked(true);
                else if (state.State == StateMachineState.Exit) _runtime.SetInputBlocked(false);
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
        public void SetAirborneMotion(float velocity, LandingType landingType) =>
            animatorComponent.SetAirborneMotion(velocity, landingType);
        public void PlayJump() => animatorComponent.SetJump();
        public void PlayRoll(Vector2 direction) => animatorComponent.TriggerRoll(direction);
        public void PlayBackStep() => animatorComponent.TriggerBackStep();
        public void SetCrouch(bool crouching) => animatorComponent.SetCrouch(crouching);

        public DamageResult ApplyDamage(DamageRequest request)
        {
            DamageResult result = healthComponent.CalculateDamage(request, healthComponent.Stats);
            healthComponent.ApplyAuthoritativeStats(result.NewStats);
            healthComponent.NotifyDamageApplied(result);
            return result;
        }

        public void Heal(float amount) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateHeal(healthComponent.Stats, amount));

        public void Revive(float health) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateRevive(healthComponent.Stats, health));

        public void SetLockOnTarget(bool isLockedOn, Transform lockOnTarget)
        {
            movementComponent.SetLockOnTarget(isLockedOn, lockOnTarget);
            animatorComponent.SetLockOn(isLockedOn);
        }

        public void ApplyEquipmentLoadout(EquipmentLoadout loadout)
        {
            equipmentPresentation.ApplyLoadout(loadout);
            WeaponDefinition rightWeapon = loadout.EffectiveRight?.Definition as WeaponDefinition;
            WeaponDefinition leftWeapon = loadout.EffectiveLeft?.Definition as WeaponDefinition;
            AnimationProfile profile = rightWeapon?.AnimationProfile ?? leftWeapon?.AnimationProfile;
            if (profile == null) animatorComponent.ResetAnimationProfile();
            else animatorComponent.ApplyAnimationProfile(
                profile,
                rightWeapon != null,
                leftWeapon != null);

            animatorComponent.TransitionHandMode(loadout.HandMode);
            _attackComponent.SetActiveWeapons(
                rightWeapon,
                equipmentPresentation.ActiveRightWeaponRuntime,
                leftWeapon,
                equipmentPresentation.ActiveLeftWeaponRuntime,
                loadout.HandMode);
        }

        private bool TryUseActiveQuickItem()
        {
            EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
            if (quickItem == null) return false;
            if (quickItem.Definition is not ConsumableDefinition consumable)
            {
                throw new InvalidOperationException(
                    $"Quick-item slot contains non-consumable '{quickItem.Definition.DisplayName}'.");
            }

            switch (consumable.UseType)
            {
                case ItemUseType.Heal:
                    Heal(consumable.EffectAmount);
                    break;
                case ItemUseType.GrantCurrency:
                    HeldCurrency = checked(HeldCurrency + Mathf.RoundToInt(consumable.EffectAmount));
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
    }
}
