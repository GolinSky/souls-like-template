using System.Collections.Generic;
using Prospector.Utility.Timer;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public class MovementComponent : BaseComponent<MovementModel>
    {
        private const float MAX_MOVEMENT_DELTA_TIME = 0.05f;
        private const float INPUT_DEAD_ZONE = 0.01f;
        private const float MIN_LOCK_ON_ROLL_RADIUS = 0.01f;
        private const float GROUNDED_VERTICAL_VELOCITY = -2.0f;
        private const float DEFAULT_TERMINAL_VELOCITY = 53.0f;
        private const float GROUND_SNAP_DEAD_ZONE = 0.005f;
        private const int GROUND_PROBE_HIT_CAPACITY = 8;

        [SerializeField] private CharacterController controller;
        [SerializeField] private bool drawGroundDebug;

        private readonly Dictionary<SpeedMultiplierKey, float> _speedMultipliers = new Dictionary<SpeedMultiplierKey, float>();
        private readonly RaycastHit[] _groundProbeHits = new RaycastHit[GROUND_PROBE_HIT_CAPACITY];

        private MovementMode _movementMode = MovementMode.Free;
        private Transform _lockOnTarget;
        private Transform _activeRollTarget;
        private Vector2 _activeRollDirection;
        private Vector3 _horizontalVelocity;
        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _defaultControllerCenter;
        private Vector2 _animationBlendDirection;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _rotationVelocity;
        private float _speedChangeTime;
        private float _lastTargetSpeed = -1.0f;
        private float _jumpTime;
        private float _airborneTime;
        private float _lowestVerticalVelocity;
        private ITimer _jumpTimer;
        private ITimer _rollTimer;
        private ITimer _fallGraceTimer;
        private float _turnAmount;
        private bool _movementBlocked;
        private bool _wasJumpInitiated;
        private bool _wasSprintingAtTakeoff;
        private bool _isCrouching;
        private bool? _lastNotifiedGrounded;
        private float _defaultControllerHeight;
        private Vector3 _lastGroundProbeOrigin;
        private float _lastGroundProbeRadius;
        private float _lastGroundProbeDistance;
        private RaycastHit _lastGroundProbeHit;
        private bool _hasGroundProbeSample;
        private bool _hasGroundProbeHit;
        private bool _lastGroundProbeWasWalkable;
        private bool _jumpStarted;
        private bool _rollStarted;
        private bool _backStepStarted;
        private bool _landed;

        public LocomotionState CurrentLocomotionState { get; private set; } = LocomotionState.Grounded;
        public LandingType CurrentLandingType { get; private set; } = LandingType.None;
        public bool IsMoving => Model.Grounded
            && _horizontalVelocity.sqrMagnitude > INPUT_DEAD_ZONE * INPUT_DEAD_ZONE;
        public float HorizontalSpeed => _horizontalVelocity.magnitude;
        public float VerticalVelocity => _verticalVelocity;
        public float JumpTime => _jumpTime;
        public bool WasSprintingAtTakeoff => _wasSprintingAtTakeoff;
        public MovementPresentation Presentation => new MovementPresentation(_animationBlend, _animationBlendDirection, _turnAmount, _verticalVelocity, CurrentLandingType, Model.Grounded, _isCrouching);

        public bool TryConsumeJumpStarted() => Consume(ref _jumpStarted);
        public bool TryConsumeRollStarted(out Vector2 direction)
        {
            direction = _activeRollDirection;
            return Consume(ref _rollStarted);
        }
        public bool TryConsumeBackStepStarted() => Consume(ref _backStepStarted);
        public bool TryConsumeLanded() => Consume(ref _landed);

        public void Initialize()
        {
            _jumpTimer = TimerFactory.ConstructTimer(Model.JumpTimeout);
            _rollTimer = TimerFactory.ConstructTimer(Model.RollCooldown);
            _fallGraceTimer = TimerFactory.ConstructTimer(Model.FallTimeout);
            _defaultControllerHeight = controller.height;
            _defaultControllerCenter = controller.center;
            SynchronizeGroundedState();
        }

        public void SetPosition(Vector3 position)
        {
            bool controllerWasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            controller.enabled = controllerWasEnabled;

            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0.0f;
            _jumpTime = 0.0f;
            _airborneTime = 0.0f;
            _lowestVerticalVelocity = 0.0f;
            _wasJumpInitiated = false;
            _wasSprintingAtTakeoff = false;
            CurrentLandingType = LandingType.None;
            SynchronizeGroundedState();
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
            bool isRollAction = _activeRollDirection.sqrMagnitude > INPUT_DEAD_ZONE;
            if (isLockedRoll)
            {
                planarDelta = CalculateLockedRollDelta(planarDelta.magnitude);
            }

            if (Model.Grounded)
            {
                planarDelta = ProjectOnGround(planarDelta);
            }

            float verticalDelta = Model.Grounded || isRollAction ? 0.0f : deltaPosition.y;
            CollisionFlags collisionFlags = controller.Move(planarDelta + Vector3.up * verticalDelta);
            ResolveMovementCollisions(collisionFlags);
            MaintainGroundContact();

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
            bool crouchActionHeld)
        {
            float deltaTime = MovementDeltaTime;
            Vector2 moveInput = Vector2.ClampMagnitude(direction, 1.0f);

            UpdateGroundedState();

            if (!_movementBlocked)
            {
                SetCrouchState(Model.Grounded && crouchActionHeld);
            }
            else if (!Model.Grounded)
            {
                SetCrouchState(false);
            }

            UpdateVerticalVelocity(deltaTime);
            UpdateAirborneMetrics(deltaTime);

            Vector3 horizontalMotion = CalculateHorizontalMovement(moveInput, cameraYaw, sprint, deltaTime);
            CollisionFlags collisionFlags = controller.Move((horizontalMotion + Vector3.up * _verticalVelocity) * deltaTime);
            ResolveMovementCollisions(collisionFlags);
            MaintainGroundContact();

        }

        public void FaceInputDirection(Vector2 moveInput, float cameraYaw)
        {
            if (moveInput.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                return;
            }

            Vector3 facingDirection = _movementMode == MovementMode.LockedOn
                ? GetLockOnForward()
                : ResolveWorldDirection(moveInput, cameraYaw);
            transform.rotation = Quaternion.LookRotation(facingDirection, Vector3.up);
            _rotationVelocity = 0.0f;
        }

        public bool TryStartRoll(
            Vector2 moveInput,
            float cameraYaw,
            bool rollRequested,
            bool canInterruptAnimation)
        {
            if (rollRequested
                && !Model.Grounded
                && _verticalVelocity <= 0.0f
                && HasWalkableGroundContact()
                && CanLand())
            {
                CompleteLanding();
            }

            if (!rollRequested
                || (_movementBlocked && !canInterruptAnimation)
                || !Model.Grounded
                || (_rollTimer.IsRunning
                    && !_rollTimer.IsComplete
                    && !canInterruptAnimation))
            {
                return false;
            }

            bool isBackStep = moveInput.sqrMagnitude <= INPUT_DEAD_ZONE;
            Vector2 rollDirection;
            if (isBackStep)
            {
                rollDirection = Vector2.down;
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

            _rollTimer
                .ChangeDuration(Model.RollCooldown)
                .Start();
            if (isBackStep)
            {
                _backStepStarted = true;
            }
            else
            {
                _rollStarted = true;
            }

            return true;
        }

        private Vector3 CalculateLockedRollDelta(float rollDistance)
        {
            Vector3 targetPosition = _activeRollTarget.position;
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0.0f;

            if (Mathf.Abs(_activeRollDirection.x) > 0.0f)
            {
                Vector3 radial = -toTarget;
                float radius = radial.magnitude;
                if (radius <= MIN_LOCK_ON_ROLL_RADIUS)
                {
                    Debug.LogError($"[{nameof(MovementComponent)}] Locked lateral roll requires distance from the target.");
                    return Vector3.zero;
                }

                float orbitAngle = -_activeRollDirection.x * rollDistance / radius * Mathf.Rad2Deg;
                Vector3 nextRadial = Quaternion.AngleAxis(orbitAngle, Vector3.up) * radial;
                return nextRadial - radial;
            }

            if (toTarget.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                Debug.LogError($"[{nameof(MovementComponent)}] Locked roll direction cannot be resolved at the target position.");
                return Vector3.zero;
            }

            return toTarget.normalized * (_activeRollDirection.y * rollDistance);
        }

        private void FaceActiveRollTarget()
        {
            Vector3 toTarget = _activeRollTarget.position - transform.position;
            toTarget.y = 0.0f;
            if (toTarget.sqrMagnitude <= INPUT_DEAD_ZONE)
            {
                Debug.LogError($"[{nameof(MovementComponent)}] Cannot face active roll target at the target position.");
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

        public bool TryStartJump(bool jumpRequested, bool sprinting)
        {
            if (!jumpRequested || _movementBlocked || !Model.Grounded || (_jumpTimer.IsRunning && !_jumpTimer.IsComplete))
            {
                return false;
            }

            SetCrouchState(false);
            _verticalVelocity = Mathf.Sqrt(Model.JumpHeight * 2.0f * Mathf.Abs(Model.Gravity));
            _jumpTimer
                .ChangeDuration(Model.JumpTimeout)
                .Start();
            CurrentLocomotionState = LocomotionState.JumpStart;
            CurrentLandingType = LandingType.None;
            _jumpTime = 0.0f;
            _airborneTime = 0.0f;
            _lowestVerticalVelocity = 0.0f;
            _wasJumpInitiated = true;
            _wasSprintingAtTakeoff = sprinting;
            _fallGraceTimer.Stop();
            SetGrounded(false);
            _jumpStarted = true;
            return true;
        }

        private void UpdateGroundedState()
        {
            if (_wasJumpInitiated
                && (CurrentLocomotionState == LocomotionState.JumpStart
                    || CurrentLocomotionState == LocomotionState.Airborne)
                && (_jumpTime < Model.JumpGroundIgnoreTime || _verticalVelocity > 0.0f))
            {
                SetGrounded(false);
                return;
            }

            if (HasWalkableGroundContact())
            {
                _fallGraceTimer
                    .ChangeDuration(Model.FallTimeout)
                    .Start();
                if (!Model.Grounded && CanLand())
                {
                    CompleteLanding();
                }
                else if (Model.Grounded
                    && (CurrentLocomotionState == LocomotionState.Landing
                        || CurrentLocomotionState == LocomotionState.HardLanding))
                {
                    CurrentLocomotionState = LocomotionState.Grounded;
                }

                return;
            }

            if (!Model.Grounded)
            {
                return;
            }

            if (!_fallGraceTimer.IsRunning || _fallGraceTimer.IsComplete)
            {
                EnterAirborne();
            }
        }

        private void SynchronizeGroundedState()
        {
            bool grounded = HasWalkableGroundContact();
            if (grounded)
            {
                _fallGraceTimer
                    .ChangeDuration(Model.FallTimeout)
                    .Start();
                if (_verticalVelocity <= 0.0f)
                {
                    _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
                }

                CurrentLocomotionState = LocomotionState.Grounded;
            }
            else
            {
                CurrentLocomotionState = LocomotionState.Airborne;
                _airborneTime = 0.0f;
                _lowestVerticalVelocity = _verticalVelocity;
            }

            SetGrounded(grounded);
        }

        private bool HasWalkableGroundContact()
        {
            float probeDistance = Mathf.Abs(Model.GroundedOffset) + controller.skinWidth;
            return TryProbeGround(probeDistance, out _);
        }

        private bool TryProbeGround(float probeDistance, out RaycastHit groundHit)
        {
            float castRadius = Mathf.Min(Model.GroundedRadius, controller.radius) * 0.9f;
            float lowerSphereOffset = Mathf.Max(controller.height * 0.5f - controller.radius, 0.0f);
            Vector3 castOrigin = transform.TransformPoint(controller.center) - Vector3.up * lowerSphereOffset;
            float castDistance = Mathf.Max(probeDistance, controller.skinWidth);
            int hitCount = Physics.SphereCastNonAlloc(
                castOrigin,
                castRadius,
                Vector3.down,
                _groundProbeHits,
                castDistance,
                Model.GroundLayers,
                QueryTriggerInteraction.Ignore);

            bool foundWalkableGround = false;
            bool foundAnyGround = false;
            float closestWalkableDistance = float.PositiveInfinity;
            float closestGroundDistance = float.PositiveInfinity;
            RaycastHit closestGroundHit = default;
            groundHit = default;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = _groundProbeHits[index];
                if (hit.collider == controller)
                {
                    continue;
                }

                if (hit.distance < closestGroundDistance)
                {
                    foundAnyGround = true;
                    closestGroundDistance = hit.distance;
                    closestGroundHit = hit;
                }

                bool isWalkable = Vector3.Angle(hit.normal, Vector3.up) <= controller.slopeLimit;
                if (isWalkable && hit.distance < closestWalkableDistance)
                {
                    foundWalkableGround = true;
                    closestWalkableDistance = hit.distance;
                    groundHit = hit;
                }
            }

            _lastGroundProbeOrigin = castOrigin;
            _lastGroundProbeRadius = castRadius;
            _lastGroundProbeDistance = castDistance;
            _lastGroundProbeHit = foundWalkableGround ? groundHit : closestGroundHit;
            _hasGroundProbeSample = true;
            _hasGroundProbeHit = foundWalkableGround || foundAnyGround;
            _lastGroundProbeWasWalkable = foundWalkableGround;

            if (foundWalkableGround)
            {
                _groundNormal = groundHit.normal;
            }

            return foundWalkableGround;
        }

        private void MaintainGroundContact()
        {
            if (!Model.Grounded || _wasJumpInitiated || _verticalVelocity > 0.0f)
            {
                return;
            }

            if (!TryProbeGround(Model.GroundSnapDistance, out RaycastHit groundHit))
            {
                return;
            }

            float capsuleBottom = transform.TransformPoint(controller.center).y - controller.height * 0.5f;
            float snapDistance = capsuleBottom - groundHit.point.y - controller.skinWidth;
            if (snapDistance > Model.GroundSnapDistance)
            {
                return;
            }

            if (snapDistance > GROUND_SNAP_DEAD_ZONE)
            {
                CollisionFlags collisionFlags = controller.Move(Vector3.down * snapDistance);
                ResolveMovementCollisions(collisionFlags);
            }

            _fallGraceTimer
                .ChangeDuration(Model.FallTimeout)
                .Start();
            _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
        }

        private void SetGrounded(bool grounded)
        {
            Model.Grounded = grounded;
            if (_lastNotifiedGrounded == grounded)
            {
                return;
            }

            _lastNotifiedGrounded = grounded;
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

            if (CurrentLocomotionState == LocomotionState.JumpStart
                && _verticalVelocity <= Model.JumpApexThreshold)
            {
                CurrentLocomotionState = LocomotionState.Airborne;
            }
        }

        private void UpdateAirborneMetrics(float deltaTime)
        {
            if (Model.Grounded)
            {
                return;
            }

            _airborneTime += deltaTime;
            if (_wasJumpInitiated)
            {
                _jumpTime += deltaTime;
            }

            _lowestVerticalVelocity = Mathf.Min(_lowestVerticalVelocity, _verticalVelocity);
        }

        private void EnterAirborne()
        {
            CurrentLocomotionState = LocomotionState.Airborne;
            CurrentLandingType = LandingType.None;
            _airborneTime = 0.0f;
            _lowestVerticalVelocity = _verticalVelocity;
            _wasJumpInitiated = false;
            _wasSprintingAtTakeoff = false;
            SetGrounded(false);
        }

        private bool CanLand()
        {
            return !Model.Grounded
                && CurrentLocomotionState != LocomotionState.Grounded
                && _airborneTime >= Model.MinimumAirborneTime
                && _verticalVelocity <= 0.0f;
        }

        private void CompleteLanding()
        {
            float impactSpeed = Mathf.Abs(Mathf.Min(_lowestVerticalVelocity, _verticalVelocity));
            CurrentLandingType = impactSpeed >= Model.HardLandingMinFallSpeed
                ? LandingType.Hard
                : LandingType.Normal;
            CurrentLocomotionState = CurrentLandingType == LandingType.Hard
                ? LocomotionState.HardLanding
                : LocomotionState.Landing;
            _wasJumpInitiated = false;
            _fallGraceTimer
                .ChangeDuration(Model.FallTimeout)
                .Start();
            SetGrounded(true);
            _landed = true;
            _verticalVelocity = GROUNDED_VERTICAL_VELOCITY;
        }

        private void ResolveMovementCollisions(CollisionFlags collisionFlags)
        {
            if ((collisionFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0.0f)
            {
                _verticalVelocity = 0.0f;
                CurrentLocomotionState = LocomotionState.Airborne;
            }

            if ((collisionFlags & CollisionFlags.Below) != 0
                && _verticalVelocity <= 0.0f
                && CanLand())
            {
                CompleteLanding();
            }
        }

        private Vector3 CalculateHorizontalMovement(Vector2 moveInput, float cameraYaw, bool sprinting, float deltaTime)
        {
            bool hasMovementInput = moveInput.sqrMagnitude > INPUT_DEAD_ZONE;
            Vector3 worldDirection = hasMovementInput ? ResolveWorldDirection(moveInput, cameraYaw) : Vector3.zero;
            float targetSpeed = ResolveTargetSpeed(moveInput, sprinting);
            Vector3 desiredVelocity = worldDirection * targetSpeed;

            if (Model.Grounded)
            {
                desiredVelocity = ProjectOnGround(desiredVelocity);
                if (_movementBlocked)
                {
                    _horizontalVelocity = Vector3.zero;
                }
                else
                {
                    UpdateGroundVelocity(desiredVelocity, targetSpeed, deltaTime);
                }

                _horizontalVelocity = ProjectOnGround(_horizontalVelocity);
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

        private Vector3 ProjectOnGround(Vector3 velocity)
        {
            float speed = velocity.magnitude;
            if (speed <= INPUT_DEAD_ZONE)
            {
                return Vector3.zero;
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(velocity, _groundNormal);
            if (projectedVelocity.sqrMagnitude <= INPUT_DEAD_ZONE * INPUT_DEAD_ZONE)
            {
                return Vector3.zero;
            }

            return projectedVelocity.normalized * speed;
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
            else if (!Model.Grounded && _horizontalVelocity.sqrMagnitude > INPUT_DEAD_ZONE)
            {
                facingDirection = _horizontalVelocity.normalized;
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
            float smoothTime = Model.Grounded
                ? Model.RotationSmoothTime
                : Model.AirRotationSmoothTime;
            float nextYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _rotationVelocity, smoothTime, Mathf.Infinity, deltaTime);
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
                controller.height = Model.CrouchHeight;
                controller.center = new Vector3(
                    _defaultControllerCenter.x,
                    Model.CrouchHeight * 0.5f,
                    _defaultControllerCenter.z);
            }
            else
            {
                controller.height = _defaultControllerHeight;
                controller.center = _defaultControllerCenter;
            }

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

        private static bool Consume(ref bool value)
        {
            if (!value) return false;
            value = false;
            return true;
        }

        public readonly struct MovementPresentation
        {
            public float Speed { get; }
            public Vector2 BlendDirection { get; }
            public float TurnAmount { get; }
            public float VerticalVelocity { get; }
            public LandingType LandingType { get; }
            public bool Grounded { get; }
            public bool Crouching { get; }

            public MovementPresentation(float speed, Vector2 blendDirection, float turnAmount, float verticalVelocity, LandingType landingType, bool grounded, bool crouching)
            {
                Speed = speed;
                BlendDirection = blendDirection;
                TurnAmount = turnAmount;
                VerticalVelocity = verticalVelocity;
                LandingType = landingType;
                Grounded = grounded;
                Crouching = crouching;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGroundDebug || !_hasGroundProbeSample)
            {
                return;
            }

            Gizmos.color = _lastGroundProbeWasWalkable
                ? Color.green
                : _hasGroundProbeHit
                    ? Color.red
                    : Color.yellow;
            Gizmos.DrawWireSphere(_lastGroundProbeOrigin, _lastGroundProbeRadius);
            Gizmos.DrawLine(
                _lastGroundProbeOrigin,
                _lastGroundProbeOrigin + Vector3.down * _lastGroundProbeDistance);
            Gizmos.DrawWireSphere(
                _lastGroundProbeOrigin + Vector3.down * _lastGroundProbeDistance,
                _lastGroundProbeRadius);

            if (_hasGroundProbeHit)
            {
                Gizmos.DrawSphere(_lastGroundProbeHit.point, 0.025f);
                Gizmos.DrawLine(
                    _lastGroundProbeHit.point,
                    _lastGroundProbeHit.point + _lastGroundProbeHit.normal * 0.25f);
            }
        }

        private static float MovementDeltaTime => Mathf.Min(Time.deltaTime, MAX_MOVEMENT_DELTA_TIME);
    }
}
