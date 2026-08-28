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

        [Header("Switch Angle")]
        [SerializeField] private float switchAngleDuration = 0.4f;
        [SerializeField] private Ease switchAngleEase = Ease.InOutQuad;

        [Header("Zoom")]
        [SerializeField] private float zoomFov = 30f;
        [SerializeField] private float zoomDuration = 0.3f;
        [SerializeField] private Ease zoomEase = Ease.OutSine;

        [SerializeField] private Camera targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

        [Header("Vertical Follow")]
        [SerializeField, Min(0f)] private float airborneRiseLag = 0.65f;
        [SerializeField, Min(0f)] private float airborneFallLag = 0.40f;
        [SerializeField, Min(0.01f)] private float groundedFollowSmoothTime = 0.10f;
        [SerializeField, Min(0.01f)] private float jumpFollowSmoothTime = 0.22f;
        [SerializeField, Min(0.01f)] private float fallFollowSmoothTime = 0.15f;
        [SerializeField, Min(0.01f)] private float longFallSmoothTime = 0.08f;
        [SerializeField, Min(0f)] private float groundedMaxFollowSpeed = 5f;
        [SerializeField, Min(0f)] private float jumpMaxFollowSpeed = 5f;
        [SerializeField, Min(0f)] private float fallMaxFollowSpeed = 8f;
        [SerializeField, Min(0f)] private float longFallMaxSpeed = 18f;
        [SerializeField, Min(0f)] private float longFallCatchupDistance = 4f;

        [Header("Free Look")]
        [SerializeField, Min(0f)] private float mouseYawDegreesPerPixel = 0.09f;
        [SerializeField, Min(0f)] private float mousePitchDegreesPerPixel = 0.08f;
        [SerializeField, Min(0f)] private float stickYawDegreesPerSecond = 220f;
        [SerializeField, Min(0f)] private float stickPitchDegreesPerSecond = 150f;

        [Header("Cinemachine")]
        [Tooltip("How far in degrees can you move the camera up")]
        public float topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float bottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float cameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool lockCameraPosition = false;

        [Header("Lock On")]
        [SerializeField, Min(0f)] private float lockHeadingHoldDistance = 0.55f;
        [SerializeField, Min(0f)] private float lockHeadingReleaseDistance = 0.90f;
        [SerializeField, Min(0.01f)] private float lockYawSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float lockYawMaxSpeed = 150f;
        [SerializeField] private float lockBasePitch = 6f;
        [SerializeField, Min(0f)] private float lockMinPitchDistance = 1.50f;
        [SerializeField, Min(0f)] private float lockVerticalCloseDistance = 1.25f;
        [SerializeField, Min(0f)] private float lockVerticalFarDistance = 4f;
        [SerializeField, Range(0f, 1f)] private float lockCloseVerticalInfluence = 0.25f;
        [SerializeField, Range(0f, 1f)] private float lockFarVerticalInfluence = 0.65f;
        [SerializeField, Min(0.01f)] private float lockPitchSmoothTime = 0.17f;
        [SerializeField, Min(0f)] private float lockPitchMaxSpeed = 100f;
        [SerializeField, Min(0f)] private float lockMinFocusDistance = 1.50f;
        [SerializeField] private float lockMinFocusHeight = -0.75f;
        [SerializeField] private float lockMaxFocusHeight = 1.25f;
        [SerializeField, Min(0.01f)] private float lockTargetSmoothTime = 0.10f;
        [SerializeField, Min(0f)] private float lockYawHalfTurnTolerance = 2f;
        [SerializeField] private CameraRigProfile humanoidLockProfile = new CameraRigProfile
        {
            shoulderOffset = new Vector3(1f, 0.48f, 0f),
            verticalArmLength = 0f,
            cameraDistance = 3.30f,
            cameraSide = 0.5f,
            fieldOfView = 48f,
            minPitch = -10f,
            maxPitch = 16f
        };
        [SerializeField, Min(0f)] private float lockRigBlendDuration = 0.2f;
        [SerializeField] private Ease lockRigBlendEase = Ease.OutSine;

        private Tween _switchTween;
        private Tween _zoomTween;
        private Tween _rigTween;
        private Tween _rigFovTween;
        private long? _lockOnTargetEntityId;
        private IEntityLocator _entityLocator;
        private IInputService _inputService;
        private Transform _sourceTarget;
        private Transform _followTarget;
        private Transform _lockLookAtTarget;
        private CameraRigProfile _freeRigProfile;
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
        private bool _isZoomed;
        private int _lastLockYawTurnSign;

        [Serializable]
        private struct CameraRigProfile
        {
            public Vector3 shoulderOffset;
            public float verticalArmLength;
            public float cameraDistance;
            public float cameraSide;
            public float fieldOfView;
            public float minPitch;
            public float maxPitch;
        }

        private void OnDestroy()
        {
            _switchTween?.Kill();
            _zoomTween?.Kill();
            _rigTween?.Kill();
            _rigFovTween?.Kill();
        }

        [Inject]
        public void Construct(IEntityLocator entityLocator, IInputService inputService)
        {
            _entityLocator = entityLocator;
            _inputService = inputService;
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
            CaptureCurrentRig();
            ResetLockState();
            cinemachineCamera.Follow = _followTarget;
            cinemachineCamera.LookAt = null;
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
                sourcePosition.y - airborneRiseLag,
                sourcePosition.y + airborneFallLag);
            float smoothTime;
            float maxSpeed;
            if (grounded)
            {
                desiredY = sourcePosition.y;
                smoothTime = groundedFollowSmoothTime;
                maxSpeed = groundedMaxFollowSpeed;
            }
            else if (verticalVelocity >= 0f)
            {
                smoothTime = jumpFollowSmoothTime;
                maxSpeed = jumpMaxFollowSpeed;
            }
            else
            {
                float fallingOvershoot = followPosition.y - (sourcePosition.y + airborneFallLag);
                float longFallProgress = Mathf.InverseLerp(0f, longFallCatchupDistance, fallingOvershoot);
                smoothTime = Mathf.Lerp(fallFollowSmoothTime, longFallSmoothTime, longFallProgress);
                maxSpeed = Mathf.Lerp(fallMaxFollowSpeed, longFallMaxSpeed, longFallProgress);
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
                ? _freeRigProfile.cameraSide
                : cinemachineThirdPersonFollow.CameraSide;
            float targetSide = currentFreeSide < 0.5f ? 1.0f : 0.0f;
            _freeRigProfile.cameraSide = targetSide;
            if (_lockOnTargetEntityId.HasValue)
            {
                return;
            }

            _switchTween = DOTween.To(
                () => cinemachineThirdPersonFollow.CameraSide,
                value => cinemachineThirdPersonFollow.CameraSide = value,
                targetSide,
                switchAngleDuration)
                .SetEase(switchAngleEase);
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

            float targetFov = isZoomed ? zoomFov : GetActiveRigProfile().fieldOfView;
            _zoomTween = DOTween.To(
                () => cinemachineCamera.Lens.FieldOfView,
                SetFieldOfView,
                targetFov,
                zoomDuration)
                .SetEase(zoomEase);
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
                float intendedFreeCameraSide = _freeRigProfile.cameraSide;
                float intendedFreeFieldOfView = _freeRigProfile.fieldOfView;
                CaptureCurrentRig();
                _freeRigProfile.cameraSide = intendedFreeCameraSide;
                if (_isZoomed)
                {
                    _freeRigProfile.fieldOfView = intendedFreeFieldOfView;
                }
                BlendRig(humanoidLockProfile);
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

            if (!_lockOnTargetEntityId.HasValue && look.sqrMagnitude > 0f && !lockCameraPosition)
            {
                ApplyFreeLook(look);
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            float minPitch = bottomClamp;
            float maxPitch = topClamp;
            if (_lockOnTargetEntityId.HasValue)
            {
                minPitch = Mathf.Max(minPitch, humanoidLockProfile.minPitch);
                maxPitch = Mathf.Min(maxPitch, humanoidLockProfile.maxPitch);
            }

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, minPitch, maxPitch);
            ApplyLogicalRotation();
        }

        private void ApplyFreeLook(Vector2 look)
        {
            if (_inputService.CharacterActions.Look.activeControl.device is Pointer)
            {
                _cinemachineTargetYaw += look.x * mouseYawDegreesPerPixel;
                _cinemachineTargetPitch += look.y * mousePitchDegreesPerPixel;
                return;
            }

            _cinemachineTargetYaw += look.x * stickYawDegreesPerSecond * Time.deltaTime;
            _cinemachineTargetPitch += look.y * stickPitchDegreesPerSecond * Time.deltaTime;
        }

        private void UpdateStableLockDirection(TargetingSnapshot snapshot)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            Vector3 planarDirection = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            float planarDistance = planarDirection.magnitude;

            if (_holdingCloseHeading && planarDistance >= lockHeadingReleaseDistance)
            {
                _holdingCloseHeading = false;
            }
            else if (!_holdingCloseHeading && planarDistance <= lockHeadingHoldDistance)
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
            if (Mathf.Abs(Mathf.Abs(yawDelta) - 180f) <= lockYawHalfTurnTolerance && _lastLockYawTurnSign != 0)
            {
                yawDelta = Mathf.Abs(yawDelta) * _lastLockYawTurnSign;
            }
            else if (Mathf.Abs(yawDelta) > 0f)
            {
                _lastLockYawTurnSign = yawDelta > 0f ? 1 : -1;
            }

            _cinemachineTargetYaw = Mathf.SmoothDampAngle(
                _cinemachineTargetYaw,
                _cinemachineTargetYaw + yawDelta,
                ref _yawVelocity,
                lockYawSmoothTime,
                lockYawMaxSpeed,
                Time.deltaTime);
        }

        private void UpdateLockBodyPitch(TargetingSnapshot snapshot)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            float targetElevation = Mathf.Atan2(toTarget.y, Mathf.Max(planarDistance, lockMinPitchDistance)) * Mathf.Rad2Deg;
            float influence = Mathf.Lerp(
                lockCloseVerticalInfluence,
                lockFarVerticalInfluence,
                Mathf.InverseLerp(lockVerticalCloseDistance, lockVerticalFarDistance, planarDistance));
            float desiredPitch = Mathf.Clamp(
                lockBasePitch - targetElevation * influence,
                humanoidLockProfile.minPitch,
                humanoidLockProfile.maxPitch);

            _cinemachineTargetPitch = Mathf.SmoothDampAngle(
                _cinemachineTargetPitch,
                desiredPitch,
                ref _pitchVelocity,
                lockPitchSmoothTime,
                lockPitchMaxSpeed,
                Time.deltaTime);
        }

        private void UpdateLockLookAtTarget(TargetingSnapshot snapshot)
        {
            Vector3 toTarget = snapshot.LockPoint - _followTarget.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            Vector3 desiredOffset = _stableLockDirection * Mathf.Max(planarDistance, lockMinFocusDistance);
            desiredOffset.y = Mathf.Clamp(toTarget.y, lockMinFocusHeight, lockMaxFocusHeight);
            _smoothedFocusOffset = Vector3.SmoothDamp(
                _smoothedFocusOffset,
                desiredOffset,
                ref _focusOffsetVelocity,
                lockTargetSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);
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
            _freeRigProfile = new CameraRigProfile
            {
                shoulderOffset = cinemachineThirdPersonFollow.ShoulderOffset,
                verticalArmLength = cinemachineThirdPersonFollow.VerticalArmLength,
                cameraDistance = cinemachineThirdPersonFollow.CameraDistance,
                cameraSide = cinemachineThirdPersonFollow.CameraSide,
                fieldOfView = cinemachineCamera.Lens.FieldOfView,
                minPitch = bottomClamp,
                maxPitch = topClamp
            };
        }

        private CameraRigProfile GetActiveRigProfile()
        {
            return _lockOnTargetEntityId.HasValue ? humanoidLockProfile : _freeRigProfile;
        }

        private void BlendRig(CameraRigProfile profile)
        {
            _switchTween?.Kill();
            _rigTween?.Kill();
            _rigTween = DOTween.Sequence()
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.ShoulderOffset,
                    value => cinemachineThirdPersonFollow.ShoulderOffset = value,
                    profile.shoulderOffset,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.VerticalArmLength,
                    value => cinemachineThirdPersonFollow.VerticalArmLength = value,
                    profile.verticalArmLength,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraDistance,
                    value => cinemachineThirdPersonFollow.CameraDistance = value,
                    profile.cameraDistance,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraSide,
                    value => cinemachineThirdPersonFollow.CameraSide = value,
                    profile.cameraSide,
                    lockRigBlendDuration))
                .SetEase(lockRigBlendEase);

            if (!_isZoomed)
            {
                _zoomTween?.Kill();
                _rigFovTween?.Kill();
                _rigFovTween = DOTween.To(
                    () => cinemachineCamera.Lens.FieldOfView,
                    SetFieldOfView,
                    profile.fieldOfView,
                    lockRigBlendDuration)
                    .SetEase(lockRigBlendEase);
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
            Vector3 forward = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f) * Vector3.forward;
            _smoothedFocusOffset = forward * lockMinFocusDistance;
            _focusOffsetVelocity = Vector3.zero;
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
            _lastLockYawTurnSign = 0;
        }

        private void ApplyLogicalRotation()
        {
            if (cinemachineCamera.Follow != null)
            {
                cinemachineCamera.Follow.rotation = Quaternion.Euler(
                    _cinemachineTargetPitch + cameraAngleOverride,
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
    }
}
