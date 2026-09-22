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
using SoulsLike.Entities.Ladder;
using SoulsLike.Entities.Elevator;
using SoulsLike.Items;
using SoulsLike.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    /// <summary>Coordinates Unity-facing character components while runtime rules stay in plain C# services.</summary>
    public sealed class Character : MonoBehaviour, IInitializable, IDisposable, IPlatformRiderMotor, ICharacterActionExecutor
    {
        private const float NORMAL_ATTACK_SPEED = 1.0f;//todo: move to model->data. animator axis
        
        public event Action OnDeathAnimationCompleted;
        public event Action<int> CurrencyChanged;
        public event Action<CharacterAttributeStats> AttributesChanged;
        
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private AnimatorComponent animatorComponent;
        [SerializeField] private CharacterAudioComponent characterAudioComponent;
        [SerializeField] private EquipmentComponent equipmentComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private InventoryComponent inventoryComponent;
        [SerializeField] private EquipmentPresentation equipmentPresentation;// todo: investigate this type of code
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private LadderClimber ladderClimber;// todo: rework to components

        [Header("Aim Settings")]
        [SerializeField, Min(0.1f)] private float aimTargetDistance = 100f;//todo: what is that and why it here? this is data related

        private IEntityLocator _entityLocator;
        private ICombatStateNotifier _combatStateNotifier;
        private UniTaskCompletionSource<bool> _graceTransitionCompletionSource;
        //todo: why this component is not assigned as serialized field. not unified composition between components
        private AttackComponent _attackComponent;
        private CombatDefenseComponent _combatDefense;

        private CharacterActionStateMachine _actionStateMachine;
        private CharacterActionCoordinator _actionCoordinator;
        private ItemCatalog _itemCatalog;//todo: naming. we need have strict numbers of classes 
        
        private PlayerMeleeCombatRelay _meleeCombatRelay;
        private CriticalAttackController _criticalAttackController;
        private CharacterData _characterData;//todo: declared but not used . why
        private CharacterAttributeStats _attributes;
        private InventoryEntryId _activeItemEntryId;
        private ConsumableDefinition _activeConsumable;
        private GracePhase _gracePhase;
        private MovementLockReason _movementLockReasons;
        private ItemId _activeItemId;//todo: declared but not used . why
        private int _heldCurrency;
        private bool _isDeathAnimationPlaying;
        private bool _isItemUseInProgress;
        private bool _hasItemUseProgressFired;
        
        public Transform CameraTarget => cameraTarget;
        public bool IsGrounded => movementComponent.Model.Grounded;
        public float VerticalVelocity => movementComponent.VerticalVelocity;
        public InventoryComponent InventoryComponent => inventoryComponent;
        public HealthStats HealthStats => healthComponent.Stats;
        public int HeldCurrency => _heldCurrency;
        public CharacterAttributeStats Attributes => _attributes;
        public bool IsInputBlocked => _actionStateMachine.IsInputBlocked;
        public bool IsInLadderOperation => ladderClimber.IsBusy;
        //todo: too much condition - not scalable, not readable. find solution - maybe locking bool flag or more complicated data type or pattern
        public bool CanStartLadder => healthComponent.Stats.IsAlive
            && IsGrounded
            && !IsInputBlocked
            && CurrentActionState == CharacterAction.State.Neutral
            && !equipmentComponent.IsSwapInProgress
            && !_isItemUseInProgress
            && _gracePhase == GracePhase.None
            && !_criticalAttackController.IsRunning
            && !_combatDefense.IsInCriticalState
            && !_combatDefense.IsInHitReaction
            && !_combatDefense.IsParryStunned;

        private CharacterAction.State CurrentActionState => _actionStateMachine.CurrentState;
 

        [Inject]
        public void Configure(
            IEntityLocator entityLocator,
            ICombatStateNotifier combatStateNotifier,
            CriticalAttackController criticalAttackController,
            CharacterActionStateMachine actionStateMachine,
            CharacterActionCoordinator actionCoordinator,
            ItemCatalog itemCatalog,
            CharacterData characterData,
            EquipmentPresentation presentation,
            PlayerMeleeCombatRelay meleeCombatRelay,
            AttackComponent attackComponent,
            CombatDefenseComponent combatDefense)
        {
            //todo: make clean order - categorise diff classes by type/base types 
            _attackComponent = attackComponent;
            equipmentPresentation = presentation;
            _itemCatalog = itemCatalog;
            _entityLocator = entityLocator;
            _combatStateNotifier = combatStateNotifier;
            _characterData = characterData;
            _combatDefense = combatDefense;
            _meleeCombatRelay = meleeCombatRelay;
            _criticalAttackController = criticalAttackController;
            _actionStateMachine = actionStateMachine;
            _actionCoordinator = actionCoordinator;
            _attributes = characterData.Attributes;
            _heldCurrency = characterData.StartingCurrency;
            animatorComponent.ConfigureCharacter(this, movementComponent);
        }

        public void StageSpawn(Vector3? spawnPosition)
        {
            //todo: if nullable spawnPosition has not value - what we do? do we have another entry point for setting position ?
            if (spawnPosition.HasValue)
            {
                transform.position = spawnPosition.Value;
            }
        }

        public void Initialize()
        {
            healthComponent.Model.OnDamageApplied += OnDamageApplied;
            _combatDefense.OnHitResolved += OnHitResolved;
            _criticalAttackController.OnCompleted += OnCriticalCompleted;
            equipmentComponent.LoadoutChanged += ApplyEquipmentLoadout;
            movementComponent.Initialize();
            animatorComponent.SetHandMode(equipmentComponent.Model.ActiveHandMode);
            ApplyEquipmentLoadout(equipmentComponent.BuildLoadout());
            ApplyMovementPresentation();
            SetInputBlocked(true);//todo: not clear that we need to play entry animation or we sit on grace  - that is why we block input - no linear connections 
        }
        
        public void Dispose()
        {
            healthComponent.Model.OnDamageApplied -= OnDamageApplied;
            _combatDefense.OnHitResolved -= OnHitResolved;
            _criticalAttackController.OnCompleted -= OnCriticalCompleted;
            equipmentComponent.LoadoutChanged -= ApplyEquipmentLoadout;
        }

        public void BeginArrival(CharacterArrival arrival)
        {
            switch (arrival)
            {
                case CharacterArrival.WorldPosition:
                    animatorComponent.TriggerSpawn();
                    break;
                case CharacterArrival.GraceRest:
                    EnterGraceRestIdle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arrival), arrival, null);
            }
        }
        
        public void Tick(in CharacterInput input)
        {
            float now = Time.time;//todo: not informative variable name
            _attackComponent.SetStrongAttackHeld(input.StrongAttackHeld);
            if (!input.StrongAttackHeld)
            {
                animatorComponent.SetChargedAttackSpeed(NORMAL_ATTACK_SPEED);
            }

            if (ladderClimber.IsBusy)
            {
                ladderClimber.TickPlayer(input, Time.deltaTime);
                _combatDefense.SetBlocking(false);
                healthComponent.TickStaminaRecovery(Time.deltaTime, false);
                return;
            }

            
            //todo: too much args. hard to read.
            _actionStateMachine.Tick(input.SprintHeld, equipmentComponent.IsSwapInProgress);
            _criticalAttackController.UpdateNeutralEligibility(
                _actionStateMachine.CurrentState == CharacterAction.State.Neutral
                && !_combatDefense.IsInHitReaction
                && !_combatDefense.IsParryStunned
                && !_combatDefense.IsInCriticalState);
            _actionCoordinator.Process(input, now, this);
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
                || CharacterStaminaPolicy.CanStart(
                    healthComponent.Stats.CurrentStamina,
                    healthComponent.Stats.MaxStamina,
                    sprintStaminaCost,
                    movementModel.CombatSprintStaminaStartThreshold);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
            movementComponent.Move(
                ToUnityVector2(input.MoveInput),
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
            ladderClimber.ForceDetach(LadderDetachReason.Death);
            equipmentComponent.CancelSwap();
            _isDeathAnimationPlaying = true;
            SetInputBlocked(true);
            animatorComponent.TriggerDeath();
        }

        public void CompleteDeathAnimation()
        {
            _isDeathAnimationPlaying = false;
            _combatDefense.SetBlocking(false);
            _combatDefense.SetHitReaction(false);
            _combatDefense.SetParryStunned(false);
            _actionStateMachine.Clear();
            animatorComponent.CompleteDeathAnimation();
            SetInputBlocked(false);
        }

        public async UniTask PlayGraceUnblock(CancellationToken token)
        {
            BeginGraceTransition(GracePhase.Unblock);
            animatorComponent.TriggerGraceUnblock();

            //todo: having try catch finally in core code is bad approach - if smth went wrong with async task - throw exception.remove try catch.Fast Fall
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

            //todo: having try catch finally in core code is bad approach - if smth went wrong with async task - throw exception.remove try catch.Fast Fall
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

        private void EnterGraceRestIdle()
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

            //todo: having try catch finally in core code is bad approach - if smth went wrong with async task - throw exception.remove try catch.Fast Fall
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
                bool usesHeavyCost = action.Intent is CharacterAction.AttackIntent.Heavy
                    or CharacterAction.AttackIntent.Special;
                float staminaCost = CharacterStaminaPolicy.CalculateAttackCost(
                    usesHeavyCost,
                    combatProfile.LightAttackStaminaCost,
                    combatProfile.HeavyAttackStaminaCost,
                    combatProfile.StaminaCostMultiplier);
                float staminaStartThreshold = CharacterStaminaPolicy.GetAttackStartThreshold(
                    usesHeavyCost,
                    combatProfile.LightAttackStaminaStartThreshold,
                    combatProfile.HeavyAttackStaminaStartThreshold);
                if (!CharacterStaminaPolicy.CanStart(
                    healthComponent.Stats.CurrentStamina,
                    healthComponent.Stats.MaxStamina,
                    staminaCost,
                    staminaStartThreshold))
                {
                    return CharacterAction.Result.TemporarilyBlocked;
                }

                healthComponent.ConsumeStamina(staminaCost);
            }

            AttackResolution resolution = _attackComponent.ResolveAttack(action);
            animatorComponent.SetChargedAttackSpeed(resolution.ChargedSpeed);
            movementComponent.FaceInputDirection(
                ToUnityVector2(action.MoveInput),
                action.CameraYaw);
            animatorComponent.PlayAttack(
                resolution.AttackType,
                resolution.IsLeftHandAttack);
            return CharacterAction.Result.Executed;
        }

        private CharacterAction.Result StartRoll(in CharacterAction action)
        {
            bool canInterrupt = _actionStateMachine.CurrentState is CharacterAction.State.Attack or CharacterAction.State.Roll
                || IsMovementLocked(MovementLockReason.Animation);
            MovementModel movementModel = movementComponent.Model;
            bool drainsStamina = _combatStateNotifier.CurrentCombatState == CombatState.Combat;
            float staminaCost = drainsStamina ? movementModel.RollStaminaCost : 0f;

            if (drainsStamina && !CharacterStaminaPolicy.CanStart(
                    healthComponent.Stats.CurrentStamina,
                    healthComponent.Stats.MaxStamina,
                    staminaCost,
                    movementModel.RollStaminaStartThreshold))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            if (!movementComponent.TryStartRoll(
                    ToUnityVector2(action.MoveInput),
                    action.CameraYaw,
                    true,
                    canInterrupt))
            {
                return CharacterAction.Result.TemporarilyBlocked;
            }

            if (drainsStamina && staminaCost > 0f)
            {
                healthComponent.ConsumeStamina(staminaCost);
            }

            if (movementComponent.TryConsumeBackStepStarted())
            {
                animatorComponent.TriggerBackStep();
            }
            else if (movementComponent.TryConsumeRollStarted(out Vector2 direction))
            {
                animatorComponent.TriggerRoll(direction);
            }
            return CharacterAction.Result.Executed;
        }

        private CharacterAction.Result StartJump(in CharacterAction action)
        {
            MovementModel movementModel = movementComponent.Model;
            float staminaCost = movementModel.JumpStaminaCost;
            if (!CharacterStaminaPolicy.CanStart(
                    healthComponent.Stats.CurrentStamina,
                    healthComponent.Stats.MaxStamina,
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
                    _actionCoordinator.TryExecuteBufferedAction(this);
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
                ladderClimber.ForceDetach(LadderDetachReason.KnockOff);
                _combatDefense.SetHitReaction(true);
                _meleeCombatRelay.Cancel();
            }

            animatorComponent.TriggerHit(result);
        }

        private void Heal(float amount) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateHeal(healthComponent.Stats, amount));

        public void OnLadderAttached()
        {
            equipmentComponent.CancelSwap();
            _meleeCombatRelay.Cancel();
            _actionStateMachine.Clear();
            _combatDefense.SetBlocking(false);
            _combatDefense.SetHitReaction(false);
            _combatDefense.SetParryStunned(false);
            healthComponent.SetRecoveryInvulnerable(false);
            SetMovementLock(MovementLockReason.Ladder, true);
            movementComponent.SetMovementBlocked(true);
            animatorComponent.SetLadderTraversalBlocked(true);
            equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.RightHandArmament, false);
            equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.LeftHandArmament, false);
            SetLockOnTarget(false, null);
        }

        public void OnLadderDetached()
        {
            SetMovementLock(MovementLockReason.Ladder, false);
            movementComponent.SetMovementBlocked(_movementLockReasons != MovementLockReason.None);
            animatorComponent.SetLadderTraversalBlocked(false);
            movementComponent.SetPosition(transform.position);
            equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.RightHandArmament, true);
            equipmentPresentation.SetArmamentVisible(EquipmentSlotGroup.LeftHandArmament, true);
        }

        public bool CanUseQuickItemOnLadder()
        {
            EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
            return quickItem != null
                && quickItem.ItemId == ItemId.CrimsonFlask
                && quickItem.Entry.Quantity > 0;
        }

        public void UseQuickItemOnLadder()
        {
            EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
            if (quickItem == null || quickItem.ItemId != ItemId.CrimsonFlask || quickItem.Entry.Quantity <= 0)
            {
                return;
            }

            ConsumableDefinition flask = _itemCatalog.GetConsumable(quickItem.ItemId);
            inventoryComponent.Consume(quickItem.Entry.EntryId, 1);
            Heal(flask.EffectAmount);
        }

        public void GrantCurrency(int amount)
        {
            _heldCurrency = checked(_heldCurrency + amount);
            CurrencyChanged?.Invoke(_heldCurrency);
        }

        public void SpendCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _heldCurrency = checked(_heldCurrency - amount);
            CurrencyChanged?.Invoke(_heldCurrency);
        }

        public void LevelUp(CharacterAttributeStats newAttributes, int runesSpent)
        {
            if (runesSpent > 0)
            {
                SpendCurrency(runesSpent);
            }

            _attributes = newAttributes;
            AttributesChanged?.Invoke(_attributes);
        }

        public void Revive(float health) => healthComponent.ApplyAuthoritativeStats(
            healthComponent.CalculateRevive(healthComponent.Stats, health));

        /// <summary>Restores the actor's resources and flask supply after grace rest or respawn.</summary>
        public void RestoreAtGrace()
        {
            HealthStats stats = healthComponent.Stats;
            stats.CurrentHealth = stats.MaxHealth;
            stats.CurrentFocus = stats.MaxFocus;
            stats.CurrentStamina = stats.MaxStamina;
            stats.IsAlive = true;
            healthComponent.ApplyAuthoritativeStats(stats);
            inventoryComponent.RefillFlask(ItemId.CrimsonFlask, 5);
        }

        public void SetPosition(Vector3 position) => movementComponent.SetPosition(position);

        public void ApplyPlatformDisplacement(Vector3 displacement) =>
            movementComponent.ApplyPlatformDisplacement(displacement);

        public bool IsSupportedBy(Collider supportCollider) =>
            movementComponent.IsSupportedBy(supportCollider);

        public void SynchronizeAfterPlatformRide()
        {
        }

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
            bool hasRightEquippedWeapon = rightWeaponId.HasValue
                && rightWeaponId != ItemId.Fist;
            AnimationProfile profile = hasRightEquippedWeapon
                ? _itemCatalog.GetWeapon(rightWeaponId.Value).AnimationProfile
                : null;
            if (profile == null) animatorComponent.ResetAnimationProfile();
            else animatorComponent.ApplyAnimationProfile(
                profile,
                hasRightEquippedWeapon);

            animatorComponent.TransitionHandMode(loadout.HandMode);
            _attackComponent.SetActiveWeapons(
                rightWeaponId,
                equipmentPresentation.ActiveRightWeaponRuntime,
                ResolveAttackWeaponId(loadout, true),
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

        //todo: move to ext class - this is utility operation. add ext method for unity vector
        private static Vector2 ToUnityVector2(System.Numerics.Vector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public CharacterActionExecution Execute(in CharacterAction action)
        {
            if (action.ActionKind == CharacterAction.Kind.Attack
                && action.Intent == CharacterAction.AttackIntent.Light
                && !action.IsLeftHand
                && _actionStateMachine.CurrentState == CharacterAction.State.Neutral
                && !_actionStateMachine.HasBufferedAction
                && _criticalAttackController.TryStart())
            {
                _actionStateMachine.SetInputBlocked(true);
                SetMovementLock(MovementLockReason.Critical, true);
                movementComponent.SetMovementBlocked(true);
                return new CharacterActionExecution(
                    CharacterAction.Result.Executed,
                    CharacterAction.State.Critical);
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
            return new CharacterActionExecution(result, state);
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

       
    }
}
