using SoulsLike.Entities.Character.Components;
using System;
using System.Collections.Generic;
using Prospector.Utility.Timer;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public class MovementComponent : BaseComponent<MovementModel>, IInitializable, IMovementComponent 
    {
        private const float MAX_MOVEMENT_DELTA_TIME = 0.05f;

        [SerializeField] private CharacterController _controller;

        private readonly Dictionary<SpeedMultiplierKey, float> _speedMultipliers = new Dictionary<SpeedMultiplierKey, float>();

        private MovementState _currentState = MovementState.Normal;

        // Locomotion state
        private float _speed;
        private float _animationBlend;
        private float _targetRotation;
        private Vector2 _animationBlendDirection;
        private float _rotationVelocity;
        private float _verticalVelocity;
        
        private float _speedChangeTime;
        private float _lastTargetSpeed;
        private float _turnAmount;
        
        // Timers
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private ITimer _rollTimer;
        private ITimer _rollCooldownTimer;
        private Vector3 _rollDirection;
        
        // Crouch state
        private bool _isCrouching;
        private float _defaultControllerHeight;
        private Vector3 _defaultControllerCenter;

        private IComponentMediator Mediator { get; set; } 


        public void Initialize()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{name} requires a MovementModel.");
            }

            if (_controller == null)
            {
                throw new InvalidOperationException($"{name} requires a CharacterController.");
            }

            _jumpTimeoutDelta = Model.JumpTimeout;
            _fallTimeoutDelta = Model.FallTimeout;
            
            _rollTimer = TimerFactory.ConstructTimer(Mathf.Max(0.01f, Model.RollDuration));
            _rollCooldownTimer = TimerFactory.ConstructTimer(Mathf.Max(0.01f, Model.RollCooldown));
            
            // To ensure they are considered "Complete" from the start before being triggered
            _rollTimer.Start();
            _rollCooldownTimer.Start();

            _defaultControllerHeight = _controller.height;
            _defaultControllerCenter = _controller.center;
        }
        
        public void SetPosition(Vector3 position)
        {
            // Specifically disable controller safely for hard teleports via transform
            if (_controller != null)
                _controller.enabled = false;
                
            transform.position = position;
            
            if (_controller != null)
                _controller.enabled = true;
        }

        public void ChangeState(MovementState newState)
        {
            if (_currentState != newState)
            {
                if (newState == MovementState.Normal)
                {
                    _lastTargetSpeed = -1f; // Force speed rate evaluation to reset
                }
                _currentState = newState;
            }
        }

        public void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier)
        {
            _speedMultipliers[key] = multiplier;
        }

        public void RemoveSpeedMultiplier(SpeedMultiplierKey key)
        {
            _speedMultipliers.Remove(key);
        }

        private float GetExternalSpeedMultiplier()
        {
            float total = 1.0f;
            foreach (var mult in _speedMultipliers.Values)
            {
                total *= mult; // multiplicative scaling
            }
            return total;
        }

        private float GetDirectionSpeedMultiplier(Vector2 moveInput)
        {
            if (moveInput.magnitude < 0.1f) return 1f;
            var dir = moveInput.normalized;
            
            // Forward is faster, backward is slower, sidestepping is medium
            float zMult = dir.y > 0 ? 1.0f : 0.6f;
            float xMult = 0.8f;
            
            // Interpolate using squared components since x^2 + y^2 = 1
            return (dir.x * dir.x * xMult) + (dir.y * dir.y * zMult);
        }
        
        public void Move(Vector2 direction, float cameraYaw, bool sprint, bool jumpRequested, bool rollRequested, bool crouchActionHeld)
        {
            switch (_currentState)
            {
                case MovementState.Normal:
                    HandleNormalState(direction, cameraYaw, sprint, jumpRequested, rollRequested, crouchActionHeld);
                    break;
                case MovementState.Rolling:
                    HandleRollingState(direction, cameraYaw, sprint, jumpRequested, rollRequested, crouchActionHeld);
                    break;
                case MovementState.Climbing:
                    // Future implementation: handle climbing movement along surface here
                    // No gravity or standard jump applied.
                    break;
                case MovementState.Ziplining:
                    // Future implementation: slide character along spline path
                    break;
                case MovementState.LedgeGrabbing:
                    // Future implementation: hang onto ledge, wait for climb-up or drop
                    break;
            }
            
        }

        private void HandleNormalState(Vector2 moveInput, float cameraYaw, bool sprinting, bool jumpRequested, bool rollRequested, bool crouchActionHeld)
        {
            // 1. Check if we are physically on the ground
            UpdateGroundedState();

            // Handle Roll
            if (rollRequested && Model.Grounded && _rollCooldownTimer.IsComplete)
            {
                ChangeState(MovementState.Rolling);
                _rollTimer.ChangeDuration(Model.RollDuration);
                _rollTimer.Start();

                if (moveInput.sqrMagnitude < 0.01f)
                {
                    _rollDirection = transform.forward;
                }
                else
                {
                    Vector3 cameraForward = Quaternion.Euler(0.0f, cameraYaw, 0.0f) * Vector3.forward;
                    Vector3 cameraRight = Quaternion.Euler(0.0f, cameraYaw, 0.0f) * Vector3.right;
                    _rollDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
                }
                
                // Immediately align body to roll direction
                if (_rollDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(_rollDirection, Vector3.up);
                }

                // Notify animations & clear blend tree briefly so animator doesn't loop locomotion badly internally if needed
                if (Mediator == null)
                {
                    Debug.LogError($"[MovementComponent] {name}: Mediator is missing!");
                }
                else
                {
                    Mediator.NotifyRoll();
                }
                return;
            }

            // Handle  Crouch
            if (Model.Grounded)
            {
                // Handle crouch state (hold)
                SetCrouchState(crouchActionHeld);
            }
            else
            {
                // If not grounded, force stand up
                SetCrouchState(false);
            }

            // 2. Perform gravity integration and jumps
            CalculateVerticalVelocity(jumpRequested);

            // 3. Perform input translation, acceleration, and rotation handling (Air control allowed)
            CalculateHorizontalMovement(moveInput, cameraYaw, sprinting, out Vector3 horizontalMotion);

            // 4. Combine into final movement delta for the frame
            Vector3 finalMotion = horizontalMotion + (Vector3.up * _verticalVelocity);
            _controller.Move(finalMotion * MovementDeltaTime);

            // 5. Update remote systems
            if (Mediator == null)
            {
                Debug.LogError($"[MovementComponent] {name}: Mediator is missing!");
            }
            else
            {
                Mediator.NotifyLocomotion(_animationBlend, _animationBlendDirection);
                Mediator.NotifyTurn(_turnAmount);
            }
        }

        private void HandleRollingState(Vector2 moveInput, float cameraYaw, bool sprinting, bool jumpRequested, bool rollRequested, bool crouchActionHeld)
        {
            // Keep evaluating grounded
            UpdateGroundedState();

            // Gravity must still apply!
            CalculateVerticalVelocity(false);

            if (_rollTimer.IsComplete)
            {
                _rollCooldownTimer.ChangeDuration(Model.RollCooldown);
                _rollCooldownTimer.Start();
                
                // Revert duplicate gravity since HandleNormalState will calculate it again this frame
                _verticalVelocity -= Model.Gravity * MovementDeltaTime;
                
                ChangeState(MovementState.Normal);
                HandleNormalState(moveInput, cameraYaw, sprinting, jumpRequested, false, crouchActionHeld);
                return;
            }

            Vector3 rollMotion = _rollDirection * Model.RollSpeed;
            Vector3 finalMotion = rollMotion + (Vector3.up * _verticalVelocity);
            _controller.Move(finalMotion * MovementDeltaTime);
        }

        private void UpdateGroundedState()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x, 
                transform.position.y - Model.GroundedOffset, 
                transform.position.z
            );
            
            bool previousGroundedState = Model.Grounded;
            Model.Grounded = Physics.CheckSphere(spherePosition, Model.GroundedRadius, Model.GroundLayers, QueryTriggerInteraction.Ignore);

            if (previousGroundedState != Model.Grounded)
            {
                if (Mediator == null)
                {
                    Debug.LogError($"[MovementComponent] {name}: Mediator is missing!");
                }
                else
                {
                    Mediator.NotifyGrounded(Model.Grounded);
                }
            }
        }

        private void SetCrouchState(bool crouched)
        {
            if (_isCrouching == crouched) return;
            _isCrouching = crouched;
            
            if (_isCrouching)
            {
                _controller.height = Model.CrouchHeight;
                _controller.center = new Vector3(_defaultControllerCenter.x, Model.CrouchHeight / 2.0f, _defaultControllerCenter.z);
            }
            else
            {
                _controller.height = _defaultControllerHeight;
                _controller.center = _defaultControllerCenter;
            }
            
            if (Mediator == null)
            {
                Debug.LogError($"[MovementComponent] {name}: Mediator is missing!");
            }
            else
            {
                Mediator.NotifyCrouch(_isCrouching);
            }
        }

        private void CalculateVerticalVelocity(bool jumpRequested)
        {
            if (Model.Grounded)
            {
                // Constantly refresh fall delays while on solid ground
                _fallTimeoutDelta = Model.FallTimeout;

                // Retain a tiny negative velocity to stick us reliably against downward slopes
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (jumpRequested && _jumpTimeoutDelta <= 0.0f)
                {
                    SetCrouchState(false); // Uncrouch on jump

                    // Basic physics kinematic equation for jump height
                    float gravityAbs = Mathf.Abs(Model.Gravity);
                    _verticalVelocity = Mathf.Sqrt(Model.JumpHeight * 2f * gravityAbs);
                    
                    if (Mediator == null)
                    {
                        Debug.LogError($"[MovementComponent] {name}: Mediator is missing!");
                    }
                    else
                    {
                        Mediator.NotifyJump();
                    }
                }

                if (_jumpTimeoutDelta > 0.0f)
                {
                    _jumpTimeoutDelta -= MovementDeltaTime;
                }
            }
            else
            {
                // Reset jump cooldown mid-air
                _jumpTimeoutDelta = Model.JumpTimeout;

                if (_fallTimeoutDelta > 0.0f)
                {
                    _fallTimeoutDelta -= MovementDeltaTime;
                }
            }

            // Unconditionally apply continuous gravity (even on ground, to hold onto slopes)
            _verticalVelocity += Model.Gravity * MovementDeltaTime;

            // Terminal falling velocity secure clamp
            float terminalVelocity = Mathf.Abs(Model.TerminalVelocity) > 0.1f ? Mathf.Abs(Model.TerminalVelocity) : 53.0f;
            if (_verticalVelocity < -terminalVelocity)
            {
                _verticalVelocity = -terminalVelocity;
            }
        }

        private void CalculateHorizontalMovement(Vector2 moveInput, float cameraYaw, bool sprinting, out Vector3 horizontalMotion)
        {
            if (_isCrouching) sprinting = false;

            float targetSpeed = sprinting ? Model.SprintSpeed : (_isCrouching ? Model.CrouchSpeed : Model.MoveSpeed);
            if (moveInput == Vector2.zero) 
            {
                targetSpeed = 0.0f;
            }
            else
            {
                // Apply modifiers gracefully
                targetSpeed *= GetDirectionSpeedMultiplier(moveInput);
                targetSpeed *= GetExternalSpeedMultiplier();
            }

            if (Mathf.Abs(targetSpeed - _lastTargetSpeed) > 0.01f)
            {
                _speedChangeTime = 0f;
                _lastTargetSpeed = targetSpeed;
            }
            _speedChangeTime += MovementDeltaTime;
            
            float currentRate = Model.SpeedChangeRate.Evaluate(_speedChangeTime) * Model.SpeedChangeMultiplier;

            // Get current planar speed
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float inputMagnitude = moveInput.magnitude;

            // Ease horizontally over time
            float speedOffset = 0.1f;
            if (moveInput == Vector2.zero)
            {
                _speed = 0f;
            }
            else if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, MovementDeltaTime * currentRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // Smooth the 1D FreeLocomotion animation blend (Speed parameter value)
            float blendRate = moveInput == Vector2.zero ? Model.StoppingAnimationBlendRate : currentRate;
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed * inputMagnitude, MovementDeltaTime * blendRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // --- FreeLocomotion ThirdPerson character rotation ---
            // Rotate character body towards movement direction relative to camera only when moving
            _turnAmount = 0f;
            horizontalMotion = Vector3.zero;
            Vector3 worldMoveDirection = Vector3.zero;

            if (moveInput != Vector2.zero)
            {
                Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraYaw;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, Model.RotationSmoothTime);
                
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                worldMoveDirection = targetDirection.normalized;
                horizontalMotion = worldMoveDirection * _speed;
            }

            // --- Animation blend: compute local direction for future LockOn state ---
            float localBlendMagnitude = moveInput != Vector2.zero ? (sprinting ? 1f : 0.5f) : 0f;
            Vector2 targetBlendDirection = Vector2.zero;

            if (moveInput != Vector2.zero && worldMoveDirection.sqrMagnitude > 0.001f)
            {
                Vector3 localDir = transform.InverseTransformDirection(worldMoveDirection);
                targetBlendDirection = new Vector2(localDir.x, localDir.z).normalized * localBlendMagnitude;
            }

            _animationBlendDirection = Vector2.Lerp(_animationBlendDirection, targetBlendDirection, MovementDeltaTime * blendRate);
            if (_animationBlendDirection.magnitude < 0.01f) _animationBlendDirection = Vector2.zero;
        }

        public void SetMediator(IComponentMediator mediator)
        {
            Mediator = mediator;
        }

        private static float MovementDeltaTime => Mathf.Min(Time.deltaTime, MAX_MOVEMENT_DELTA_TIME);
    }
}
