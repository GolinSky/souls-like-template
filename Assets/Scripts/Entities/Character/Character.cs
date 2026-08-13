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
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class Character : MonoBehaviour, IInitializable, IComponentMediator
    {
        private const float SPRINT_HOLD_THRESHOLD = 0.2f;

        [SerializeField] private MovementComponent _movementComponent;
        [SerializeField] private AnimatorComponent _animatorComponent;
        [SerializeField] private EquipmentComponent _equipmentComponent;
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private InventoryComponent _inventoryComponent;
        [SerializeField] private Transform _cameraTarget;

        [Header("Aim Settings")]
        [SerializeField, Min(0.1f)] private float _aimTargetDistance = 100f;
        [SerializeField] private LayerMask _aimLayerMask;

        private ICameraService _cameraService;
        private AttackComponent _attackComponent;
        private CharacterActionBuffer _actionBuffer;
        private ITimer _sprintHoldTimer;
        private bool _sprintHoldQualified;
        private bool _actionTransitionOpen;
        private bool _manualMovementBlocked;
        private bool _animationMovementBlocked;
        private bool _animationRootMotionEnabled;

        public Transform CameraTarget => _cameraTarget;
        public InventoryComponent InventoryComponent => _inventoryComponent;
        public HealthStats HealthStats => _healthComponent.Stats;
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

        public void Initialize()
        {
            _movementComponent.SetMediator(this);
            _animatorComponent.SetMediator(this);
            _attackComponent.SetMediator(this);
            _equipmentComponent.SetMediator(this);
            _healthComponent.SetMediator(this);
            _animatorComponent.SetHandMode(_equipmentComponent.Model.ActiveHandMode);
            _sprintHoldTimer = TimerFactory.ConstructTimer(SPRINT_HOLD_THRESHOLD);
            Cursor.lockState = CursorLockMode.Locked;
            IsInputBlocked = true;
            _animatorComponent.TriggerSpawn();
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
            bool canStartAttack = _movementComponent.Model.Grounded
                && !_manualMovementBlocked
                && !_animationMovementBlocked;
            bool canBufferAttack = _movementComponent.Model.Grounded
                && !_manualMovementBlocked;
            bool canBufferSpecialAttack = canBufferAttack;

            bool handModeSwitched = false;
            if (actions.TwoHanded.WasPressedThisFrame()
                && canStartAttack
                && !_attackComponent.IsActionActive)
            {
                handModeSwitched = true;
                HandMode handMode = _equipmentComponent.SwitchHandMode();
                _animatorComponent.TransitionHandMode(handMode);
            }

            if (!handModeSwitched && _attackComponent.TryCaptureAction(
                actions,
                sprinting && hasMovementInput,
                canBufferAttack,
                canBufferSpecialAttack,
                out BufferedCharacterAction attackAction))
            {
                _actionBuffer.Buffer(attackAction);
            }

            if (!handModeSwitched
                && actions.Roll.WasReleasedThisFrame()
                && !_sprintHoldQualified)
            {
                _actionBuffer.Buffer(BufferedCharacterAction.Roll(moveInput, cameraYaw));
            }

            if (!handModeSwitched)
            {
                TryExecuteBufferedAction(
                    canStartAttack,
                    canBufferSpecialAttack,
                    _actionTransitionOpen && _attackComponent.IsActionActive);
            }

            _movementComponent.Move(
                moveInput,
                cameraYaw,
                sprinting,
                actions.Jump.WasPressedThisFrame(),
                false,
                actions.Crouch.IsPressed());

            bool canBlock = !handModeSwitched
                && _movementComponent.Model.Grounded
                && !_manualMovementBlocked
                && (!_animationMovementBlocked
                    || (_actionTransitionOpen && _attackComponent.IsActionActive));
            _animatorComponent.SetWeaponBlock(actions.Guard.IsPressed() && canBlock);

            if (actions.Sprint.WasReleasedThisFrame())
            {
                _sprintHoldQualified = false;
                _sprintHoldTimer.Reset();
            }
        }

        public DamageResult ApplyDamage(DamageRequest request)
        {
            DamageResult result = _healthComponent.CalculateDamage(request, _healthComponent.Stats);
            _healthComponent.ApplyAuthoritativeStats(result.NewStats);
            _healthComponent.NotifyDamageApplied(result);
            return result;
        }

        public void Heal(float amount)
        {
            HealthStats stats = _healthComponent.CalculateHeal(_healthComponent.Stats, amount);
            _healthComponent.ApplyAuthoritativeStats(stats);
        }

        public void Revive(float health)
        {
            HealthStats stats = _healthComponent.CalculateRevive(_healthComponent.Stats, health);
            _healthComponent.ApplyAuthoritativeStats(stats);
        }

        public void NotifyGrounded(bool isGrounded)
        {
            _animatorComponent.SetGrounded(isGrounded);
        }

        public void NotifyHealthStatsChanged(HealthStats stats)
        {
            _healthComponent.Model.ApplyStats(stats);
        }

        public void NotifyDamageApplied(DamageResult result)
        {
            _healthComponent.Model.NotifyDamageApplied(result);
        }

        public void NotifyDeath()
        {
            _healthComponent.Model.NotifyDeath();
        }

        public void NotifyLocomotion(float speed, Vector2 blendDirection)
        {
            _animatorComponent.SetLocomotion(speed, blendDirection);
        }

        public void NotifyJump()
        {
            _animatorComponent.SetJump();
        }

        public void NotifyRoll(Vector2 direction)
        {
            _animatorComponent.TriggerRoll(direction);
        }

        public void NotifyBackStep()
        {
            _animatorComponent.TriggerBackStep();
        }

        public void NotifyCrouch(bool isCrouching)
        {
            _animatorComponent.SetCrouch(isCrouching);
        }
        
        public void SetLockOnTarget(bool isLockedOn, Transform lockOnTarget)
        {
            _movementComponent.SetLockOnTarget(isLockedOn, lockOnTarget);
            _animatorComponent.SetLockOn(isLockedOn);
        }

        public void NotifyTurn(float turnAmount)
        {
            _animatorComponent.SetTurn(turnAmount);
        }

        public void NotifyAttack(AttackType attackType)
        {
            _animatorComponent.PlayAttack(attackType);
        }

        public void SetChargedAttackSpeed(float speed)
        {
            _animatorComponent.SetChargedAttackSpeed(speed);
        }

        public void NotifyAnimatorStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);

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
                _movementComponent.ApplyAnimationMovement(deltaPosition, deltaRotation);
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
            _movementComponent.SetSpeedMultiplier(key, multiplier);
        }

        public void RemoveSpeedMultiplier(SpeedMultiplierKey key)
        {
            _movementComponent.RemoveSpeedMultiplier(key);
        }

        private void SynchronizeMovementBlock()
        {
            _movementComponent.SetMovementBlocked(_manualMovementBlocked || _animationMovementBlocked);
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
                if (_movementComponent.TryStartRoll(
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

            if (!_movementComponent.Model.Grounded
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
