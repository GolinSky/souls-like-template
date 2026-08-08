using System;
using SoulsLike.Entities.Character.Components;
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
    public class Character : MonoBehaviour, IInitializable, IComponentMediator, IDisposable
    {
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

        public Transform CameraTarget => _cameraTarget;
        public InventoryComponent InventoryComponent => _inventoryComponent;
        public HealthStats HealthStats => _healthComponent.Stats;

        [Inject]
        public void InjectDependencies(ICameraService cameraService)
        {
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
        }

        public void Initialize()
        {
            ValidateDependencies();
            _movementComponent.SetMediator(this);
            _equipmentComponent.SetMediator(this);
            _healthComponent.SetMediator(this);
            _healthComponent.OnStatsChanged += NotifyHealthStatsChanged;
            _healthComponent.OnDamageApplied += NotifyDamageApplied;
            _healthComponent.OnDied += NotifyDeath;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Dispose()
        {
            _healthComponent.OnStatsChanged -= NotifyHealthStatsChanged;
            _healthComponent.OnDamageApplied -= NotifyDamageApplied;
            _healthComponent.OnDied -= NotifyDeath;
        }

        public void UpdateBehaviour(ProjectInputActions.CharacterActions actions)
        {
            _movementComponent.Move(
                actions.Move.ReadValue<Vector2>(),
                _cameraService.GetYaw(),
                actions.Sprint.IsPressed(),
                actions.Jump.WasPressedThisFrame(),
                actions.Roll.WasPressedThisFrame(),
                actions.Crouch.IsPressed());

            Ray aimRay = _cameraService.GetRay();
            Vector3 targetPoint = Physics.Raycast(aimRay, out RaycastHit hit, _aimTargetDistance, _aimLayerMask)
                ? hit.point
                : aimRay.GetPoint(_aimTargetDistance);
            NotifyAimTarget(targetPoint);
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
        

        public void NotifyWeaponFire()
        {
            _equipmentComponent.NotifyWeaponFired();
        }

        public void NotifyHealthStatsChanged(HealthStats stats)
        {
        }

        public void NotifyDamageApplied(DamageResult result)
        {
        }

        public void NotifyDeath()
        {
        }

        public void NotifyLocomotion(float speed, Vector2 blendDirection)
        {
            _animatorComponent.SetLocomotion(speed, blendDirection);
        }

        public void NotifyJump()
        {
            _animatorComponent.SetJump();
        }

        public void NotifyRoll()
        {
            _animatorComponent.TriggerRoll();
        }

        public void NotifyCrouch(bool isCrouching)
        {
            _animatorComponent.SetCrouch(isCrouching);
        }

        public void NotifyZoom(bool isZoomed)
        {
            _cameraService.SetZoom(isZoomed);
        }

        public void NotifyAimTarget(Vector3 targetPosition)
        {
            _animatorComponent.SetAimTarget(targetPosition);
        }

        public void NotifyLockOn(bool isLockedOn)
        {
            _animatorComponent.SetLockOn(isLockedOn);
        }

        public void NotifyTurn(float turnAmount)
        {
            _animatorComponent.SetTurn(turnAmount);
        }

        public void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier)
        {
            _movementComponent.SetSpeedMultiplier(key, multiplier);
        }

        public void RemoveSpeedMultiplier(SpeedMultiplierKey key)
        {
            _movementComponent.RemoveSpeedMultiplier(key);
        }

        private void ValidateDependencies()
        {
            if (_movementComponent == null) throw new InvalidOperationException($"{name} requires a MovementComponent.");
            if (_animatorComponent == null) throw new InvalidOperationException($"{name} requires an AnimatorComponent.");
            if (_equipmentComponent == null) throw new InvalidOperationException($"{name} requires an EquipmentComponent.");
            if (_healthComponent == null) throw new InvalidOperationException($"{name} requires a HealthComponent.");
            if (_inventoryComponent == null) throw new InvalidOperationException($"{name} requires an InventoryComponent.");
            if (_cameraTarget == null) throw new InvalidOperationException($"{name} requires a camera target.");
            if (_cameraService == null) throw new InvalidOperationException($"{name} requires an ICameraService.");
        }
    }
}
