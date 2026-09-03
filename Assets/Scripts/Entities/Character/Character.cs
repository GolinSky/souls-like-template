using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using SoulsLike.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public sealed class Character : MonoBehaviour, IInitializable, IDisposable
    {
        private const float NORMAL_ATTACK_SPEED = 1.0f;

        private enum GracePhase
        {
            None,
            Unblock,
            RestStart,
            RestIdle,
            RestEnd
        }

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

        private AttackComponent _attackComponent;
        private readonly CharacterActionStateMachine _actionStateMachine = new CharacterActionStateMachine();
        private ItemCatalog _itemCatalog;
        private IEntityLocator _entityLocator;
        private ICombatStateNotifier _combatStateNotifier;
        private CombatDefenseComponent _combatDefense;
        private PlayerMeleeCombatRelay _meleeCombatRelay;
        private CriticalAttackController _criticalAttackController;
        private CharacterData _characterData;
        private int _heldCurrency;
        private bool _isDeathAnimationPlaying;
        private UniTaskCompletionSource<bool> _graceTransitionCompletionSource;
        private GracePhase _gracePhase;
        private MovementLockReason _movementLockReasons;
        private bool _isItemUseInProgress;
        private bool _hasItemUseProgressFired;
        private InventoryEntryId _activeItemEntryId;
        private ItemId _activeItemId;
        private ConsumableDefinition _activeConsumable;

        public Transform CameraTarget => cameraTarget;
        public bool IsGrounded => movementComponent.Model.Grounded;
        public float VerticalVelocity => movementComponent.VerticalVelocity;
        public InventoryComponent InventoryComponent => inventoryComponent;
        public HealthStats HealthStats => healthComponent.Stats;
        public int HeldCurrency => _heldCurrency;
        public CharacterAttributeStats Attributes => _characterData.Attributes;
        public bool IsInputBlocked => _actionStateMachine.IsInputBlocked;
        public CharacterAction.State CurrentActionState => _actionStateMachine.CurrentState;
        public event Action OnDeathAnimationCompleted;

        [Inject]
        public void Configure(
            AttackComponent attackComponent,
            EquipmentPresentation presentation,
            ItemCatalog itemCatalog,
            IEntityLocator entityLocator,
            ICombatStateNotifier combatStateNotifier,
            CharacterData characterData,
            CombatDefenseComponent combatDefense,
            PlayerMeleeCombatRelay meleeCombatRelay,
            CriticalAttackController criticalAttackController)
        {
            _attackComponent = attackComponent;
            equipmentPresentation = presentation;
            _itemCatalog = itemCatalog;
            _entityLocator = entityLocator;
            _combatStateNotifier = combatStateNotifier;
            _characterData = characterData;
            _combatDefense = combatDefense;
            _meleeCombatRelay = meleeCombatRelay;
            _criticalAttackController = criticalAttackController;
            _heldCurrency = characterData.StartingCurrency;
        }

        public void Initialize()
        {
            healthComponent.Model.OnDamageApplied += OnDamageApplied;
            _combatDefense.OnHitResolved += OnHitResolved;
            _criticalAttackController.OnCompleted += OnCriticalCompleted;
            movementComponent.Initialize();
            animatorComponent.SetHandMode(equipmentComponent.Model.ActiveHandMode);
            ApplyEquipmentLoadout(equipmentComponent.BuildLoadout());
            ApplyMovementPresentation();
            Cursor.lockState = CursorLockMode.Locked;
            SetInputBlocked(true);
            animatorComponent.TriggerSpawn();
        }

        public void Dispose()
        {
            healthComponent.Model.OnDamageApplied -= OnDamageApplied;
            _combatDefense.OnHitResolved -= OnHitResolved;
            _criticalAttackController.OnCompleted -= OnCriticalCompleted;
        }

        public void Tick(in CharacterInput input)
        {
            float now = Time.time;
            _attackComponent.SetStrongAttackHeld(input.StrongAttackHeld);
            if (!input.StrongAttackHeld)
            {
                animatorComponent.SetChargedAttackSpeed(NORMAL_ATTACK_SPEED);
            }

            _actionStateMachine.Tick(input.SprintHeld, equipmentComponent.IsSwapInProgress);
            _criticalAttackController.UpdateNeutralEligibility(
                _actionStateMachine.CurrentState == CharacterAction.State.Neutral
                && !_combatDefense.IsInHitReaction
                && !_combatDefense.IsParryStunned
                && !_combatDefense.IsInCriticalState);
            Submit(input.FirstAction, now);
            Submit(input.SecondAction, now);
            _actionStateMachine.PruneExpiredBuffer(now);
            TryExecuteBufferedAction(now);
            ApplyActionStateMachineRequests();
            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            bool blockRequested = input.GuardHeld
                && CanGuard()
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
                && input.SprintHeld
                && !input.CrouchHeld;
            float sprintStaminaCost =
                movementModel.CombatSprintStaminaDrainPerSecond * Time.deltaTime;
            bool sprintAllowed = !combatSprintDrainsStamina
                || healthComponent.CanConsumeStamina(
                    sprintStaminaCost,
                    movementModel.CombatSprintStaminaStartThreshold);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
            movementComponent.Move(
                input.MoveInput,
                input.CameraYaw,
                input.SprintHeld && sprintAllowed,
                input.CrouchHeld);
            characterAudioComponent.Tick(
                movementComponent.IsMoving,
                input.SprintHeld
                && sprintAllowed
                && !input.CrouchHeld);

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
            _combatDefense.SetBlocking(shieldBlock || weaponBlock);
            _combatDefense.TickRecovery(Time.deltaTime);
            healthComponent.TickStaminaRecovery(Time.deltaTime, shieldBlock || weaponBlock);
            ApplyMovementPresentation();
        }

        public void PlayDeath()
        {
            equipmentComponent.CancelSwap();
            _isDeathAnimationPlaying = true;
            SetInputBlocked(true);
            animatorComponent.TriggerDeath();
        }

        public void CompleteDeathAnimation()
        {
            animatorComponent.CompleteDeathAnimation();
            SetInputBlocked(false);
        }

        public async UniTask PlayGraceUnblock(CancellationToken token)
        {
            BeginGraceTransition(GracePhase.Unblock);
            animatorComponent.TriggerGraceUnblock();

            try
            {
                await _graceTransitionCompletionSource.Task.AttachExternalCancellation(token);
            }
            finally
            {
                if (_gracePhase == GracePhase.Unblock)
                {
                    CompleteGraceTransition();
                }
            }
        }

        public async UniTask EnterGraceRest(CancellationToken token)
        {
            BeginGraceTransition(GracePhase.RestStart);
            animatorComponent.TriggerGraceRestStart();

            try
            {
                await _graceTransitionCompletionSource.Task.AttachExternalCancellation(token);
            }
            finally
            {
                if (_gracePhase == GracePhase.RestStart)
                {
                    CompleteGraceTransition();
                }
            }
        }

        public void EnterGraceRestIdle()
        {
            _gracePhase = GracePhase.RestIdle;
            SetGraceProtection(true);
            animatorComponent.EnterGraceRestIdle();
        }

        public void CancelGraceRest()
        {
            if (_gracePhase is GracePhase.RestStart or GracePhase.RestIdle)
            {
                CompleteGraceTransition();
            }
        }

        public async UniTask ExitGraceRest(CancellationToken token)
        {
            _gracePhase = GracePhase.RestEnd;
            _graceTransitionCompletionSource = new UniTaskCompletionSource<bool>();
            SetGraceProtection(true);
            animatorComponent.TriggerGraceRestEnd();

            try
            {
                await _graceTransitionCompletionSource.Task.AttachExternalCancellation(token);
            }
            finally
            {
                if (_gracePhase == GracePhase.RestEnd)
                {
                    CompleteGraceTransition();
                }
            }
        }

        private CharacterAction.Result StartAttack(in CharacterAction action)
        {
            bool canInterrupt = _actionStateMachine.CurrentState is CharacterAction.State.Attack or CharacterAction.State.Roll or CharacterAction.State.BlockHit;
            if (!movementComponent.Model.Grounded
                || IsMovementLocked(MovementLockReason.Manual)
                || IsMovementLocked(MovementLockReason.Spawn)
                || (IsMovementLocked(MovementLockReason.Animation) && !canInterrupt))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            if (action.Intent == CharacterAction.AttackIntent.Special
                && _actionStateMachine.CurrentState == CharacterAction.State.Roll)
            {
                return CharacterAction.Result.Invalid;
            }

            EquipmentLoadout loadout = equipmentComponent.BuildLoadout();
            if (action.Intent == CharacterAction.AttackIntent.Special
                && loadout.HandMode == HandMode.OneHanded
                && loadout.EffectiveLeft != null
                && _itemCatalog.GetItem(loadout.EffectiveLeft.ItemId).ItemType == ItemType.Shield)
            {
                animatorComponent.TriggerParry();
                _actionStateMachine.SetInputBlocked(true);
                SetMovementLock(MovementLockReason.Parry, true);
                return CharacterAction.Result.Executed;
            }

            ItemId? rightWeaponId = ResolveAttackWeaponId(loadout, false);
            ItemId? leftWeaponId = ResolveAttackWeaponId(loadout, true);
            bool hasRightWeapon = rightWeaponId.HasValue;
            bool hasLeftWeapon = leftWeaponId.HasValue;
            if ((action.IsLeftHand && !hasLeftWeapon)
                || (!action.IsLeftHand && !hasRightWeapon))
            {
                return CharacterAction.Result.Invalid;
            }

            ItemId? weaponId = action.IsLeftHand
                ? leftWeaponId
                : rightWeaponId;
            if (weaponId.HasValue)
            {
                CombatProfile combatProfile = _itemCatalog.GetWeapon(weaponId.Value).CombatProfile;
                float staminaCost = ResolveAttackStaminaCost(action, combatProfile);
                float staminaStartThreshold = ResolveAttackStaminaStartThreshold(action, combatProfile);
                if (!healthComponent.CanConsumeStamina(staminaCost, staminaStartThreshold))
                {
                    return CharacterAction.Result.TemporarilyBlocked;
                }

                healthComponent.ConsumeStamina(staminaCost);
            }

            AttackExecutionContext context = _attackComponent.CurrentExecutionContext;
            AttackResolution resolution = _attackComponent.ResolveAttack(action, context);
            animatorComponent.SetChargedAttackSpeed(resolution.ChargedSpeed);
            movementComponent.FaceInputDirection(action.MoveInput, action.CameraYaw);
            animatorComponent.PlayAttack(
                resolution.AttackType,
                resolution.IsLeftHandAttack);
            return CharacterAction.Result.Executed;
        }

        private CharacterAction.Result StartRoll(in CharacterAction action)
        {
            bool canInterrupt = _actionStateMachine.CurrentState != CharacterAction.State.Neutral;
            MovementModel movementModel = movementComponent.Model;
            float staminaCost = movementModel.RollStaminaCost;
            if (!healthComponent.CanConsumeStamina(
                    staminaCost,
                    movementModel.RollStaminaStartThreshold))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            if (!movementComponent.TryStartRoll(
                    action.MoveInput,
                    action.CameraYaw,
                    true,
                    canInterrupt))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            healthComponent.ConsumeStamina(staminaCost);
            if (movementComponent.TryConsumeBackStepStarted()) animatorComponent.TriggerBackStep();
            else if (movementComponent.TryConsumeRollStarted(out Vector2 direction)) animatorComponent.TriggerRoll(direction);
            return CharacterAction.Result.Executed;
        }

        private CharacterAction.Result StartJump(in CharacterAction action)
        {
            MovementModel movementModel = movementComponent.Model;
            float staminaCost = movementModel.JumpStaminaCost;
            if (!healthComponent.CanConsumeStamina(
                    staminaCost,
                    movementModel.JumpStaminaStartThreshold))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            if (!movementComponent.TryStartJump(true, action.IsSprinting))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            healthComponent.ConsumeStamina(staminaCost);
            if (movementComponent.TryConsumeJumpStarted()) animatorComponent.SetJump();
            return CharacterAction.Result.Executed;
        }

        private CharacterAction.Result StartEquipmentAction(in CharacterAction action)
        {
            switch (action.EquipmentAction)
            {
                case CharacterAction.EquipmentKind.SwitchRightWeapon:
                    return equipmentComponent.StartSwap(EquipmentSlotGroup.RightHandArmament);
                case CharacterAction.EquipmentKind.SwitchLeftWeapon:
                    return equipmentComponent.StartSwap(EquipmentSlotGroup.LeftHandArmament);
                case CharacterAction.EquipmentKind.SwitchQuickItem:
                    equipmentComponent.SwitchActive(EquipmentSlotGroup.QuickItem);
                    return CharacterAction.Result.Executed;
                case CharacterAction.EquipmentKind.UseQuickItem:
                    return StartUseQuickItem();
                case CharacterAction.EquipmentKind.ToggleHandMode:
                    if (!movementComponent.Model.Grounded
                        || IsMovementLocked(MovementLockReason.Manual)
                        || IsMovementLocked(MovementLockReason.Animation)
                        || IsMovementLocked(MovementLockReason.Spawn))
                    {
                        return CharacterAction.Result.TemporarilyBlocked;
                    }

                    return equipmentComponent.TrySwitchHandMode(out _)
                        ? CharacterAction.Result.Executed
                        : CharacterAction.Result.Invalid;
                default:
                    return CharacterAction.Result.Invalid;
            }
        }

        public void OnAnimationStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);
            if (equipmentComponent.IsSwapInProgress) equipmentComponent.HandleAnimationState(state);

            if (state.StateMachineName == StateMachineName.Spawn)
            {
                if (state.State == StateMachineState.Enter) SetInputBlocked(true);
                else if (state.State == StateMachineState.Exit && _gracePhase == GracePhase.None)
                    SetInputBlocked(false);
            }

            HandleGraceAnimationState(state);

            if (state.StateMachineName == StateMachineName.Death
                && state.State == StateMachineState.Exit
                && _isDeathAnimationPlaying)
            {
                _isDeathAnimationPlaying = false;
                OnDeathAnimationCompleted?.Invoke();
            }

            if (state.StateMachineName == StateMachineName.Parry)
            {
                if (state.State == StateMachineState.Enter) { _actionStateMachine.SetInputBlocked(true); SetMovementLock(MovementLockReason.Parry, true); }
                else if (state.State == StateMachineState.QueueCheck) { _actionStateMachine.SetInputBlocked(false); SetMovementLock(MovementLockReason.Parry, false); }
                else if (state.State == StateMachineState.Exit) { _actionStateMachine.SetInputBlocked(false); SetMovementLock(MovementLockReason.Parry, false); }
            }

            if (state.StateMachineName == StateMachineName.HitReaction
                && state.State is StateMachineState.Enter or StateMachineState.Exit)
            {
                _combatDefense.SetHitReaction(state.State == StateMachineState.Enter);
            }

            if (state.StateMachineName == StateMachineName.ParryStun
                && state.State is StateMachineState.Enter or StateMachineState.Exit)
            {
                _combatDefense.SetParryStunned(state.State == StateMachineState.Enter);
            }

            if (state.State == StateMachineState.Progress
                && state.StateMachineName is StateMachineName.HeavyAttack
                    or StateMachineName.HeavyAttackAlt)
            {
                animatorComponent.SetChargedAttackSpeed(NORMAL_ATTACK_SPEED);
            }

            if (state.StateMachineName is StateMachineName.ItemDrink or StateMachineName.ItemDrinkEmpty)
            {
                if (state.State == StateMachineState.Progress
                    && state.StateMachineName == StateMachineName.ItemDrink
                    && !_hasItemUseProgressFired)
                {
                    _hasItemUseProgressFired = true;
                    inventoryComponent.Consume(_activeItemEntryId, 1);
                    Heal(_activeConsumable.EffectAmount);
                }
                else if (state.State == StateMachineState.Exit && _isItemUseInProgress)
                {
                    _isItemUseInProgress = false;
                    movementComponent.RemoveSpeedMultiplier(SpeedMultiplierKey.ItemUse);
                    equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.RightHandArmament, true);
                }
            }

            if ((state.StateMachineName == StateMachineName.HitReaction
                 || state.StateMachineName == StateMachineName.Death
                 || state.StateMachineName == StateMachineName.GraceRestStart)
                && state.State == StateMachineState.Enter
                && _isItemUseInProgress)
            {
                _isItemUseInProgress = false;
                movementComponent.RemoveSpeedMultiplier(SpeedMultiplierKey.ItemUse);
                equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.RightHandArmament, true);
            }

            if (TryResolveActionState(state.StateMachineName, out CharacterAction.State actionState))
            {
                bool handled = state.State switch
                {
                    StateMachineState.Enter => _actionStateMachine.HandleEntered(actionState),
                    StateMachineState.QueueCheck => _actionStateMachine.HandleQueueCheck(actionState),
                    StateMachineState.Exit => _actionStateMachine.HandleExited(actionState),
                    StateMachineState.Progress => true,
                    StateMachineState.Loop => true,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(state.State), state.State, null)
                };
                if (!handled)
                {
                    Debug.LogWarning(
                        $"Ignoring {actionState} animation signal while state machine is in "
                        + $"{_actionStateMachine.CurrentState}.",
                        this);
                }

                if (handled && state.State == StateMachineState.QueueCheck)
                {
                    TryExecuteBufferedAction(Time.time);
                    ApplyActionStateMachineRequests();
                }
            }
        }

        private void ApplyActionStateMachineRequests()
        {
            if (_actionStateMachine.TryConsumeRollSprintInterrupt())
            {
                animatorComponent.InterruptRollForSprint();
            }
        }

        private void BeginGraceTransition(GracePhase phase)
        {
            _gracePhase = phase;
            _graceTransitionCompletionSource = new UniTaskCompletionSource<bool>();
            SetGraceProtection(true);
        }

        private void HandleGraceAnimationState(AnimatorStateMachineDto state)
        {
            if (state.StateMachineName == StateMachineName.GraceUnblock
                && state.State == StateMachineState.Exit
                && _gracePhase == GracePhase.Unblock)
            {
                CompleteGraceTransition();
                return;
            }

            if (state.StateMachineName == StateMachineName.GraceRestIdle
                && state.State == StateMachineState.Enter
                && _gracePhase == GracePhase.RestStart)
            {
                _gracePhase = GracePhase.RestIdle;
                _graceTransitionCompletionSource.TrySetResult(true);
                return;
            }

            if (state.StateMachineName == StateMachineName.GraceRestEnd
                && state.State == StateMachineState.Exit
                && _gracePhase == GracePhase.RestEnd)
            {
                CompleteGraceTransition();
            }
        }

        private void CompleteGraceTransition()
        {
            _gracePhase = GracePhase.None;
            SetGraceProtection(false);
            _graceTransitionCompletionSource.TrySetResult(true);
        }

        private void SetGraceProtection(bool isProtected)
        {
            SetInputBlocked(isProtected);
            healthComponent.SetInvulnerable(isProtected);
        }

        public void SetMovementBlocked(bool blocked)
        {
            SetMovementLock(MovementLockReason.Manual, blocked);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
        }

        public void SetAnimationMotionContract(bool movementBlocked)
        {
            SetMovementLock(MovementLockReason.Animation, movementBlocked);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
        }

        private void OnDamageApplied(DamageResult damage)
        {
            if (damage.HealthDamageAmount <= 0f)
            {
                return;
            }

            characterAudioComponent.NotifyHit();
        }

        private void OnHitResolved(MeleeHitResult result)
        {
            if (result.Type is MeleeHitResultType.Ignored
                or MeleeHitResultType.Invulnerable
                or MeleeHitResultType.Parried
                or MeleeHitResultType.Killed)
            {
                return;
            }

            if (result.Type is MeleeHitResultType.PoiseStaggered
                or MeleeHitResultType.StanceBroken
                or MeleeHitResultType.GuardBroken)
            {
                _combatDefense.SetHitReaction(true);
                _meleeCombatRelay.Cancel();
            }

            animatorComponent.TriggerHit(result);
        }

        public void Heal(float amount) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateHeal(healthComponent.Stats, amount));

        public void GrantCurrency(int amount)
        {
            _heldCurrency = checked(_heldCurrency + amount);
        }

        public void Revive(float health) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateRevive(healthComponent.Stats, health));

        public void SetPosition(Vector3 position) => movementComponent.SetPosition(position);

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

        private CharacterAction.Result StartUseQuickItem()
        {
            if (!movementComponent.Model.Grounded
                || IsMovementLocked(MovementLockReason.Manual)
                || IsMovementLocked(MovementLockReason.Spawn)
                || IsMovementLocked(MovementLockReason.Critical))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
            if (quickItem == null) return CharacterAction.Result.Invalid;

            ItemDefinition item = _itemCatalog.GetItem(quickItem.ItemId);
            if (item.ItemType != ItemType.Consumable)
            {
                throw new InvalidOperationException(
                    $"Quick-item slot contains non-consumable '{item.DisplayName}'.");
            }

            ConsumableDefinition consumable = _itemCatalog.GetConsumable(quickItem.ItemId);
            if (quickItem.ItemId == ItemId.CrimsonFlask)
            {
                _isItemUseInProgress = true;
                _hasItemUseProgressFired = false;
                _activeItemEntryId = quickItem.Entry.EntryId;
                _activeItemId = quickItem.ItemId;
                _activeConsumable = consumable;

                equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.RightHandArmament, false);
                movementComponent.SetSpeedMultiplier(SpeedMultiplierKey.ItemUse, 0.35f);

                if (quickItem.Entry.Quantity > 0)
                {
                    animatorComponent.TriggerItemDrink();
                }
                else
                {
                    animatorComponent.TriggerItemDrinkEmpty();
                }

                return CharacterAction.Result.Executed;
            }

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
                    if (runtime == null) return CharacterAction.Result.Invalid;
                    runtime.ApplyLightningInfusion(
                        consumable.EffectAmount,
                        consumable.DurationSeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(consumable.UseType), consumable.UseType, null);
            }

            inventoryComponent.Consume(quickItem.Entry.EntryId);
            return CharacterAction.Result.Executed;
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
            in CharacterAction action,
            CombatProfile combatProfile)
        {
            float baseCost = action.Intent == CharacterAction.AttackIntent.Heavy
                || action.Intent == CharacterAction.AttackIntent.Special
                    ? combatProfile.HeavyAttackStaminaCost
                    : combatProfile.LightAttackStaminaCost;
            return baseCost * combatProfile.StaminaCostMultiplier;
        }

        private static float ResolveAttackStaminaStartThreshold(
            in CharacterAction action,
            CombatProfile combatProfile)
        {
            return action.Intent == CharacterAction.AttackIntent.Heavy
                || action.Intent == CharacterAction.AttackIntent.Special
                    ? combatProfile.HeavyAttackStaminaStartThreshold
                    : combatProfile.LightAttackStaminaStartThreshold;
        }

        private void Submit(CharacterAction? action, float now)
        {
            if (!action.HasValue || !_actionStateMachine.TryDispatch(action.Value, now)) return;
            ExecuteAction(action.Value, false, now);
        }

        private void TryExecuteBufferedAction(float now)
        {
            if (_actionStateMachine.TryGetBufferedAction(out CharacterAction action)) ExecuteAction(action, true, now);
        }

        private void ExecuteAction(in CharacterAction action, bool buffered, float now)
        {
            if (!buffered
                && action.ActionKind == CharacterAction.Kind.Attack
                && action.Intent == CharacterAction.AttackIntent.Light
                && !action.IsLeftHand
                && _actionStateMachine.CurrentState == CharacterAction.State.Neutral
                && !_actionStateMachine.HasBufferedAction
                && _criticalAttackController.TryStart())
            {
                _actionStateMachine.EnterCritical();
                _actionStateMachine.SetInputBlocked(true);
                SetMovementLock(MovementLockReason.Critical, true);
                movementComponent.SetMovementBlocked(true);
                return;
            }

            CharacterAction.Result result = action.ActionKind switch
            {
                CharacterAction.Kind.Attack => StartAttack(action),
                CharacterAction.Kind.Roll => StartRoll(action),
                CharacterAction.Kind.Jump => StartJump(action),
                CharacterAction.Kind.Equipment => StartEquipmentAction(action),
                _ => throw new ArgumentOutOfRangeException()
            };
            CharacterAction.State state = action.ActionKind switch
            {
                CharacterAction.Kind.Attack => CharacterAction.State.Attack,
                CharacterAction.Kind.Roll => CharacterAction.State.Roll,
                CharacterAction.Kind.Equipment when equipmentComponent.IsSwapInProgress => CharacterAction.State.EquipmentSwap,
                CharacterAction.Kind.Equipment when _isItemUseInProgress => CharacterAction.State.ItemUse,
                _ => CharacterAction.State.Neutral
            };
            if (buffered) _actionStateMachine.ReportBufferedExecution(result, state);
            else _actionStateMachine.ReportExecution(action, result, state, now);
        }

        private void OnCriticalCompleted()
        {
            animatorComponent.ClearGroundedOverride();
            _actionStateMachine.CompleteCritical();
            _actionStateMachine.SetInputBlocked(false);
            SetMovementLock(MovementLockReason.Critical, false);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
        }

        private bool CanGuard() => _movementLockReasons == MovementLockReason.None || (_movementLockReasons == MovementLockReason.Animation && _actionStateMachine.CanGuardDuringAnimationBlock);
        private void SetInputBlocked(bool blocked)
        {
            _actionStateMachine.SetInputBlocked(blocked);
            SetMovementLock(MovementLockReason.Spawn, blocked);
        }
        private bool IsMovementLocked(MovementLockReason reason) => (_movementLockReasons & reason) != 0;
        private void SetMovementLock(MovementLockReason reason, bool value)
        {
            if (value) _movementLockReasons |= reason;
            else _movementLockReasons &= ~reason;
        }

        private void ApplyMovementPresentation()
        {
            MovementComponent.MovementPresentation presentation = movementComponent.Presentation;
            animatorComponent.SetLocomotion(presentation.Speed, presentation.BlendDirection);
            animatorComponent.SetTurn(presentation.TurnAmount);
            animatorComponent.SetGrounded(presentation.Grounded);
            animatorComponent.SetAirborneMotion(presentation.VerticalVelocity, presentation.LandingType);
            animatorComponent.SetCrouch(presentation.Crouching);
            if (movementComponent.TryConsumeLanded()) characterAudioComponent.NotifyLand();
        }

        private static bool TryResolveActionState(StateMachineName stateMachineName, out CharacterAction.State state)
        {
            switch (stateMachineName)
            {
                case StateMachineName.LightAttack:
                case StateMachineName.LightAttackAlt:
                case StateMachineName.HeavyAttack:
                case StateMachineName.HeavyAttackAlt:
                case StateMachineName.RollAttack:
                case StateMachineName.BackStepAttack:
                case StateMachineName.RunAttack:
                case StateMachineName.SpecialAttack:
                case StateMachineName.Parry:
                    state = CharacterAction.State.Attack;
                    return true;
                case StateMachineName.Roll:
                case StateMachineName.BackStep:
                    state = CharacterAction.State.Roll;
                    return true;
                case StateMachineName.EquipmentSwapOut:
                case StateMachineName.EquipmentSwapIn:
                    state = CharacterAction.State.EquipmentSwap;
                    return true;
                case StateMachineName.ItemDrink:
                case StateMachineName.ItemDrinkEmpty:
                    state = CharacterAction.State.ItemUse;
                    return true;
                case StateMachineName.BlockHit:
                    state = CharacterAction.State.BlockHit;
                    return true;
                default:
                    state = default;
                    return false;
            }
        }

        [Flags]
        private enum MovementLockReason { None = 0, Manual = 1, Animation = 2, Spawn = 4, Parry = 8, Critical = 16 }
    }
}
