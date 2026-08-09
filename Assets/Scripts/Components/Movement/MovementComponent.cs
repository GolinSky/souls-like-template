using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public class MovementComponent : BaseComponent<MovementModel>, IInitializable, IMovementComponent
    {
        private const float MAX_MOVEMENT_DELTA_TIME = 0.05f;
        private const float INPUT_DEAD_ZONE = 0.01f;
        private const float MIN_LOCK_ON_ROLL_RADIUS = 0.01f;
        private const float GROUNDED_VERTICAL_VELOCITY = -2.0f;
        private const float DEFAULT_TERMINAL_VELOCITY = 53.0f;

        [SerializeField] private CharacterController _controller;

        private readonly Dictionary<SpeedMultiplierKey, float> _speedMultipliers = new Dictionary<SpeedMultiplierKey, float>();

        private IComponentMediator _mediator;
        private MovementMode _movementMode = MovementMode.Free;
        private Transform _lockOnTarget;
        private Transform _activeRollTarget;
        private Vector2 _activeRollDirection;
        private Vector3 _horizontalVelocity;
        private Vector3 _groundNormal = Vector3.up;
        private Vector2 _animationBlendDirection;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _rotationVelocity;
        private float _speedChangeTime;
        private float _lastTargetSpeed = -1.0f;
        private float _jumpCooldownRemaining;
        private float _rollCooldownRemaining;
        private float _fallGraceRemaining;
        private float _turnAmount;
        private bool _movementBlocked;
        private bool _isRisingFromJump;
        private bool _isCrouching;
        private bool? _lastNotifiedGrounded;
        private float _defaultControllerHeight;
        private Vector3 _defaultControllerCenter;

        public void Initialize()
        {
            ValidateDependencies();

            _fallGraceRemaining = Model.FallTimeout;
            _defaultControllerHeight = _controller.height;
            _defaultControllerCenter = _controller.center;
            SynchronizeGroundedState();
        }

        public void SetMediator(IComponentMediator mediator)
        {
            ValidateDependencies();
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            SynchronizeGroundedState();
        }

        public void SetPosition(Vector3 position)
        {
            bool controllerWasEnabled = _controller.enabled;
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = controllerWasEnabled;

            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0.0f;
        }

        public void SetMovementBlocked(bool blocked)
        {
            _movementBlocked = blocked;

            if (!blocked)
            {
                _activeRollTarget = null;
                _activeRollDirection = Vector2.zero;
            }

            if (blocked && Model.Grounded)
            {
                _horizontalVelocity = Vector3.zero;
            }
        }

        public void ApplyAnimationMovement(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            Vector3 planarDelta = new Vector3(deltaPosition.x, 0.0f, deltaPosition.z);
            bool isLockedRoll = _activeRollTarget != null;
            if (isLockedRoll)
            {
                planarDelta = CalculateLockedRollDelta(planarDelta.magnitude);
            }

            if (Model.Grounded)
            {
                planarDelta = Vector3.ProjectOnPlane(planarDelta, _groundNormal);
            }

            _controller.Move(planarDelta + Vector3.up * deltaPosition.y);

            if (isLockedRoll)
            {
                FaceActiveRollTarget();
                return;
            }

            Vector3 rotatedForward = deltaRotation * transform.forward;
            rotatedForward.y = 0.0f;
            if (rotatedForward.sqrMagnitude > INPUT_DEAD_ZONE)
            {
                transform.rotation = Quaternion.LookRotation(rotatedForward.normalized, Vector3.up);
            }
        }

        public void SetLockOnTarget(bool isLockedOn, Transform lockOnTarget)
        {
            if (isLockedOn && lockOnTarget == null)
            {
                throw new ArgumentNullException(nameof(lockOnTarget), "Locked-on movement requires a target.");
            }

            _movementMode = isLockedOn ? MovementMode.LockedOn : MovementMode.Free;
            _lockOnTarget = isLockedOn ? lockOnTarget : null;
        }

        public void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier)
        {
            _speedMultipliers[key] = multiplier;
        }

        public void RemoveSpeedMultiplier(SpeedMultiplierKey key)
        {
            _speedMultipliers.Remove(key);
        }

        public void Move(
            Vector2 direction,
            float cameraYaw,
            bool sprint,
            bool jumpRequested,
            bool rollRequested,
            bool crouchActionHeld)
        {
            float deltaTime = MovementDeltaTime;
            Vector2 moveInput = Vector2.ClampMagnitude(direction, 1.0f);

            UpdateActionCooldowns(deltaTime);
            UpdateGroundedState(deltaTime);

            if (!_movementBlocked)
            {
                SetCrouchState(Model.Grounded && crouchActionHeld);
            }
            else if (!Model.Grounded)
            {
                SetCrouchState(false);
            }

            TryStartRoll(moveInput, cameraYaw, rollRequested);
            TryStartJump(jumpRequested);
            UpdateVerticalVelocity(deltaTime);

            Vector3 horizontalMotion = CalculateHorizontalMovement(moveInput, cameraYaw, sprint, deltaTime);
            CollisionFlags collisionFlags = _controller.Move((horizontalMotion + Vector3.up * _verticalVelocity) * deltaTime);

            if ((collisionFlags & CollisionFlags.Below) != 0 && _verticalVelocity <= 0.0f)
            {
                _isRisingFromJump = false;
                _fallGraceRemaining = Model.FallTimeout;
                SetGrounded(true);
                _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
            }

            RequireMediator().NotifyLocomotion(_animationBlend, _animationBlendDirection);
            RequireMediator().NotifyTurn(_turnAmount);
        }

        private void TryStartRoll(Vector2 moveInput, float cameraYaw, bool rollRequested)
        {
            if (!rollRequested || _movementBlocked || !Model.Grounded || _rollCooldownRemaining > 0.0f)
            {
                return;
            }

            Vector2 rollDirection;
            if (moveInput.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                rollDirection = Vector2.down;
                if (_movementMode == MovementMode.Free)
                {
                    Vector3 backward = -transform.forward;
                    backward.y = 0.0f;
                    transform.rotation = Quaternion.LookRotation(backward.normalized, Vector3.up);
                }
            }
            else
            {
                Vector3 worldDirection = ResolveWorldDirection(moveInput, cameraYaw);

                if (_movementMode == MovementMode.Free)
                {
                    transform.rotation = Quaternion.LookRotation(worldDirection, Vector3.up);
                    rollDirection = Vector2.up;
                }
                else
                {
                    rollDirection = QuantizeLockedRollDirection(moveInput);
                }
            }

            _activeRollDirection = rollDirection;
            _activeRollTarget = _movementMode == MovementMode.LockedOn ? _lockOnTarget : null;
            if (_activeRollTarget != null)
            {
                FaceActiveRollTarget();
            }

            _rollCooldownRemaining = Model.RollCooldown;
            RequireMediator().NotifyRoll(rollDirection);
        }

        private Vector3 CalculateLockedRollDelta(float rollDistance)
        {
            if (_activeRollTarget == null)
            {
                throw new InvalidOperationException("Locked roll has no target.");
            }

            Vector3 targetPosition = _activeRollTarget.position;
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0.0f;

            if (Mathf.Abs(_activeRollDirection.x) > 0.0f)
            {
                Vector3 radial = -toTarget;
                float radius = radial.magnitude;
                if (radius <= MIN_LOCK_ON_ROLL_RADIUS)
                {
                    throw new InvalidOperationException("Locked lateral roll requires distance from the target.");
                }

                float orbitAngle = -_activeRollDirection.x * rollDistance / radius * Mathf.Rad2Deg;
                Vector3 nextRadial = Quaternion.AngleAxis(orbitAngle, Vector3.up) * radial;
                return nextRadial - radial;
            }

            if (toTarget.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                throw new InvalidOperationException("Locked roll direction cannot be resolved at the target position.");
            }

            return toTarget.normalized * (_activeRollDirection.y * rollDistance);
        }

        private void FaceActiveRollTarget()
        {
            Vector3 toTarget = _activeRollTarget.position - transform.position;
            toTarget.y = 0.0f;
            if (toTarget.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        }

        private static Vector2 QuantizeLockedRollDirection(Vector2 moveInput)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                return new Vector2(Mathf.Sign(moveInput.x), 0.0f);
            }

            return new Vector2(0.0f, Mathf.Sign(moveInput.y));
        }

        private void TryStartJump(bool jumpRequested)
        {
            if (!jumpRequested || _movementBlocked || !Model.Grounded || _jumpCooldownRemaining > 0.0f)
            {
                return;
            }

            SetCrouchState(false);
            _verticalVelocity = Mathf.Sqrt(Model.JumpHeight * 2.0f * Mathf.Abs(Model.Gravity));
            _jumpCooldownRemaining = Model.JumpTimeout;
            _isRisingFromJump = true;
            _fallGraceRemaining = 0.0f;
            SetGrounded(false);
            RequireMediator().NotifyJump();
        }

        private void UpdateActionCooldowns(float deltaTime)
        {
            _jumpCooldownRemaining = Mathf.Max(0.0f, _jumpCooldownRemaining - deltaTime);
            _rollCooldownRemaining = Mathf.Max(0.0f, _rollCooldownRemaining - deltaTime);
        }

        private void UpdateGroundedState(float deltaTime)
        {
            if (_isRisingFromJump)
            {
                if (_verticalVelocity > 0.0f)
                {
                    SetGrounded(false);
                    return;
                }

                _isRisingFromJump = false;
            }

            if (HasWalkableGroundContact())
            {
                _fallGraceRemaining = Model.FallTimeout;
                SetGrounded(true);
                return;
            }

            if (!Model.Grounded)
            {
                return;
            }

            _fallGraceRemaining = Mathf.Max(0.0f, _fallGraceRemaining - deltaTime);
            if (_fallGraceRemaining <= 0.0f)
            {
                SetGrounded(false);
            }
        }

        private void SynchronizeGroundedState()
        {
            bool grounded = HasWalkableGroundContact();
            if (grounded)
            {
                _fallGraceRemaining = Model.FallTimeout;
                if (_verticalVelocity <= 0.0f)
                {
                    _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
                }
            }

            SetGrounded(grounded);
        }

        private bool HasWalkableGroundContact()
        {
            Vector3 groundCheckPosition = transform.position + Vector3.up * Model.GroundedOffset;
            bool hasGroundContact = Physics.CheckSphere(
                groundCheckPosition,
                Model.GroundedRadius,
                Model.GroundLayers,
                QueryTriggerInteraction.Ignore);
            return hasGroundContact && TryUpdateGroundNormal();
        }

        private bool TryUpdateGroundNormal()
        {
            float castRadius = Model.GroundedRadius * 0.9f;
            Vector3 castOrigin = transform.position + Vector3.up * (Model.GroundedRadius + 0.1f);
            float castDistance = Model.GroundedRadius + Mathf.Abs(Model.GroundedOffset) + 0.2f;

            if (Physics.SphereCast(
                    castOrigin,
                    castRadius,
                    Vector3.down,
                    out RaycastHit hit,
                    castDistance,
                    Model.GroundLayers,
                    QueryTriggerInteraction.Ignore))
            {
                _groundNormal = hit.normal;
                return Vector3.Angle(_groundNormal, Vector3.up) <= _controller.slopeLimit;
            }

            _groundNormal = Vector3.up;
            return false;
        }

        private void SetGrounded(bool grounded)
        {
            Model.Grounded = grounded;
            if (_mediator == null || _lastNotifiedGrounded == grounded)
            {
                return;
            }

            _lastNotifiedGrounded = grounded;
            _mediator.NotifyGrounded(grounded);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (Model.Grounded && _verticalVelocity <= 0.0f)
            {
                _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
                return;
            }

            _verticalVelocity += Model.Gravity * deltaTime;
            float terminalVelocity = Mathf.Abs(Model.TerminalVelocity) > 0.1f
                ? Mathf.Abs(Model.TerminalVelocity)
                : DEFAULT_TERMINAL_VELOCITY;
            _verticalVelocity = Mathf.Max(_verticalVelocity, -terminalVelocity);
        }

        private Vector3 CalculateHorizontalMovement(Vector2 moveInput, float cameraYaw, bool sprinting, float deltaTime)
        {
            bool hasMovementInput = moveInput.sqrMagnitude > INPUT_DEAD_ZONE;
            Vector3 worldDirection = hasMovementInput ? ResolveWorldDirection(moveInput, cameraYaw) : Vector3.zero;
            float targetSpeed = ResolveTargetSpeed(moveInput, sprinting);
            Vector3 desiredVelocity = worldDirection * targetSpeed;

            if (Model.Grounded)
            {
                if (_movementBlocked)
                {
                    _horizontalVelocity = Vector3.zero;
                }
                else
                {
                    UpdateGroundVelocity(desiredVelocity, targetSpeed, deltaTime);
                }

                _horizontalVelocity = Vector3.ProjectOnPlane(_horizontalVelocity, _groundNormal);
            }
            else if (!_movementBlocked && hasMovementInput)
            {
                float airAcceleration = Model.AirAcceleration * Model.AirControl;
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, desiredVelocity, airAcceleration * deltaTime);
            }

            UpdateFacing(worldDirection, hasMovementInput, deltaTime);
            UpdateAnimationBlend(worldDirection, hasMovementInput, targetSpeed, sprinting, deltaTime);
            return _horizontalVelocity;
        }

        private void UpdateGroundVelocity(Vector3 desiredVelocity, float targetSpeed, float deltaTime)
        {
            if (Mathf.Abs(targetSpeed - _lastTargetSpeed) > 0.01f)
            {
                _speedChangeTime = 0.0f;
                _lastTargetSpeed = targetSpeed;
            }

            _speedChangeTime += deltaTime;
            float changeRate = desiredVelocity.sqrMagnitude > INPUT_DEAD_ZONE
                ? Model.SpeedChangeRate.Evaluate(_speedChangeTime) * Model.SpeedChangeMultiplier
                : Model.StoppingAnimationBlendRate;
            float interpolation = 1.0f - Mathf.Exp(-Mathf.Max(0.0f, changeRate) * deltaTime);
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, desiredVelocity, interpolation);

            if (_horizontalVelocity.sqrMagnitude < INPUT_DEAD_ZONE * INPUT_DEAD_ZONE)
            {
                _horizontalVelocity = Vector3.zero;
            }
        }

        private float ResolveTargetSpeed(Vector2 moveInput, bool sprinting)
        {
            if (_movementBlocked || moveInput.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                return 0.0f;
            }

            if (_isCrouching)
            {
                sprinting = false;
            }

            float targetSpeed = sprinting
                ? Model.SprintSpeed
                : _isCrouching
                    ? Model.CrouchSpeed
                    : Model.MoveSpeed;

            if (_movementMode == MovementMode.LockedOn)
            {
                Vector2 normalizedInput = moveInput.normalized;
                float forwardMultiplier = normalizedInput.y >= 0.0f ? 1.0f : 0.72f;
                targetSpeed *= normalizedInput.x * normalizedInput.x * 0.85f
                    + normalizedInput.y * normalizedInput.y * forwardMultiplier;
            }

            return targetSpeed * moveInput.magnitude * GetExternalSpeedMultiplier();
        }

        private Vector3 ResolveWorldDirection(Vector2 moveInput, float cameraYaw)
        {
            if (_movementMode == MovementMode.LockedOn)
            {
                Vector3 forward = GetLockOnForward();
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                return (right * moveInput.x + forward * moveInput.y).normalized;
            }

            Quaternion cameraRotation = Quaternion.Euler(0.0f, cameraYaw, 0.0f);
            return (cameraRotation * new Vector3(moveInput.x, 0.0f, moveInput.y)).normalized;
        }

        private void UpdateFacing(Vector3 worldDirection, bool hasMovementInput, float deltaTime)
        {
            _turnAmount = 0.0f;
            if (_movementBlocked)
            {
                return;
            }

            Vector3 facingDirection;
            if (_movementMode == MovementMode.LockedOn)
            {
                facingDirection = GetLockOnForward();
            }
            else if (hasMovementInput)
            {
                facingDirection = worldDirection;
            }
            else
            {
                return;
            }

            float targetYaw = Mathf.Atan2(facingDirection.x, facingDirection.z) * Mathf.Rad2Deg;
            float currentYaw = transform.eulerAngles.y;
            float nextYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _rotationVelocity, Model.RotationSmoothTime, Mathf.Infinity, deltaTime);
            _turnAmount = Mathf.DeltaAngle(currentYaw, nextYaw) / 180.0f;
            transform.rotation = Quaternion.Euler(0.0f, nextYaw, 0.0f);
        }

        private void UpdateAnimationBlend(
            Vector3 worldDirection,
            bool hasMovementInput,
            float targetSpeed,
            bool sprinting,
            float deltaTime)
        {
            float blendRate = hasMovementInput
                ? Model.SpeedChangeRate.Evaluate(_speedChangeTime) * Model.SpeedChangeMultiplier
                : Model.StoppingAnimationBlendRate;
            float interpolation = 1.0f - Mathf.Exp(-Mathf.Max(0.0f, blendRate) * deltaTime);
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, interpolation);

            Vector2 targetBlendDirection = Vector2.zero;
            if (!_movementBlocked && hasMovementInput)
            {
                float blendMagnitude = sprinting && !_isCrouching ? 1.0f : 0.5f;
                if (_movementMode == MovementMode.LockedOn)
                {
                    Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
                    targetBlendDirection = new Vector2(localDirection.x, localDirection.z).normalized * blendMagnitude;
                }
                else
                {
                    targetBlendDirection = Vector2.up * blendMagnitude;
                }
            }

            _animationBlendDirection = Vector2.Lerp(_animationBlendDirection, targetBlendDirection, interpolation);
            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0.0f;
            }

            if (_animationBlendDirection.sqrMagnitude < INPUT_DEAD_ZONE * INPUT_DEAD_ZONE)
            {
                _animationBlendDirection = Vector2.zero;
            }
        }

        private Vector3 GetLockOnForward()
        {
            if (_lockOnTarget == null)
            {
                throw new InvalidOperationException("Locked-on movement has no target.");
            }

            Vector3 toTarget = _lockOnTarget.position - transform.position;
            toTarget.y = 0.0f;
            if (toTarget.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                return transform.forward;
            }

            return toTarget.normalized;
        }

        private void SetCrouchState(bool crouched)
        {
            if (_isCrouching == crouched)
            {
                return;
            }

            _isCrouching = crouched;
            if (crouched)
            {
                _controller.height = Model.CrouchHeight;
                _controller.center = new Vector3(
                    _defaultControllerCenter.x,
                    Model.CrouchHeight * 0.5f,
                    _defaultControllerCenter.z);
            }
            else
            {
                _controller.height = _defaultControllerHeight;
                _controller.center = _defaultControllerCenter;
            }

            RequireMediator().NotifyCrouch(crouched);
        }

        private float GetExternalSpeedMultiplier()
        {
            float total = 1.0f;
            foreach (float multiplier in _speedMultipliers.Values)
            {
                total *= multiplier;
            }

            return total;
        }

        private IComponentMediator RequireMediator()
        {
            return _mediator ?? throw new InvalidOperationException($"{name} requires an IComponentMediator.");
        }

        private void ValidateDependencies()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{name} requires a MovementModel.");
            }

            if (_controller == null)
            {
                throw new InvalidOperationException($"{name} requires a CharacterController.");
            }
        }

        private static float MovementDeltaTime => Mathf.Min(Time.deltaTime, MAX_MOVEMENT_DELTA_TIME);
    }
}
