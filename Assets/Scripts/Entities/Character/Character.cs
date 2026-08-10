using System;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
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
        private float _sprintPressedAt;
        private bool _sprintHoldQualified;
        private bool _manualMovementBlocked;
        private bool _animationMovementBlocked;
        private bool _animationRootMotionEnabled;

        public Transform CameraTarget => _cameraTarget;
        public InventoryComponent InventoryComponent => _inventoryComponent;
        public HealthStats HealthStats => _healthComponent.Stats;

        [Inject]
        public void InjectDependencies(ICameraService cameraService, AttackComponent attackComponent)
        {
            _cameraService = cameraService;
            _attackComponent = attackComponent;
        }

        public void Initialize()
        {
            ValidateDependencies();
            _movementComponent.SetMediator(this);
            _animatorComponent.SetMediator(this);
            _attackComponent.SetMediator(this);
            _equipmentComponent.SetMediator(this);
            _healthComponent.SetMediator(this);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void UpdateBehaviour(ProjectInputActions.CharacterActions actions)
        {
            if (actions.Sprint.WasPressedThisFrame())
            {
                _sprintPressedAt = Time.time;
                _sprintHoldQualified = false;
            }

            bool sprinting = actions.Sprint.IsPressed()
                && (_sprintHoldQualified || Time.time - _sprintPressedAt >= SPRINT_HOLD_THRESHOLD);

            if (sprinting)
            {
                _sprintHoldQualified = true;
            }

            bool rollRequested = actions.Roll.WasReleasedThisFrame() && !_sprintHoldQualified;

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            bool hasMovementInput = moveInput.sqrMagnitude > 0.0001f;
            bool canStartAttack = _movementComponent.Model.Grounded
                && !_manualMovementBlocked
                && !_animationMovementBlocked;
            bool canUseSpecialAttack = canStartAttack
                && !hasMovementInput
                && !_movementComponent.IsMoving;
            _attackComponent.HandleInput(
                actions,
                sprinting && hasMovementInput,
                canStartAttack,
                canUseSpecialAttack);

            _movementComponent.Move(
                moveInput,
                _cameraService.GetYaw(),
                sprinting,
                actions.Jump.WasPressedThisFrame(),
                rollRequested,
                actions.Crouch.IsPressed());

            if (actions.Sprint.WasReleasedThisFrame())
            {
                _sprintHoldQualified = false;
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

        public void NotifyAnimatorStateChanged(AnimatorStateMachineDto state)
        {
            _attackComponent.HandleAnimatorState(state);
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

        private void ValidateDependencies()
        {
            if (_movementComponent == null) throw new InvalidOperationException($"{name} requires a MovementComponent.");
            if (_animatorComponent == null) throw new InvalidOperationException($"{name} requires an AnimatorComponent.");
            if (_attackComponent == null) throw new InvalidOperationException($"{name} requires an AttackComponent.");
            if (_equipmentComponent == null) throw new InvalidOperationException($"{name} requires an EquipmentComponent.");
            if (_healthComponent == null) throw new InvalidOperationException($"{name} requires a HealthComponent.");
            if (_inventoryComponent == null) throw new InvalidOperationException($"{name} requires an InventoryComponent.");
            if (_cameraTarget == null) throw new InvalidOperationException($"{name} requires a camera target.");
            if (_cameraService == null) throw new InvalidOperationException($"{name} requires an ICameraService.");
        }
    }
}
