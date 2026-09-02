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
        private const float LOCK_TARGET_ACQUISITION_MAX_DURATION_MULTIPLIER = 4f;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;
        private Tween _switchTween;
        private Tween _zoomTween;
        private Tween _rigTween;
        private Tween _rigFovTween;
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
        private Vector3 _smoothedFocusOffset;
        private Vector3 _focusOffsetVelocity;
        private Vector3 _stableLockDirection;
        private bool _wasGrounded = true;
        private bool _hasStableLockDirection;
        private bool _holdingCloseHeading;
        private bool _isLockTargetAcquiring;
        private bool _isZoomed;
        private int _lastLockYawTurnSign;
        private float _lockTargetAcquisitionElapsed;

        private void OnDestroy()
        {
            _switchTween?.Kill();
            _zoomTween?.Kill();
            _rigTween?.Kill();
            _rigFovTween?.Kill();
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
            _lockLookAtTarget.position = _followTarget.position + _followTarget.forward * _cameraData.LockMinFocusDistance;
            CaptureCurrentRig();
            ResetLockState();
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

            float currentFreeSide = _lockOnTargetEntityId.HasValue
                ? _freeRigProfile.CameraSide
                : cinemachineThirdPersonFollow.CameraSide;
            float targetSide = currentFreeSide < 0.5f ? 1.0f : 0.0f;
            _freeRigProfile.CameraSide = targetSide;
            if (_lockOnTargetEntityId.HasValue)
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
            _rigFovTween?.Kill();
            _zoomTween?.Kill();

            float targetFov = isZoomed ? _cameraData.ZoomFov : GetActiveRigProfile().FieldOfView;
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

            bool targetChanged = _lockOnTargetEntityId != targetEntityId;
            bool wasUnlocked = !_lockOnTargetEntityId.HasValue;
            _lockOnTargetEntityId = targetEntityId;

            if (targetChanged)
            {
                ResetLockState();
                InitializeLockLookAtTarget();
            }

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
                BlendRig(_cameraData.HumanoidLockProfile);
                cinemachineCamera.PreviousStateIsValid = false;
            }

            cinemachineCamera.LookAt = _lockLookAtTarget;
        }

        public void ClearLockOnTarget()
        {
            if (!_lockOnTargetEntityId.HasValue)
            {
                return;
            }

            _lockOnTargetEntityId = null;
            cinemachineCamera.LookAt = null;
            ResetLockState();
            BlendRig(_freeRigProfile);
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
                    UpdateLockBodyYaw();
                    UpdateLockBodyPitch(snapshot);
                    UpdateLockLookAtTarget(snapshot);
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
            float minPitch = _cameraData.BottomClamp;
            float maxPitch = _cameraData.TopClamp;
            if (_lockOnTargetEntityId.HasValue)
            {
                minPitch = Mathf.Max(minPitch, _cameraData.HumanoidLockProfile.MinPitch);
                maxPitch = Mathf.Min(maxPitch, _cameraData.HumanoidLockProfile.MaxPitch);
            }

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, minPitch, maxPitch);
            ApplyLogicalRotation();
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
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            Vector3 planarDirection = Vector3.ProjectOnPlane(toTarget, Vector3.up);
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

        private void UpdateLockBodyYaw()
        {
            float targetYaw = Mathf.Atan2(_stableLockDirection.x, _stableLockDirection.z) * Mathf.Rad2Deg;
            float yawDelta = Mathf.DeltaAngle(_cinemachineTargetYaw, targetYaw);
            if (Mathf.Abs(Mathf.Abs(yawDelta) - 180f) <= _cameraData.LockYawHalfTurnTolerance && _lastLockYawTurnSign != 0)
            {
                yawDelta = Mathf.Abs(yawDelta) * _lastLockYawTurnSign;
            }
            else if (Mathf.Abs(yawDelta) > 0f)
            {
                _lastLockYawTurnSign = yawDelta > 0f ? 1 : -1;
            }

            float targetAngle;
            if (_isLockTargetAcquiring)
            {
                targetAngle = _cinemachineTargetYaw + yawDelta;
            }
            else
            {
                float deadZone = _cameraData.LockYawDeadZoneDegrees;
                if (Mathf.Abs(yawDelta) <= deadZone)
                {
                    targetAngle = _cinemachineTargetYaw;
                }
                else
                {
                    targetAngle = _cinemachineTargetYaw + (yawDelta - Mathf.Sign(yawDelta) * deadZone);
                }
            }

            _cinemachineTargetYaw = Mathf.SmoothDampAngle(
                _cinemachineTargetYaw,
                targetAngle,
                ref _yawVelocity,
                _cameraData.LockYawSmoothTime,
                _cameraData.LockYawMaxSpeed,
                Time.deltaTime);
        }

        private void UpdateLockBodyPitch(TargetingSnapshot snapshot)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            float targetElevation = Mathf.Atan2(toTarget.y, Mathf.Max(planarDistance, _cameraData.LockMinPitchDistance)) * Mathf.Rad2Deg;
            float influence = Mathf.Lerp(
                _cameraData.LockCloseVerticalInfluence,
                _cameraData.LockFarVerticalInfluence,
                Mathf.InverseLerp(_cameraData.LockVerticalCloseDistance, _cameraData.LockVerticalFarDistance, planarDistance));
            float desiredPitch = Mathf.Clamp(
                _cameraData.LockBasePitch - targetElevation * influence,
                _cameraData.HumanoidLockProfile.MinPitch,
                _cameraData.HumanoidLockProfile.MaxPitch);

            _cinemachineTargetPitch = Mathf.SmoothDampAngle(
                _cinemachineTargetPitch,
                desiredPitch,
                ref _pitchVelocity,
                _cameraData.LockPitchSmoothTime,
                _cameraData.LockPitchMaxSpeed,
                Time.deltaTime);
        }

        private void UpdateLockLookAtTarget(TargetingSnapshot snapshot)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            Vector3 desiredOffset = _stableLockDirection * Mathf.Max(planarDistance, _cameraData.LockMinFocusDistance);
            desiredOffset.y = Mathf.Clamp(toTarget.y, _cameraData.LockMinFocusHeight, _cameraData.LockMaxFocusHeight);

            _smoothedFocusOffset = Vector3.SmoothDamp(
                _smoothedFocusOffset,
                desiredOffset,
                ref _focusOffsetVelocity,
                _cameraData.LockTargetSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            if (_isLockTargetAcquiring)
            {
                _lockTargetAcquisitionElapsed += Time.deltaTime;
                float targetYaw = Mathf.Atan2(_stableLockDirection.x, _stableLockDirection.z) * Mathf.Rad2Deg;
                float yawDelta = Mathf.Abs(Mathf.DeltaAngle(_cinemachineTargetYaw, targetYaw));
                if (yawDelta <= _cameraData.LockYawDeadZoneDegrees
                    || _lockTargetAcquisitionElapsed >= _cameraData.LockTargetSmoothTime * LOCK_TARGET_ACQUISITION_MAX_DURATION_MULTIPLIER)
                {
                    _isLockTargetAcquiring = false;
                }
            }

            _lockLookAtTarget.position = _followTarget.position + _smoothedFocusOffset;
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

        private CameraData.CameraRigProfile GetActiveRigProfile()
        {
            return _lockOnTargetEntityId.HasValue ? _cameraData.HumanoidLockProfile : _freeRigProfile;
        }

        private void BlendRig(CameraData.CameraRigProfile profile)
        {
            _switchTween?.Kill();
            _rigTween?.Kill();
            _rigTween = DOTween.Sequence()
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.ShoulderOffset,
                    value => cinemachineThirdPersonFollow.ShoulderOffset = value,
                    profile.ShoulderOffset,
                    _cameraData.LockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.VerticalArmLength,
                    value => cinemachineThirdPersonFollow.VerticalArmLength = value,
                    profile.VerticalArmLength,
                    _cameraData.LockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraDistance,
                    value => cinemachineThirdPersonFollow.CameraDistance = value,
                    profile.CameraDistance,
                    _cameraData.LockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraSide,
                    value => cinemachineThirdPersonFollow.CameraSide = value,
                    profile.CameraSide,
                    _cameraData.LockRigBlendDuration))
                .SetEase(_cameraData.LockRigBlendEase);

            if (!_isZoomed)
            {
                _zoomTween?.Kill();
                _rigFovTween?.Kill();
                _rigFovTween = DOTween.To(
                    () => cinemachineCamera.Lens.FieldOfView,
                    SetFieldOfView,
                    profile.FieldOfView,
                    _cameraData.LockRigBlendDuration)
                    .SetEase(_cameraData.LockRigBlendEase);
            }
        }

        private void SetFieldOfView(float fieldOfView)
        {
            var lens = cinemachineCamera.Lens;
            lens.FieldOfView = fieldOfView;
            cinemachineCamera.Lens = lens;
        }

        private void InitializeLockLookAtTarget()
        {
            float initialDistance = _cameraData.LockMinFocusDistance;
            float targetHeight = 0f;
            if (TryGetLockTarget(out TargetingSnapshot snapshot))
            {
                Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
                initialDistance = Mathf.Max(new Vector2(toTarget.x, toTarget.z).magnitude, _cameraData.LockMinFocusDistance);
                targetHeight = Mathf.Clamp(toTarget.y, _cameraData.LockMinFocusHeight, _cameraData.LockMaxFocusHeight);
            }

            Vector3 forwardPlanar = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f) * Vector3.forward;
            _smoothedFocusOffset = forwardPlanar * initialDistance;
            _smoothedFocusOffset.y = targetHeight;
            _focusOffsetVelocity = Vector3.zero;
            _lockTargetAcquisitionElapsed = 0f;
            _isLockTargetAcquiring = true;
            _lockLookAtTarget.position = _followTarget.position + _smoothedFocusOffset;
        }

        private void ResetLockState()
        {
            _yawVelocity = 0f;
            _pitchVelocity = 0f;
            _focusOffsetVelocity = Vector3.zero;
            _smoothedFocusOffset = Vector3.zero;
            _stableLockDirection = Vector3.zero;
            _hasStableLockDirection = false;
            _holdingCloseHeading = false;
            _isLockTargetAcquiring = false;
            _lockTargetAcquisitionElapsed = 0f;
            _lastLockYawTurnSign = 0;
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
                _lockLookAtTarget.position = _followTarget.position + forward * _cameraData.LockMinFocusDistance;
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
    }
}
