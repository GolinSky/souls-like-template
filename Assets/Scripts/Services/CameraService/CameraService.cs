using System;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services;
using VContainer;

namespace SoulsLike.Services.CameraService
{
    public interface ICameraService
    {
        void SetTarget(Transform target);
        void UpdateFollowTarget(bool grounded, float verticalVelocity);
        void UpdateRotation(Vector2 look);
        float GetYaw();
        float GetPitch();
        void SwitchAngle();
        Ray GetRay();
        void SetZoom(bool isZoomed);
        void SetLockOnTarget(long? targetEntityId);
        void ClearLockOnTarget();
        void RecenterCamera();
        Camera GetMainCamera();
    }

    public class CameraService : MonoBehaviour, ICameraService
    {
        private const float DIRECTION_THRESHOLD_SQR = 0.0001f;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

#if UNITY_EDITOR
        [Header("Debug / Diagnostics (Editor Only)")]
        [SerializeField] private bool debugIsYawOrbiting;
        [SerializeField] private bool debugIsPitchOrbiting;
        [SerializeField] private float debugYawVelocity;
        [SerializeField] private float debugLockBlend;
        [SerializeField] private Vector3 debugFilteredLockPoint;
#endif

        private Tween _switchTween;
        private Tween _zoomTween;
        private Tween _rigTween;
        private long? _lockOnTargetEntityId;
        private IEntityLocator _entityLocator;
        private IInputService _inputService;
        private CameraData _cameraData;
        private Transform _sourceTarget;
        private Transform _followTarget;
        private Transform _lockLookAtTarget;
        private CameraData.CameraRigProfile _freeRigProfile;
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

        private void OnDestroy()
        {
            _switchTween?.Kill();
            _zoomTween?.Kill();
            _rigTween?.Kill();
        }

        [Inject]
        public void Construct(IEntityLocator entityLocator, IInputService inputService, CameraData cameraData)
        {
            _entityLocator = entityLocator;
            _inputService = inputService;
            _cameraData = cameraData;
        }

        public void SetTarget(Transform target)
        {
            _sourceTarget = target;

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
            _lockLookAtTarget.position = _followTarget.position + _followTarget.forward * _cameraData.LockInitialFocusMinDistance;
            CaptureCurrentRig();
            ResetLockModeState();
            cinemachineCamera.Follow = _followTarget;
            cinemachineCamera.LookAt = null;
            cinemachineCamera.PreviousStateIsValid = false;
        }

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
                sourcePosition.y - _cameraData.AirborneRiseLag,
                sourcePosition.y + _cameraData.AirborneFallLag);
            float smoothTime;
            float maxSpeed;
            if (grounded)
            {
                desiredY = sourcePosition.y;
                smoothTime = _cameraData.GroundedFollowSmoothTime;
                maxSpeed = _cameraData.GroundedMaxFollowSpeed;
            }
            else if (verticalVelocity >= 0f)
            {
                smoothTime = _cameraData.JumpFollowSmoothTime;
                maxSpeed = _cameraData.JumpMaxFollowSpeed;
            }
            else
            {
                float fallingOvershoot = followPosition.y - (sourcePosition.y + _cameraData.AirborneFallLag);
                float longFallProgress = Mathf.InverseLerp(0f, _cameraData.LongFallCatchupDistance, fallingOvershoot);
                smoothTime = Mathf.Lerp(_cameraData.FallFollowSmoothTime, _cameraData.LongFallSmoothTime, longFallProgress);
                maxSpeed = Mathf.Lerp(_cameraData.FallMaxFollowSpeed, _cameraData.LongFallMaxSpeed, longFallProgress);
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

            float currentFreeSide = _freeRigProfile.CameraSide;
            float targetSide = currentFreeSide < 0.5f ? 1.0f : 0.0f;
            _freeRigProfile.CameraSide = targetSide;

            if (_lockOnTargetEntityId.HasValue || _lockBlend > 0.001f)
            {
                return;
            }

            _switchTween = DOTween.To(
                () => cinemachineThirdPersonFollow.CameraSide,
                value => cinemachineThirdPersonFollow.CameraSide = value,
                targetSide,
                _cameraData.SwitchAngleDuration)
                .SetEase(_cameraData.SwitchAngleEase);
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
                ? _cameraData.ZoomFov
                : Mathf.Lerp(_freeRigProfile.FieldOfView, _cameraData.HumanoidLockProfile.FieldOfView, _lockBlend);

            _zoomTween = DOTween.To(
                () => cinemachineCamera.Lens.FieldOfView,
                SetFieldOfView,
                targetFov,
                _cameraData.ZoomDuration)
                .SetEase(_cameraData.ZoomEase);
        }

        public void SetLockOnTarget(long? targetEntityId)
        {
            if (!targetEntityId.HasValue)
            {
                ClearLockOnTarget();
                return;
            }

            if (_lockOnTargetEntityId == targetEntityId)
            {
                cinemachineCamera.LookAt = _lockLookAtTarget;
                return;
            }

            bool wasUnlocked = !_lockOnTargetEntityId.HasValue;
            _lockOnTargetEntityId = targetEntityId;

            if (wasUnlocked)
            {
                float intendedFreeCameraSide = _freeRigProfile.CameraSide;
                float intendedFreeFieldOfView = _freeRigProfile.FieldOfView;
                CaptureCurrentRig();
                _freeRigProfile.CameraSide = intendedFreeCameraSide;
                if (_isZoomed)
                {
                    _freeRigProfile.FieldOfView = intendedFreeFieldOfView;
                }

                if (TryGetLockTarget(out TargetingSnapshot snapshot))
                {
                    Vector3 cameraToTarget = snapshot.LockPoint - targetCamera.transform.position;
                    float depth = Mathf.Max(
                        Vector3.Dot(cameraToTarget, targetCamera.transform.forward),
                        _cameraData.LockInitialFocusMinDistance);
                    _filteredLockPoint = targetCamera.transform.position + targetCamera.transform.forward * depth;
                }
                else
                {
                    _filteredLockPoint = targetCamera.transform.position + targetCamera.transform.forward * _cameraData.LockInitialFocusMinDistance;
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
                    _cameraData.LockBlendDuration)
                    .SetEase(_cameraData.LockBlendEase);

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

            _cinemachineTargetPitch = ClampAngle(
                -Mathf.Asin(Mathf.Clamp(cameraForward.y, -1f, 1f)) * Mathf.Rad2Deg,
                _cameraData.BottomClamp,
                _cameraData.TopClamp);

            if (_followTarget != null)
            {
                _followTarget.rotation = Quaternion.Euler(
                    _cinemachineTargetPitch + _cameraData.CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0f);
            }

            _lockOnTargetEntityId = null;
            cinemachineCamera.LookAt = null;
            ResetLockModeState();

            _rigTween?.Kill();
            _rigTween = DOTween.To(
                () => _lockBlend,
                value =>
                {
                    _lockBlend = value;
                    ApplyRigBlend();
                },
                0f,
                _cameraData.LockBlendDuration)
                .SetEase(_cameraData.LockBlendEase);

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
            return targetCamera;
        }

        public void UpdateRotation(Vector2 look)
        {
            if (_lockOnTargetEntityId.HasValue)
            {
                if (TryGetLockTarget(out TargetingSnapshot snapshot))
                {
                    UpdateStableLockDirection(snapshot);
                    UpdateLockBodyYaw(Time.deltaTime);
                    UpdateLockBodyPitch(snapshot, Time.deltaTime);
                    UpdateLockLookAtTarget(snapshot, Time.deltaTime);
                }
                else
                {
                    ClearLockOnTarget();
                }
            }

            if (!_lockOnTargetEntityId.HasValue && look.sqrMagnitude > 0f && !_cameraData.LockCameraPosition)
            {
                ApplyFreeLook(look);
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            float minPitch = Mathf.Lerp(_cameraData.BottomClamp, Mathf.Max(_cameraData.BottomClamp, _cameraData.HumanoidLockProfile.MinPitch), _lockBlend);
            float maxPitch = Mathf.Lerp(_cameraData.TopClamp, Mathf.Min(_cameraData.TopClamp, _cameraData.HumanoidLockProfile.MaxPitch), _lockBlend);

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, minPitch, maxPitch);
            ApplyLogicalRotation();

#if UNITY_EDITOR
            UpdateDiagnostics();
#endif
        }

        private void ApplyFreeLook(Vector2 look)
        {
            if (_inputService.CharacterActions.Look.activeControl.device is Pointer)
            {
                _cinemachineTargetYaw += look.x * _cameraData.MouseYawDegreesPerPixel;
                _cinemachineTargetPitch += look.y * _cameraData.MousePitchDegreesPerPixel;
                return;
            }

            _cinemachineTargetYaw += look.x * _cameraData.StickYawDegreesPerSecond * Time.deltaTime;
            _cinemachineTargetPitch += look.y * _cameraData.StickPitchDegreesPerSecond * Time.deltaTime;
        }

        private void UpdateStableLockDirection(TargetingSnapshot snapshot)
        {
            Vector3 toRoot = snapshot.Position - _followTarget.position;
            Vector3 planarDirection = Vector3.ProjectOnPlane(toRoot, Vector3.up);
            float planarDistance = planarDirection.magnitude;

            if (_holdingCloseHeading && planarDistance >= _cameraData.LockHeadingReleaseDistance)
            {
                _holdingCloseHeading = false;
            }
            else if (!_holdingCloseHeading && planarDistance <= _cameraData.LockHeadingHoldDistance)
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

        private void UpdateLockBodyYaw(float deltaTime)
        {
            float targetYaw = Mathf.Atan2(_stableLockDirection.x, _stableLockDirection.z) * Mathf.Rad2Deg;
            float yawDelta = Mathf.DeltaAngle(_cinemachineTargetYaw, targetYaw);

            if (Mathf.Abs(Mathf.Abs(yawDelta) - 180f) <= _cameraData.LockYawHalfTurnTolerance && _lastLockYawTurnSign != 0)
            {
                yawDelta = Mathf.Abs(yawDelta) * _lastLockYawTurnSign;
            }
            else if (Mathf.Abs(yawDelta) > 0.001f)
            {
                _lastLockYawTurnSign = yawDelta > 0f ? 1 : -1;
            }

            float enterAngle = _cameraData.LockOrbitYawEnterAngle;
            float releaseAngle = _cameraData.LockOrbitYawReleaseAngle;
            float absError = Mathf.Abs(yawDelta);

            if (!_isYawOrbiting && absError >= enterAngle)
            {
                _isYawOrbiting = true;
            }
            else if (_isYawOrbiting && absError <= releaseAngle)
            {
                _isYawOrbiting = false;
            }

            float targetAngle = _cinemachineTargetYaw;
            if (_isYawOrbiting)
            {
                targetAngle = _cinemachineTargetYaw + (yawDelta - Mathf.Sign(yawDelta) * releaseAngle);
            }

            _cinemachineTargetYaw = Mathf.SmoothDampAngle(
                _cinemachineTargetYaw,
                targetAngle,
                ref _yawVelocity,
                _cameraData.LockOrbitYawSmoothTime,
                _cameraData.LockOrbitYawMaxSpeed,
                deltaTime);
        }

        private void UpdateLockBodyPitch(TargetingSnapshot snapshot, float deltaTime)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            float targetElevation = Mathf.Atan2(toTarget.y, Mathf.Max(planarDistance, _cameraData.LockMinPitchDistance)) * Mathf.Rad2Deg;
            float influence = Mathf.Lerp(
                _cameraData.LockCloseVerticalInfluence,
                _cameraData.LockFarVerticalInfluence,
                Mathf.InverseLerp(_cameraData.LockVerticalCloseDistance, _cameraData.LockVerticalFarDistance, planarDistance));
            float desiredPitch = _cameraData.LockBasePitch - targetElevation * influence;
            float pitchError = desiredPitch - _cinemachineTargetPitch;
            float absPitchError = Mathf.Abs(pitchError);

            float pitchEnter = _cameraData.LockOrbitPitchEnterAngle;
            float pitchRelease = _cameraData.LockOrbitPitchReleaseAngle;

            if (!_isPitchOrbiting && absPitchError >= pitchEnter)
            {
                _isPitchOrbiting = true;
            }
            else if (_isPitchOrbiting && absPitchError <= pitchRelease)
            {
                _isPitchOrbiting = false;
            }

            float targetPitchAngle = _cinemachineTargetPitch;
            if (_isPitchOrbiting)
            {
                targetPitchAngle = _cinemachineTargetPitch + (pitchError - Mathf.Sign(pitchError) * pitchRelease);
            }

            _cinemachineTargetPitch = Mathf.SmoothDampAngle(
                _cinemachineTargetPitch,
                targetPitchAngle,
                ref _pitchVelocity,
                _cameraData.LockOrbitPitchSmoothTime,
                _cameraData.LockOrbitPitchMaxSpeed,
                deltaTime);
        }

        private void UpdateLockLookAtTarget(TargetingSnapshot snapshot, float deltaTime)
        {
            Vector3 desiredLockPoint = snapshot.LockPoint;
            float heightOffset = desiredLockPoint.y - _followTarget.position.y;
            float clampedHeight = Mathf.Clamp(heightOffset, _cameraData.LockMinFocusHeight, _cameraData.LockMaxFocusHeight);
            desiredLockPoint.y = _followTarget.position.y + clampedHeight;

            _filteredLockPoint = Vector3.SmoothDamp(
                _filteredLockPoint,
                desiredLockPoint,
                ref _lockPointVelocity,
                _cameraData.LockAimSmoothTime,
                _cameraData.LockAimMaxSpeed,
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

        private void CaptureCurrentRig()
        {
            _freeRigProfile = new CameraData.CameraRigProfile
            {
                ShoulderOffset = cinemachineThirdPersonFollow.ShoulderOffset,
                VerticalArmLength = cinemachineThirdPersonFollow.VerticalArmLength,
                CameraDistance = cinemachineThirdPersonFollow.CameraDistance,
                CameraSide = cinemachineThirdPersonFollow.CameraSide,
                FieldOfView = cinemachineCamera.Lens.FieldOfView,
                MinPitch = _cameraData.BottomClamp,
                MaxPitch = _cameraData.TopClamp
            };
        }

        private void BeginLockTargetChange()
        {
            _lockPointVelocity *= 0.5f;
            _holdingCloseHeading = false;
            _hasStableLockDirection = false;
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
        }

        private void ApplyRigBlend()
        {
            cinemachineThirdPersonFollow.ShoulderOffset = Vector3.Lerp(
                _freeRigProfile.ShoulderOffset,
                _cameraData.HumanoidLockProfile.ShoulderOffset,
                _lockBlend);
            cinemachineThirdPersonFollow.VerticalArmLength = Mathf.Lerp(
                _freeRigProfile.VerticalArmLength,
                _cameraData.HumanoidLockProfile.VerticalArmLength,
                _lockBlend);
            cinemachineThirdPersonFollow.CameraDistance = Mathf.Lerp(
                _freeRigProfile.CameraDistance,
                _cameraData.HumanoidLockProfile.CameraDistance,
                _lockBlend);
            cinemachineThirdPersonFollow.CameraSide = Mathf.Lerp(
                _freeRigProfile.CameraSide,
                _cameraData.HumanoidLockProfile.CameraSide,
                _lockBlend);

            if (!_isZoomed)
            {
                SetFieldOfView(Mathf.Lerp(
                    _freeRigProfile.FieldOfView,
                    _cameraData.HumanoidLockProfile.FieldOfView,
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
                    _cinemachineTargetPitch + _cameraData.CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0f);
            }

            if (!_lockOnTargetEntityId.HasValue && _lockLookAtTarget != null && _followTarget != null)
            {
                Vector3 forward = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f) * Vector3.forward;
                _lockLookAtTarget.position = _followTarget.position + forward * _cameraData.LockInitialFocusMinDistance;
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
        }
#endif
    }
}
