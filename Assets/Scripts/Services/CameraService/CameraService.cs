using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services;
using SoulsLike.Services.Settings;
using VContainer;

namespace SoulsLike.Services.CameraService
{
    public class CameraService : MonoBehaviour, ICameraService
    {
        private const float DIRECTION_THRESHOLD_SQR = 0.0001f;
        private const float SWITCH_ANGLE_DURATION = 0.4f;
        private const Ease SWITCH_ANGLE_EASE = Ease.InOutQuad;
        private const float ZOOM_FOV = 48f;
        private const float ZOOM_DURATION = 0.3f;
        private const Ease ZOOM_EASE = Ease.OutSine;

        private const float AIRBORNE_RISE_LAG = 0.65f;
        private const float AIRBORNE_FALL_LAG = 0.4f;
        private const float GROUNDED_FOLLOW_SMOOTH_TIME = 0.1f;
        private const float JUMP_FOLLOW_SMOOTH_TIME = 0.22f;
        private const float FALL_FOLLOW_SMOOTH_TIME = 0.15f;
        private const float LONG_FALL_SMOOTH_TIME = 0.08f;
        private const float GROUNDED_MAX_FOLLOW_SPEED = 5f;
        private const float JUMP_MAX_FOLLOW_SPEED = 5f;
        private const float FALL_MAX_FOLLOW_SPEED = 8f;
        private const float LONG_FALL_MAX_SPEED = 18f;
        private const float LONG_FALL_CATCHUP_DISTANCE = 4f;

        private const float MOUSE_YAW_DEGREES_PER_PIXEL = 0.09f;
        private const float MOUSE_PITCH_DEGREES_PER_PIXEL = 0.08f;
        private const float STICK_YAW_DEGREES_PER_SECOND = 220f;
        private const float STICK_PITCH_DEGREES_PER_SECOND = 150f;
        private const float TOP_CLAMP = 70f;
        private const float BOTTOM_CLAMP = -30f;

        private const float LOCK_BLEND_DURATION = 0.3f;
        private const Ease LOCK_BLEND_EASE = Ease.InOutSine;
        private const float LOCK_INITIAL_FOCUS_MIN_DISTANCE = 1.5f;
        private const float LOCK_AIM_SMOOTH_TIME = 0.08f;
        private const float LOCK_AIM_MAX_SPEED = 40f;
        private const float LOCK_MIN_FOCUS_HEIGHT = -0.75f;
        private const float LOCK_MAX_FOCUS_HEIGHT = 1.25f;
        private const float LOCK_ORBIT_YAW_ENTER_ANGLE = 12f;
        private const float LOCK_ORBIT_YAW_RELEASE_ANGLE = 6f;
        private const float LOCK_ORBIT_YAW_SMOOTH_TIME = 0.42f;
        private const float LOCK_ORBIT_YAW_MAX_SPEED = 90f;
        private const float LOCK_YAW_HALF_TURN_TOLERANCE = 2f;
        private const float LOCK_HEADING_HOLD_DISTANCE = 0.55f;
        private const float LOCK_HEADING_RELEASE_DISTANCE = 0.9f;
        private const float LOCK_YAW_FAST_SMOOTH_TIME = 0.05f;
        private const float LOCK_YAW_FAST_MAX_SPEED = 360f;
        private const float LOCK_YAW_FAST_DEAD_ZONE_DEGREES = 0.75f;
        private const float LOCK_FAST_FOLLOW_START_RATE = 45f;
        private const float LOCK_FAST_FOLLOW_FULL_RATE = 135f;
        private const float LOCK_FAST_FOLLOW_START_ERROR = 5f;
        private const float LOCK_FAST_FOLLOW_FULL_ERROR = 18f;
        private const float LOCK_YAW_RATE_FILTER_TIME = 0.08f;
        private const float LOCK_YAW_URGENCY_SMOOTH_TIME = 0.08f;
        private const float LOCK_YAW_LEAD_TIME = 0.045f;
        private const float LOCK_YAW_MAX_LEAD_DEGREES = 5f;
        private const float LOCK_BASE_PITCH = 20f;
        private const float LOCK_AIM_TILT_NEAR_DISTANCE = 2f;
        private const float LOCK_AIM_TILT_FAR_DISTANCE = 5f;
        private const float LOCK_ORBIT_PITCH_SMOOTH_TIME = 0.35f;
        private const float LOCK_ORBIT_PITCH_MAX_SPEED = 60f;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;
        [SerializeField] private CinemachineImpulseDefinition impulseDefinition = new()
        {
            ImpulseChannel = 1,
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
            ImpulseDuration = 0.2f,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Dissipating,
            DissipationRate = 0.25f,
            DissipationDistance = 12f,
            AmplitudeGain = 2f
        };

#if UNITY_EDITOR
        [Header("Debug / Diagnostics (Editor Only)")]
        [SerializeField] private bool debugIsYawOrbiting;
        [SerializeField] private bool debugIsPitchOrbiting;
        [SerializeField] private float debugYawVelocity;
        [SerializeField] private float debugLockBlend;
        [SerializeField] private Vector3 debugFilteredLockPoint;
        [SerializeField] private float debugFilteredLockBearingRate;
        [SerializeField] private float debugLockYawUrgency;
        [SerializeField] private float debugCameraTargetDistance = 3.3f;
#endif

        private Tween _switchTween;
        private Tween _zoomTween;
        private Tween _rigTween;
        private Tween _lockAimTiltTween;
        private long? _lockOnTargetEntityId;
        private IEntityLocator _entityLocator;
        private IInputService _inputService;
        private ISettingsService _settingsService;
        private CameraData _cameraData;
        private Transform _sourceTarget;
        private Transform _followTarget;
        private Transform _lockLookAtTarget;
        private CinemachineRecomposer _cinemachineRecomposer;
        private CameraData.CinemachineCameraSettings _freeLookCamera;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _followYVelocity;
        private float _yawVelocity;
        private float _pitchVelocity;
        private Vector3 _filteredLockPoint;
        private Vector3 _lockPointVelocity;
        private Vector3 _stableLockDirection;
        private float _lockBlend;
        private bool _wasGrounded = true;
        private bool _hasStableLockDirection;
        private bool _holdingCloseHeading;
        private bool _isYawOrbiting;
        private bool _isPitchOrbiting;
        private bool _isZoomed;
        private int _lastLockYawTurnSign;
        private float _previousLockBearingYaw;
        private float _filteredLockBearingRate;
        private float _lockYawUrgency;
        private bool _hasPreviousLockBearingYaw;
        private CameraSettingsData _settings = new();

        private void OnDestroy()
        {
            _settingsService?.UnregisterCameraService(this);
            _switchTween?.Kill();
            _zoomTween?.Kill();
            _rigTween?.Kill();
            _lockAimTiltTween?.Kill();
        }

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            IInputService inputService,
            CameraData cameraData,
            ISettingsService settingsService)
        {
            _entityLocator = entityLocator;
            _inputService = inputService;
            _settingsService = settingsService;
            _cameraData = cameraData;
            settingsService.RegisterCameraService(this);
        }

        public void SetTarget(Transform target)
        {
            _sourceTarget = target;
            _lockAimTiltTween?.Kill();
            _cinemachineRecomposer = cinemachineCamera.GetComponent<CinemachineRecomposer>();
            var rotationComposer = cinemachineCamera.GetComponent<CinemachineRotationComposer>();
            _cinemachineRecomposer.Tilt = 0f;
            rotationComposer.Damping = _cameraData.LockOnRotationComposerDamping;

            if (_followTarget == null)
            {
                _followTarget = new GameObject("Camera Follow Target").transform;
                _followTarget.SetParent(transform);
            }

            if (_lockLookAtTarget == null)
            {
                _lockLookAtTarget = new GameObject("Camera Lock LookAt Target").transform;
                _lockLookAtTarget.SetParent(transform);
            }

            _followTarget.SetPositionAndRotation(target.position, target.rotation);
            _cinemachineTargetYaw = target.eulerAngles.y;
            _cinemachineTargetPitch = 0f;
            _followYVelocity = 0f;
            _wasGrounded = true;
            _lockLookAtTarget.position = _followTarget.position + _followTarget.forward * LOCK_INITIAL_FOCUS_MIN_DISTANCE;
            _freeLookCamera = _cameraData.FreeLookCamera;
            ApplyRigBlend();
            ResetLockModeState();
            cinemachineCamera.Follow = _followTarget;
            cinemachineCamera.LookAt = null;
            cinemachineCamera.PreviousStateIsValid = false;
        }

        public void GenerateImpulse(Vector3 position, Vector3 velocity) =>
            impulseDefinition.CreateEvent(position, velocity);

        public void UpdateFollowTarget(bool grounded, float verticalVelocity)
        {
            Vector3 sourcePosition = _sourceTarget.position;
            Vector3 followPosition = _followTarget.position;
            followPosition.x = sourcePosition.x;
            followPosition.z = sourcePosition.z;

            if (grounded != _wasGrounded)
            {
                _followYVelocity = 0f;
            }

            float desiredY = Mathf.Clamp(
                followPosition.y,
                sourcePosition.y - AIRBORNE_RISE_LAG,
                sourcePosition.y + AIRBORNE_FALL_LAG);
            float smoothTime;
            float maxSpeed;
            if (grounded)
            {
                desiredY = sourcePosition.y;
                smoothTime = GROUNDED_FOLLOW_SMOOTH_TIME;
                maxSpeed = GROUNDED_MAX_FOLLOW_SPEED;
            }
            else if (verticalVelocity >= 0f)
            {
                smoothTime = JUMP_FOLLOW_SMOOTH_TIME;
                maxSpeed = JUMP_MAX_FOLLOW_SPEED;
            }
            else
            {
                float fallingOvershoot = followPosition.y - (sourcePosition.y + AIRBORNE_FALL_LAG);
                float longFallProgress = Mathf.InverseLerp(0f, LONG_FALL_CATCHUP_DISTANCE, fallingOvershoot);
                smoothTime = Mathf.Lerp(FALL_FOLLOW_SMOOTH_TIME, LONG_FALL_SMOOTH_TIME, longFallProgress);
                maxSpeed = Mathf.Lerp(FALL_MAX_FOLLOW_SPEED, LONG_FALL_MAX_SPEED, longFallProgress);
            }

            followPosition.y = Mathf.SmoothDamp(
                followPosition.y,
                desiredY,
                ref _followYVelocity,
                smoothTime,
                maxSpeed,
                Time.deltaTime);
            _followTarget.position = followPosition;
            _wasGrounded = grounded;
        }

        public float GetYaw()
        {
            return _cinemachineTargetYaw;
        }

        public float GetPitch()
        {
            return _cinemachineTargetPitch;
        }

        public void SwitchAngle()
        {
            _switchTween?.Kill();

            float currentFreeSide = _freeLookCamera.CameraSide;
            float targetSide = currentFreeSide < 0.5f ? 1.0f : 0.0f;
            _freeLookCamera.CameraSide = targetSide;

            if (_lockOnTargetEntityId.HasValue || _lockBlend > 0.001f)
            {
                return;
            }

            _switchTween = DOTween.To(
                () => cinemachineThirdPersonFollow.CameraSide,
                value => cinemachineThirdPersonFollow.CameraSide = value,
                targetSide,
                SWITCH_ANGLE_DURATION)
                .SetEase(SWITCH_ANGLE_EASE);
        }

        public Ray GetRay()
        {
            return targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        public void SetZoom(bool isZoomed)
        {
            _isZoomed = isZoomed;
            _zoomTween?.Kill();

            float targetFov = isZoomed
                ? ZOOM_FOV
                : Mathf.Lerp(_freeLookCamera.FieldOfView, _cameraData.LockOnCamera.FieldOfView, _lockBlend);

            _zoomTween = DOTween.To(
                () => cinemachineCamera.Lens.FieldOfView,
                SetFieldOfView,
                targetFov,
                ZOOM_DURATION)
                .SetEase(ZOOM_EASE);
        }

        public void SetLockOnTarget(long? targetEntityId)
        {
            if (!targetEntityId.HasValue)
            {
                ClearLockOnTarget();
                return;
            }

            _lockAimTiltTween?.Kill();
            if (_lockOnTargetEntityId == targetEntityId)
            {
                cinemachineCamera.LookAt = _lockLookAtTarget;
                return;
            }

            bool wasUnlocked = !_lockOnTargetEntityId.HasValue;
            _lockOnTargetEntityId = targetEntityId;

            if (wasUnlocked)
            {
                if (TryGetLockTarget(out TargetingSnapshot snapshot))
                {
                    Vector3 cameraToTarget = snapshot.LockPoint - targetCamera.transform.position;
                    float depth = Mathf.Max(
                        Vector3.Dot(cameraToTarget, targetCamera.transform.forward),
                        LOCK_INITIAL_FOCUS_MIN_DISTANCE);
                    _filteredLockPoint = targetCamera.transform.position + targetCamera.transform.forward * depth;
                }
                else
                {
                    _filteredLockPoint = targetCamera.transform.position + targetCamera.transform.forward * LOCK_INITIAL_FOCUS_MIN_DISTANCE;
                }

                _lockPointVelocity = Vector3.zero;
                _lockLookAtTarget.position = _filteredLockPoint;
                _isYawOrbiting = false;
                _isPitchOrbiting = false;
                _hasStableLockDirection = false;
                _holdingCloseHeading = false;
                _lastLockYawTurnSign = 0;

                _rigTween?.Kill();
                _rigTween = DOTween.To(
                    () => _lockBlend,
                    value =>
                    {
                        _lockBlend = value;
                        ApplyRigBlend();
                    },
                    1f,
                    LOCK_BLEND_DURATION)
                    .SetEase(LOCK_BLEND_EASE);

                cinemachineCamera.PreviousStateIsValid = false;
            }
            else
            {
                BeginLockTargetChange();
            }

            cinemachineCamera.LookAt = _lockLookAtTarget;
        }

        public void ClearLockOnTarget()
        {
            if (!_lockOnTargetEntityId.HasValue)
            {
                return;
            }

            Vector3 cameraForward = targetCamera.transform.forward;
            Vector3 planarForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            if (planarForward.sqrMagnitude > DIRECTION_THRESHOLD_SQR)
            {
                _cinemachineTargetYaw = Mathf.Atan2(planarForward.x, planarForward.z) * Mathf.Rad2Deg;
            }

            if (_followTarget != null)
            {
                _followTarget.rotation = Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f);
            }

            _lockOnTargetEntityId = null;
            cinemachineCamera.LookAt = null;
            ResetLockModeState();

            _lockAimTiltTween?.Kill();
            _lockAimTiltTween = DOTween.To(
                () => _cinemachineRecomposer.Tilt,
                value =>
                {
                    _cinemachineRecomposer.Tilt = value;
                    _isPitchOrbiting = Mathf.Abs(value) > 0.1f;
                },
                0f,
                LOCK_BLEND_DURATION)
                .SetEase(LOCK_BLEND_EASE);

            _rigTween?.Kill();
            _rigTween = DOTween.To(
                () => _lockBlend,
                value =>
                {
                    _lockBlend = value;
                    ApplyRigBlend();
                },
                0f,
                LOCK_BLEND_DURATION)
                .SetEase(LOCK_BLEND_EASE);

            cinemachineCamera.PreviousStateIsValid = false;
        }

        public void RecenterCamera()
        {
            if (cinemachineCamera.Follow != null)
            {
                _cinemachineTargetYaw = cinemachineCamera.Follow.eulerAngles.y;
                _cinemachineTargetPitch = 0f;
            }
        }

        public Camera GetMainCamera()
        {
            return targetCamera != null ? targetCamera : Camera.main;
        }

        public void ApplySettings(CameraSettingsData settings)
        {
            _settings = SettingsDataUtility.Copy(settings);
        }

        public void UpdateRotation(Vector2 look)
        {
            if (_lockOnTargetEntityId.HasValue)
            {
                if (TryGetLockTarget(out TargetingSnapshot snapshot))
                {
                    UpdateStableLockDirection(snapshot);
                    UpdateLockBearingRate(snapshot, Time.deltaTime);
                    UpdateLockBodyYaw(Time.deltaTime);
                    UpdateLockLookAtTarget(snapshot, Time.deltaTime);
                    UpdateLockAimPitch(snapshot, Time.deltaTime);
                }
                else
                {
                    ClearLockOnTarget();
                }
            }

            if (!_lockOnTargetEntityId.HasValue && look.sqrMagnitude > 0f)
            {
                ApplyFreeLook(look);
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            if (!_lockOnTargetEntityId.HasValue)
            {
                _cinemachineTargetPitch = ClampAngle(
                    _cinemachineTargetPitch,
                    BOTTOM_CLAMP,
                    TOP_CLAMP);
            }
            ApplyLogicalRotation();

#if UNITY_EDITOR
            UpdateDiagnostics();
#endif
        }

        private void ApplyFreeLook(Vector2 look)
        {
            float sensitivityMultiplier = Mathf.Pow(2f, (_settings.Sensitivity - 0.5f) * 2f);
            float horizontalMultiplier = _settings.InvertX ? -1f : 1f;
            float verticalMultiplier = _settings.InvertY ? -1f : 1f;

            if (_inputService.CharacterActions.Look.activeControl.device is Pointer)
            {
                _cinemachineTargetYaw += look.x * MOUSE_YAW_DEGREES_PER_PIXEL
                    * sensitivityMultiplier * horizontalMultiplier;
                _cinemachineTargetPitch += look.y * MOUSE_PITCH_DEGREES_PER_PIXEL
                    * sensitivityMultiplier * verticalMultiplier;
                return;
            }

            _cinemachineTargetYaw += look.x * STICK_YAW_DEGREES_PER_SECOND * Time.deltaTime
                * sensitivityMultiplier * horizontalMultiplier;
            _cinemachineTargetPitch += look.y * STICK_PITCH_DEGREES_PER_SECOND * Time.deltaTime
                * sensitivityMultiplier * verticalMultiplier;
        }

        private void UpdateStableLockDirection(TargetingSnapshot snapshot)
        {
            Vector3 toRoot = snapshot.Position - _followTarget.position;
            Vector3 planarDirection = Vector3.ProjectOnPlane(toRoot, Vector3.up);
            float planarDistance = planarDirection.magnitude;

            if (_holdingCloseHeading && planarDistance >= LOCK_HEADING_RELEASE_DISTANCE)
            {
                _holdingCloseHeading = false;
            }
            else if (!_holdingCloseHeading && planarDistance <= LOCK_HEADING_HOLD_DISTANCE)
            {
                _holdingCloseHeading = true;
            }

            if (!_holdingCloseHeading && planarDirection.sqrMagnitude > DIRECTION_THRESHOLD_SQR)
            {
                _stableLockDirection = planarDirection.normalized;
                _hasStableLockDirection = true;
            }
            else if (!_hasStableLockDirection)
            {
                _stableLockDirection = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f) * Vector3.forward;
                _hasStableLockDirection = true;
            }
        }

        private void UpdateLockBearingRate(TargetingSnapshot snapshot, float deltaTime)
        {
            Vector3 toTargetRoot = snapshot.Position - _followTarget.position;
            Vector3 planarDirection = Vector3.ProjectOnPlane(toTargetRoot, Vector3.up);

            float rawBearingRate = 0f;
            if (planarDirection.sqrMagnitude > DIRECTION_THRESHOLD_SQR)
            {
                float bearingYaw = Mathf.Atan2(planarDirection.x, planarDirection.z) * Mathf.Rad2Deg;
                if (_hasPreviousLockBearingYaw)
                {
                    rawBearingRate = Mathf.DeltaAngle(_previousLockBearingYaw, bearingYaw) / Mathf.Max(deltaTime, 0.0001f);
                }

                _previousLockBearingYaw = bearingYaw;
                _hasPreviousLockBearingYaw = true;
            }

            float rateBlend = 1f - Mathf.Exp(-deltaTime / Mathf.Max(LOCK_YAW_RATE_FILTER_TIME, 0.0001f));
            _filteredLockBearingRate = Mathf.Lerp(_filteredLockBearingRate, rawBearingRate, rateBlend);
        }

        private void UpdateLockBodyYaw(float deltaTime)
        {
            float targetYaw = Mathf.Atan2(_stableLockDirection.x, _stableLockDirection.z) * Mathf.Rad2Deg;
            float yawDelta = Mathf.DeltaAngle(_cinemachineTargetYaw, targetYaw);

            if (Mathf.Abs(Mathf.Abs(yawDelta) - 180f) <= LOCK_YAW_HALF_TURN_TOLERANCE && _lastLockYawTurnSign != 0)
            {
                yawDelta = Mathf.Abs(yawDelta) * _lastLockYawTurnSign;
            }
            else if (Mathf.Abs(yawDelta) > 0.001f)
            {
                _lastLockYawTurnSign = yawDelta > 0f ? 1 : -1;
            }

            float rateUrgency = Mathf.InverseLerp(
                LOCK_FAST_FOLLOW_START_RATE,
                LOCK_FAST_FOLLOW_FULL_RATE,
                Mathf.Abs(_filteredLockBearingRate));

            float errorUrgency = Mathf.InverseLerp(
                LOCK_FAST_FOLLOW_START_ERROR,
                LOCK_FAST_FOLLOW_FULL_ERROR,
                Mathf.Abs(yawDelta));

            float desiredUrgency = Mathf.Max(rateUrgency, errorUrgency);
            float urgencyBlend = 1f - Mathf.Exp(-deltaTime / Mathf.Max(LOCK_YAW_URGENCY_SMOOTH_TIME, 0.0001f));
            _lockYawUrgency = Mathf.Lerp(_lockYawUrgency, desiredUrgency, urgencyBlend);

            float smoothTime = Mathf.Lerp(
                LOCK_ORBIT_YAW_SMOOTH_TIME,
                LOCK_YAW_FAST_SMOOTH_TIME,
                _lockYawUrgency);

            float maxSpeed = Mathf.Lerp(
                LOCK_ORBIT_YAW_MAX_SPEED,
                LOCK_YAW_FAST_MAX_SPEED,
                _lockYawUrgency);

            float deadZone = Mathf.Lerp(
                LOCK_ORBIT_YAW_RELEASE_ANGLE,
                LOCK_YAW_FAST_DEAD_ZONE_DEGREES,
                _lockYawUrgency);

            float enterAngle = Mathf.Lerp(
                LOCK_ORBIT_YAW_ENTER_ANGLE,
                LOCK_YAW_FAST_DEAD_ZONE_DEGREES * 2f,
                _lockYawUrgency);

            float absError = Mathf.Abs(yawDelta);

            if (!_isYawOrbiting && absError >= enterAngle)
            {
                _isYawOrbiting = true;
            }
            else if (_isYawOrbiting && absError <= deadZone)
            {
                _isYawOrbiting = false;
            }

            float targetAngle = _cinemachineTargetYaw;
            if (_isYawOrbiting)
            {
                targetAngle = _cinemachineTargetYaw + (yawDelta - Mathf.Sign(yawDelta) * deadZone);
            }

            float predictedLead = Mathf.Clamp(
                _filteredLockBearingRate * LOCK_YAW_LEAD_TIME,
                -LOCK_YAW_MAX_LEAD_DEGREES,
                LOCK_YAW_MAX_LEAD_DEGREES);

            targetAngle += predictedLead * _lockYawUrgency;

            _cinemachineTargetYaw = Mathf.SmoothDampAngle(
                _cinemachineTargetYaw,
                targetAngle,
                ref _yawVelocity,
                smoothTime,
                maxSpeed,
                deltaTime);
        }

        private void UpdateLockAimPitch(TargetingSnapshot snapshot, float deltaTime)
        {
            Vector3 toTarget = snapshot.Position - _sourceTarget.position;
            toTarget.y = 0f;
            float targetTilt = Mathf.Lerp(
                LOCK_BASE_PITCH,
                0f,
                Mathf.InverseLerp(
                    LOCK_AIM_TILT_NEAR_DISTANCE,
                    LOCK_AIM_TILT_FAR_DISTANCE,
                    toTarget.magnitude));

            _cinemachineRecomposer.Tilt = Mathf.SmoothDampAngle(
                _cinemachineRecomposer.Tilt,
                targetTilt,
                ref _pitchVelocity,
                LOCK_ORBIT_PITCH_SMOOTH_TIME,
                LOCK_ORBIT_PITCH_MAX_SPEED,
                deltaTime);

            _isPitchOrbiting = Mathf.Abs(_cinemachineRecomposer.Tilt - targetTilt) > 0.1f;
        }

        private void UpdateLockLookAtTarget(TargetingSnapshot snapshot, float deltaTime)
        {
            Vector3 desiredLockPoint = snapshot.LockPoint;
            float heightOffset = desiredLockPoint.y - _followTarget.position.y;
            float clampedHeight = Mathf.Clamp(heightOffset, LOCK_MIN_FOCUS_HEIGHT, LOCK_MAX_FOCUS_HEIGHT);
            desiredLockPoint.y = _followTarget.position.y + clampedHeight;

            _filteredLockPoint = Vector3.SmoothDamp(
                _filteredLockPoint,
                desiredLockPoint,
                ref _lockPointVelocity,
                LOCK_AIM_SMOOTH_TIME,
                LOCK_AIM_MAX_SPEED,
                deltaTime);

            _lockLookAtTarget.position = _filteredLockPoint;
            cinemachineCamera.LookAt = _lockLookAtTarget;
        }

        private bool TryGetLockTarget(out TargetingSnapshot snapshot)
        {
            snapshot = default;
            if (!_lockOnTargetEntityId.HasValue
                || !_entityLocator.TryGetEntity(_lockOnTargetEntityId.Value, out IEntity entity))
            {
                return false;
            }

            if (!entity.TryGetComponent(out TargetingCommand command))
            {
                throw new InvalidOperationException(
                    $"Target entity {entity.Id} ({entity.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            snapshot = command.Read();
            return snapshot.IsAlive;
        }

        private void BeginLockTargetChange()
        {
            _lockPointVelocity *= 0.5f;
            _holdingCloseHeading = false;
            _hasStableLockDirection = false;
            _hasPreviousLockBearingYaw = false;
            _filteredLockBearingRate = 0f;
        }

        private void ResetLockModeState()
        {
            _yawVelocity = 0f;
            _pitchVelocity = 0f;
            _lockPointVelocity = Vector3.zero;
            _stableLockDirection = Vector3.zero;
            _hasStableLockDirection = false;
            _holdingCloseHeading = false;
            _isYawOrbiting = false;
            _isPitchOrbiting = false;
            _lastLockYawTurnSign = 0;
            _hasPreviousLockBearingYaw = false;
            _filteredLockBearingRate = 0f;
            _lockYawUrgency = 0f;
        }

        private void ApplyRigBlend()
        {
            cinemachineThirdPersonFollow.CameraSide = Mathf.Lerp(
                _freeLookCamera.CameraSide,
                _cameraData.LockOnCamera.CameraSide,
                _lockBlend);
            cinemachineThirdPersonFollow.Damping = Vector3.Lerp(
                _freeLookCamera.ThirdPersonFollowDamping,
                _cameraData.LockOnCamera.ThirdPersonFollowDamping,
                _lockBlend);

            if (!_isZoomed)
            {
                SetFieldOfView(Mathf.Lerp(
                    _freeLookCamera.FieldOfView,
                    _cameraData.LockOnCamera.FieldOfView,
                    _lockBlend));
            }
        }

        private void SetFieldOfView(float fieldOfView)
        {
            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = fieldOfView;
            cinemachineCamera.Lens = lens;
        }

        private void ApplyLogicalRotation()
        {
            if (cinemachineCamera.Follow != null)
            {
                cinemachineCamera.Follow.rotation = Quaternion.Euler(
                    _cinemachineTargetPitch,
                    _cinemachineTargetYaw,
                    0f);
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
            {
                angle += 360f;
            }

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return Mathf.Clamp(angle, min, max);
        }

#if UNITY_EDITOR
        private void UpdateDiagnostics()
        {
            debugIsYawOrbiting = _isYawOrbiting;
            debugIsPitchOrbiting = _isPitchOrbiting;
            debugYawVelocity = _yawVelocity;
            debugLockBlend = _lockBlend;
            debugFilteredLockPoint = _filteredLockPoint;
            debugFilteredLockBearingRate = _filteredLockBearingRate;
            debugLockYawUrgency = _lockYawUrgency;
            if (cinemachineCamera != null && cinemachineCamera.Follow != null && targetCamera != null)
            {
                Vector3 planarCameraToTarget = Vector3.ProjectOnPlane(cinemachineCamera.Follow.position - targetCamera.transform.position, Vector3.up);
                debugCameraTargetDistance = planarCameraToTarget.magnitude;
            }
        }
#endif
    }
}
