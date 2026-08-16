using System;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using Prospector.Utility.Timer;
using SoulsLike.Services.CameraService;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class Character : MonoBehaviour, IInitializable, IComponentMediator
    {
        private enum EquipmentSwapPhase
        {
            None = 0,
            SwapOut = 1,
            SwapOutCompleted = 2,
            SwapIn = 3
        }

        private const float SPRINT_HOLD_THRESHOLD = 0.2f;

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

        private ICameraService _cameraService;
        private AttackComponent _attackComponent;
        private CharacterActionBuffer _actionBuffer;
        private ITimer _sprintHoldTimer;
        private bool _sprintHoldQualified;
        private bool _actionTransitionOpen;
        private bool _manualMovementBlocked;
        private bool _animationMovementBlocked;
        private bool _animationRootMotionEnabled;
        private EquipmentSlotGroup? _pendingEquipmentSwapGroup;
        private EquipmentSwapPhase _equipmentSwapPhase;

        public Transform CameraTarget => cameraTarget;
        public InventoryComponent InventoryComponent => inventoryComponent;
        public HealthStats HealthStats => healthComponent.Stats;
        public int HeldCurrency { get; private set; }
        public CharacterAttributeStats Attributes => attributes;
        public bool IsInputBlocked { get; private set; }

        [Inject]
        public void InjectDependencies(
            ICameraService cameraService,
            AttackComponent attackComponent,
            CharacterActionBuffer actionBuffer)
        {
            _cameraService = cameraService;
            _attackComponent = attackComponent;
            _actionBuffer = actionBuffer;
        }

        public void SetEquipmentPresentation(EquipmentPresentation equipmentPresentation)
        {
            this.equipmentPresentation = equipmentPresentation
                ?? throw new ArgumentNullException(nameof(equipmentPresentation));
        }

        public void Initialize()
        {
            movementComponent.SetMediator(this);
            animatorComponent.SetMediator(this);
            _attackComponent.SetMediator(this);
            equipmentComponent.SetMediator(this);
            healthComponent.SetMediator(this);
            if (equipmentPresentation == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires an {nameof(EquipmentPresentation)} component.");
            }

            animatorComponent.SetHandMode(equipmentComponent.Model.ActiveHandMode);
            NotifyEquipmentLoadoutChanged(equipmentComponent.BuildLoadout());
            _sprintHoldTimer = TimerFactory.ConstructTimer(SPRINT_HOLD_THRESHOLD);
            Cursor.lockState = CursorLockMode.Locked;
            IsInputBlocked = true;
            animatorComponent.TriggerSpawn();
        }

        public void UpdateBehaviour(ProjectInputActions.CharacterActions actions)
        {
            if (actions.Sprint.WasPressedThisFrame())
            {
                _sprintHoldQualified = false;
                _sprintHoldTimer
                    .ChangeDuration(SPRINT_HOLD_THRESHOLD)
                    .Start();
            }

            bool sprinting = actions.Sprint.IsPressed()
                && (_sprintHoldQualified || _sprintHoldTimer.IsComplete);

            if (sprinting)
            {
                _sprintHoldQualified = true;
            }

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            float cameraYaw = _cameraService.GetYaw();
            bool hasMovementInput = moveInput.sqrMagnitude > 0.0001f;
            bool canStartAttack = movementComponent.Model.Grounded
                && !_manualMovementBlocked
                && !_animationMovementBlocked;
            bool canBufferAttack = movementComponent.Model.Grounded
                && !_manualMovementBlocked;
            bool canBufferSpecialAttack = canBufferAttack;

            bool equipmentActionPerformed = TryAdvanceEquipmentSwap();
            if (!_attackComponent.IsActionActive && !_pendingEquipmentSwapGroup.HasValue)
            {
                if (actions.SwitchWeapon.WasPressedThisFrame())
                {
                    BeginEquipmentSwap(EquipmentSlotGroup.RightHandArmament);
                    equipmentActionPerformed = true;
                }
                else if (actions.SwitchShield.WasPressedThisFrame())
                {
                    equipmentComponent.SwitchActive(EquipmentSlotGroup.LeftHandArmament);
                    equipmentActionPerformed = true;
                }
                else if (actions.SwitchFlask.WasPressedThisFrame())
                {
                    equipmentComponent.SwitchActive(EquipmentSlotGroup.QuickItem);
                    equipmentActionPerformed = true;
                }
                else if (actions.UseItem.WasPressedThisFrame())
                {
                    equipmentActionPerformed = TryUseActiveQuickItem();
                }
            }

            if (actions.TwoHanded.WasPressedThisFrame()
                && canStartAttack
                && !_attackComponent.IsActionActive)
            {
                equipmentActionPerformed = equipmentComponent.TrySwitchHandMode(out _)
                    || equipmentActionPerformed;
            }

            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            bool hasRightWeapon = loadout.EffectiveRight?.Definition is WeaponDefinition;
            bool hasLeftWeapon = loadout.EffectiveLeft?.Definition is WeaponDefinition;
            bool attackCaptured = false;
            BufferedCharacterAction attackAction = default;
            if (!equipmentActionPerformed && (hasRightWeapon || !hasLeftWeapon))
            {
                attackCaptured = _attackComponent.TryCaptureAction(
                    actions,
                    sprinting && hasMovementInput,
                    canBufferAttack,
                    canBufferSpecialAttack,
                    out attackAction);
            }

            if (!equipmentActionPerformed
                && !attackCaptured
                && hasLeftWeapon
                && canBufferAttack
                && actions.Guard.WasPressedThisFrame())
            {
                attackAction = BufferedCharacterAction.Attack(
                    CharacterActionType.LightAttack,
                    false,
                    true);
                attackCaptured = true;
            }

            if (attackCaptured)
            {
                _actionBuffer.Buffer(attackAction);
            }

            if (!equipmentActionPerformed
                && actions.Roll.WasReleasedThisFrame()
                && !_sprintHoldQualified)
            {
                _actionBuffer.Buffer(BufferedCharacterAction.Roll(moveInput, cameraYaw));
            }

            if (!equipmentActionPerformed)
            {
                TryExecuteBufferedAction(
                    canStartAttack,
                    canBufferSpecialAttack,
                    _actionTransitionOpen && _attackComponent.IsActionActive);
            }

            movementComponent.Move(
                moveInput,
                cameraYaw,
                sprinting,
                actions.Jump.WasPressedThisFrame(),
                false,
                actions.Crouch.IsPressed());

            bool canBlock = !equipmentActionPerformed
                && !_pendingEquipmentSwapGroup.HasValue
                && loadout.EffectiveLeft?.Definition is ShieldDefinition
                && movementComponent.Model.Grounded
                && !_manualMovementBlocked
                && (!_animationMovementBlocked
                    || (_actionTransitionOpen && _attackComponent.IsActionActive));
            animatorComponent.SetWeaponBlock(actions.Guard.IsPressed() && canBlock);

            if (actions.Sprint.WasReleasedThisFrame())
            {
                _sprintHoldQualified = false;
                _sprintHoldTimer.Reset();
            }
        }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            DamageResult result = healthComponent.CalculateDamage(request, healthComponent.Stats);
            healthComponent.ApplyAuthoritativeStats(result.NewStats);
            healthComponent.NotifyDamageApplied(result);
            return result;
        }

        public void Heal(float amount)
        {
            HealthStats stats = healthComponent.CalculateHeal(healthComponent.Stats, amount);
            healthComponent.ApplyAuthoritativeStats(stats);
        }

        public void Revive(float health)
        {
            HealthStats stats = healthComponent.CalculateRevive(healthComponent.Stats, health);
            healthComponent.ApplyAuthoritativeStats(stats);
        }

        public void NotifyGrounded(bool isGrounded)
        {
            animatorComponent.SetGrounded(isGrounded);
        }

        public void NotifyHealthStatsChanged(HealthStats stats)
        {
            healthComponent.Model.ApplyStats(stats);
        }

        public void NotifyDamageApplied(DamageResult result)
        {
            healthComponent.Model.NotifyDamageApplied(result);
        }

        public void NotifyDeath()
        {
            healthComponent.Model.NotifyDeath();
        }

        public void NotifyLocomotion(float speed, Vector2 blendDirection)
        {
            animatorComponent.SetLocomotion(speed, blendDirection);
        }

        public void NotifyJump()
        {
            animatorComponent.SetJump();
        }

        public void NotifyRoll(Vector2 direction)
        {
            animatorComponent.TriggerRoll(direction);
        }

        public void NotifyBackStep()
        {
            animatorComponent.TriggerBackStep();
        }

        public void NotifyCrouch(bool isCrouching)
        {
            animatorComponent.SetCrouch(isCrouching);
        }
        
        public void SetLockOnTarget(bool isLockedOn, Transform lockOnTarget)
        {
            movementComponent.SetLockOnTarget(isLockedOn, lockOnTarget);
            animatorComponent.SetLockOn(isLockedOn);
        }

        public void NotifyTurn(float turnAmount)
        {
            animatorComponent.SetTurn(turnAmount);
        }

        public void NotifyAttack(AttackType attackType, bool isLeftHandAttack)
        {
            animatorComponent.PlayAttack(attackType, isLeftHandAttack);
        }

        public void SetChargedAttackSpeed(float speed)
        {
            animatorComponent.SetChargedAttackSpeed(speed);
        }

        public void NotifyAnimatorStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);
            HandleEquipmentSwapState(state);

            Debug.Log($"{state.StateMachineName}:{state.State}");
        if (state.StateMachineName == StateMachineName.Spawn)
            {
                if (state.State == StateMachineState.Enter)
                {
                    IsInputBlocked = true;
                }
                else if (state.State == StateMachineState.Exit)
                {
                    IsInputBlocked = false;
                }
            }

            if (state.State == StateMachineState.Enter)
            {
                _actionTransitionOpen = false;
            }
            else if (state.State == StateMachineState.QueueCheck)
            {
                Debug.Log("queue check");
                _actionTransitionOpen = true;
                TryExecuteBufferedAction(false, false, true);
            }
            else if (state.State == StateMachineState.Exit)
            {
                _actionTransitionOpen = false;
            }
        }

        public void SetMovementBlocked(bool blocked)
        {
            _manualMovementBlocked = blocked;
            SynchronizeMovementBlock();
        }

        public void NotifyAnimationMovement(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            if (_animationRootMotionEnabled)
            {
                movementComponent.ApplyAnimationMovement(deltaPosition, deltaRotation);
            }
        }

        public void SetAnimationMovementContract(bool movementBlocked, bool useRootMotion)
        {
            _animationMovementBlocked = movementBlocked;
            _animationRootMotionEnabled = useRootMotion;
            SynchronizeMovementBlock();
        }

        public void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier)
        {
            movementComponent.SetSpeedMultiplier(key, multiplier);
        }

        public void RemoveSpeedMultiplier(SpeedMultiplierKey key)
        {
            movementComponent.RemoveSpeedMultiplier(key);
        }

        public void NotifyEquipmentLoadoutChanged(EquipmentLoadout loadout)
        {
            equipmentPresentation.ApplyLoadout(loadout);

            //todo: instead of casting ItemDefinition to WeaponDefinition - get item type and id - then get WeaponDefinition,Don't use inheritance on WeaponDefinition->ItemDefinition
            WeaponDefinition rightWeapon = loadout.EffectiveRight?.Definition as WeaponDefinition;
            WeaponDefinition leftWeapon = loadout.EffectiveLeft?.Definition as WeaponDefinition;
            AnimationProfile animationProfile = rightWeapon?.AnimationProfile
                ?? leftWeapon?.AnimationProfile;
            if (animationProfile != null)
            {
                animatorComponent.ApplyAnimationProfile(
                    animationProfile,
                    rightWeapon != null,
                    leftWeapon != null);
            }
            else
            {
                animatorComponent.ResetAnimationProfile();
            }
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
            if (quickItem == null)
            {
                return false;
            }

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
                    WeaponRuntime weaponRuntime = equipmentPresentation.ActiveRightWeaponRuntime;
                    if (weaponRuntime == null)
                    {
                        return false;
                    }

                    weaponRuntime.ApplyLightningInfusion(
                        consumable.EffectAmount,
                        consumable.DurationSeconds);
                    break;
                case ItemUseType.None:
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(consumable.UseType),
                        consumable.UseType,
                        $"Consumable '{consumable.DisplayName}' has no supported use behavior.");
            }

            inventoryComponent.Consume(quickItem.Entry.EntryId);
            return true;
        }

        private void SynchronizeMovementBlock()
        {
            movementComponent.SetMovementBlocked(
                _manualMovementBlocked
                || _animationMovementBlocked
                || _pendingEquipmentSwapGroup.HasValue);
        }

        private void BeginEquipmentSwap(EquipmentSlotGroup group)
        {
            if (_pendingEquipmentSwapGroup.HasValue)
            {
                throw new InvalidOperationException("An equipment swap is already in progress.");
            }

            EquipmentSlotId previousActiveSlot = equipmentComponent.Model.GetActiveSlot(group);
            _pendingEquipmentSwapGroup = group;
            SynchronizeMovementBlock();

            if (animatorComponent.IsNoWeaponMode)
            {
                _equipmentSwapPhase = EquipmentSwapPhase.SwapIn;
                EquipmentSlotId activeSlot = equipmentComponent.SwitchActive(group);
                EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
                bool hasWeapon = loadout.EffectiveRight?.Definition is WeaponDefinition
                    || loadout.EffectiveLeft?.Definition is WeaponDefinition;

                if (activeSlot == previousActiveSlot || !hasWeapon)
                {
                    _equipmentSwapPhase = EquipmentSwapPhase.None;
                    _pendingEquipmentSwapGroup = null;
                    SynchronizeMovementBlock();
                    return;
                }

                animatorComponent.TriggerEquipmentSwapIn();
                return;
            }

            _equipmentSwapPhase = EquipmentSwapPhase.SwapOut;
            animatorComponent.TriggerEquipmentSwapOut();
        }

        private bool TryAdvanceEquipmentSwap()
        {
            if (_equipmentSwapPhase != EquipmentSwapPhase.SwapOutCompleted)
            {
                return false;
            }

            if (!_pendingEquipmentSwapGroup.HasValue)
            {
                throw new InvalidOperationException(
                    "Equipment swap-out completed without a pending equipment group.");
            }

            _equipmentSwapPhase = EquipmentSwapPhase.SwapIn;
            equipmentComponent.SwitchActive(_pendingEquipmentSwapGroup.Value);

            if (animatorComponent.IsNoWeaponMode)
            {
                _equipmentSwapPhase = EquipmentSwapPhase.None;
                _pendingEquipmentSwapGroup = null;
                SynchronizeMovementBlock();
                return true;
            }

            animatorComponent.TriggerEquipmentSwapIn();
            return true;
        }

        private void HandleEquipmentSwapState(AnimatorStateMachineDto state)
        {
            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            switch (state.StateMachineName)
            {
                case StateMachineName.EquipmentSwapOut:
                    if (_equipmentSwapPhase is EquipmentSwapPhase.SwapOutCompleted
                        or EquipmentSwapPhase.SwapIn)
                    {
                        return;
                    }

                    if (!_pendingEquipmentSwapGroup.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Equipment swap-out exited without a pending equipment group.");
                    }

                    if (_equipmentSwapPhase != EquipmentSwapPhase.SwapOut)
                    {
                        throw new InvalidOperationException(
                            $"Equipment swap-out exited during phase '{_equipmentSwapPhase}'.");
                    }

                    _equipmentSwapPhase = EquipmentSwapPhase.SwapOutCompleted;
                    break;
                case StateMachineName.EquipmentSwapIn:
                    if (_equipmentSwapPhase == EquipmentSwapPhase.None
                        && !_pendingEquipmentSwapGroup.HasValue)
                    {
                        return;
                    }

                    if (!_pendingEquipmentSwapGroup.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Equipment swap-in exited without a pending equipment group.");
                    }

                    if (_equipmentSwapPhase != EquipmentSwapPhase.SwapIn)
                    {
                        throw new InvalidOperationException(
                            $"Equipment swap-in exited during phase '{_equipmentSwapPhase}'.");
                    }

                    _equipmentSwapPhase = EquipmentSwapPhase.None;
                    _pendingEquipmentSwapGroup = null;
                    SynchronizeMovementBlock();
                    break;
            }
        }

        private void TryExecuteBufferedAction(
            bool canStartAttack,
            bool canUseSpecialAttack,
            bool canInterruptAnimation)
        {
            if (!_actionBuffer.TryPeek(
                out BufferedCharacterAction action,
                _attackComponent.IsActionActive))
            {
                return;
            }

            if (action.Type == CharacterActionType.Roll)
            {
                if (movementComponent.TryStartRoll(
                    action.MoveInput,
                    action.CameraYaw,
                    true,
                    canInterruptAnimation))
                {
                    _actionBuffer.Consume();
                    if (canInterruptAnimation)
                    {
                        _actionTransitionOpen = false;
                    }
                }

                return;
            }

            if (!movementComponent.Model.Grounded
                || (_attackComponent.IsActionActive && !canInterruptAnimation)
                || (!canInterruptAnimation && !canStartAttack)
                || (action.Type == CharacterActionType.SpecialAttack
                    && !canInterruptAnimation
                    && !canUseSpecialAttack))
            {
                return;
            }

            _attackComponent.ExecuteAction(action);
            _actionBuffer.Consume();
            if (canInterruptAnimation)
            {
                _actionTransitionOpen = false;
            }
        }

    }
}
