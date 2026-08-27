using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
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
    
    public class CameraService: MonoBehaviour, ICameraService
    {
        private const float THRESHOLD = 0.01f;
        private const float MIN_FALL_FOLLOW_WEIGHT = 0.35f;
        
        [Header("Switch Angle")]
        [SerializeField] private float switchAngleDuration = 0.4f;
        [SerializeField] private Ease switchAngleEase = Ease.InOutQuad;
        
        private Tween _switchTween;
        private Tween _zoomTween;
        
        [Header("Zoom")]
        [SerializeField] private float zoomFov = 30f;
        [SerializeField] private float normalFov = 60f;
        [SerializeField] private float zoomDuration = 0.3f;
        [SerializeField] private Ease zoomEase = Ease.OutSine;
        
        [SerializeField] private Camera targetCamera;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

        [Header("Vertical Follow")]
        [SerializeField, Min(0f)] private float fallFollowThreshold = 1.25f;
        [SerializeField, Min(0.01f)] private float groundedFollowDamping = 0.3f;
        [SerializeField, Min(0.01f)] private float fallFollowDamping = 0.45f;
        [SerializeField, Min(0.01f)] private float longFallFollowDistance = 4f;
  
        [Header("Cinemachine")]
        
        [Tooltip("How far in degrees can you move the camera up")]
        public float topClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float bottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float cameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool lockCameraPosition = false;
        
        [Range(0.1f,10.0f)] [SerializeField] private float mouseSensitivityY = 1.0f;
        [Range(0.1f,10.0f)] [SerializeField] private float mouseSensitivityX = 1.0f;

        [Header("Lock On")]
        [SerializeField, Range(0f, 0.45f)] private float lockHorizontalDeadZone = 0.1f;
        [SerializeField, Range(0f, 0.45f)] private float lockVerticalDeadZone = 0.08f;
        [SerializeField, Range(0f, 1f)] private float lockTargetViewportY = 0.58f;
        [SerializeField, Min(0f)] private float lockYawSpeed = 150f;
        [SerializeField, Min(0f)] private float lockPitchSpeed = 90f;
        [SerializeField] private float lockVerticalArmLength = 0.65f;
        [SerializeField, Min(0.01f)] private float lockYawSmoothTime = 0.12f;
        [SerializeField, Min(0.01f)] private float lockPitchSmoothTime = 0.2f;
        [SerializeField, Min(0f)] private float lockCloseTrackingDistance = 1.25f;
        [SerializeField, Min(0.01f)] private float lockCloseTrackingBlendRange = 1.75f;
        [SerializeField, Range(1f, 90f)] private float lockMaxYawCorrection = 35f;
        [SerializeField, Range(1f, 80f)] private float lockMaxPitchCorrection = 25f;
        [SerializeField, Range(0f, 1f)] private float lockCameraSide = 0.5f;
        [SerializeField, Min(0f)] private float lockRigBlendDuration = 0.2f;
        [SerializeField] private Ease lockRigBlendEase = Ease.OutSine;
        
        private long? _lockOnTargetEntityId;
        private IEntityLocator _entityLocator;

        [Inject]
        public void Construct(IEntityLocator entityLocator) => _entityLocator = entityLocator;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _freeVerticalArmLength;
        private float _freeCameraSide;
        private float _freeCameraDistance;
        private float _yawVelocity;
        private float _pitchVelocity;
        private Tween _lockRigTween;
        private Transform _sourceTarget;
        private Transform _followTarget;
        private float _airborneStartY;
        private float _followYVelocity;
        private bool _wasGrounded = true;

        public void SetTarget(Transform target)
        {
            _sourceTarget = target;

            if (_followTarget == null)
            {
                _followTarget = new GameObject("Camera Follow Target").transform;
                _followTarget.SetParent(transform);
            }

            _followTarget.SetPositionAndRotation(target.position, target.rotation);
            _airborneStartY = target.position.y;
            _followYVelocity = 0f;
            _wasGrounded = true;
            cinemachineCamera.Follow = _followTarget;
            cinemachineCamera.LookAt = null;
        }

        public void UpdateFollowTarget(bool grounded, float verticalVelocity)
        {
            Vector3 sourcePosition = _sourceTarget.position;
            Vector3 followPosition = _followTarget.position;
            followPosition.x = sourcePosition.x;
            followPosition.z = sourcePosition.z;

            if (grounded)
            {
                followPosition.y = Mathf.SmoothDamp(followPosition.y, sourcePosition.y,
                    ref _followYVelocity, groundedFollowDamping);
            }
            else
            {
                if (_wasGrounded)
                {
                    _airborneStartY = sourcePosition.y;
                    _followYVelocity = 0f;
                }

                float downwardDisplacement = _airborneStartY - sourcePosition.y;
                if (verticalVelocity < 0f && downwardDisplacement > fallFollowThreshold)
                {
                    float longFallBlend = Mathf.InverseLerp(fallFollowThreshold,
                        fallFollowThreshold + longFallFollowDistance, downwardDisplacement);
                    float followWeight = Mathf.Lerp(MIN_FALL_FOLLOW_WEIGHT, 1f, longFallBlend);
                    float desiredY = Mathf.Lerp(followPosition.y, sourcePosition.y, followWeight);
                    followPosition.y = Mathf.SmoothDamp(followPosition.y, desiredY,
                        ref _followYVelocity, fallFollowDamping);
                }
                else
                {
                    _followYVelocity = 0f;
                }
            }

            _followTarget.position = followPosition;
            _wasGrounded = grounded;
        }
        
        public float GetYaw()
        {
            return targetCamera.transform.eulerAngles.y;
        }

        public float GetPitch()
        {
            return targetCamera.transform.eulerAngles.x;
        }

        public void SwitchAngle()
        {
            _switchTween?.Kill();
            
            float targetSide = cinemachineThirdPersonFollow.CameraSide < 0.5f ? 1.0f : 0.0f;
            
            _switchTween = DOTween.To(
                () => cinemachineThirdPersonFollow.CameraSide,
                x => cinemachineThirdPersonFollow.CameraSide = x,
                targetSide,
                switchAngleDuration
            ).SetEase(switchAngleEase);
        }

        public Ray GetRay()
        {
            return targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        }

        public void SetZoom(bool isZoomed)
        {
            _zoomTween?.Kill();
            
            float targetFov = isZoomed ? zoomFov : normalFov;
            
            _zoomTween = DOTween.To(
                () => cinemachineCamera.Lens.FieldOfView,
                x => {
                    var lens = cinemachineCamera.Lens;
                    lens.FieldOfView = x;
                    cinemachineCamera.Lens = lens;
                },
                targetFov,
                zoomDuration
            ).SetEase(zoomEase);
        }

        public void SetLockOnTarget(long? targetEntityId)
        {
            if (!targetEntityId.HasValue)
            {
                ClearLockOnTarget();
                return;
            }

            bool wasUnlocked = !_lockOnTargetEntityId.HasValue;
            _lockOnTargetEntityId = targetEntityId;

            if (wasUnlocked)
            {
                _freeVerticalArmLength = cinemachineThirdPersonFollow.VerticalArmLength;
                _freeCameraSide = cinemachineThirdPersonFollow.CameraSide;
                _freeCameraDistance = cinemachineThirdPersonFollow.CameraDistance;
                _yawVelocity = 0f;
                _pitchVelocity = 0f;
                cinemachineCamera.LookAt = null;
                BlendLockRig(lockVerticalArmLength, lockCameraSide);
            }
        }

        public void ClearLockOnTarget()
        {
            if (!_lockOnTargetEntityId.HasValue)
            {
                return;
            }

            _lockOnTargetEntityId = null;
            cinemachineCamera.LookAt = null;
            BlendFreeRig();
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
            if (_lockOnTargetEntityId.HasValue && cinemachineCamera.Follow != null)
            {
                UpdateLockOnRig();
            }
            else
            {
                // Unlocked Mode: Manual camera stick/mouse input
                if (look.sqrMagnitude >= THRESHOLD && !lockCameraPosition)
                {
                    _cinemachineTargetYaw += look.x * Time.deltaTime * mouseSensitivityX;
                    _cinemachineTargetPitch += look.y * Time.deltaTime * mouseSensitivityY;
                }
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            // Cinemachine will follow this target
            if (cinemachineCamera.Follow != null)
            {
                cinemachineCamera.Follow.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
                    _cinemachineTargetYaw, 0.0f);
            }
        }

        private void UpdateLockOnRig()
        {
            if (!TryGetLockTarget(out TargetingSnapshot snapshot))
            {
                ClearLockOnTarget();
                return;
            }

            float targetTrackingWeight = GetCloseTrackingWeight(snapshot.LockPoint);
            float behindPlayerWeight = 1f - targetTrackingWeight;
            float yawCorrection = GetBehindPlayerYawCorrection(behindPlayerWeight);
            float pitchCorrection = Mathf.DeltaAngle(_cinemachineTargetPitch, 0f) * behindPlayerWeight;

            Vector3 targetViewportPosition = targetCamera.WorldToViewportPoint(snapshot.LockPoint);
            if (targetViewportPosition.z <= 0f)
            {
                yawCorrection += GetTargetYawCorrection(snapshot.LockPoint) * targetTrackingWeight;
                ApplyLockYawCorrection(yawCorrection);
                ApplyLockPitchCorrection(pitchCorrection);
                return;
            }

            if (targetTrackingWeight > THRESHOLD)
            {
                float horizontalError = GetDeadZoneError(
                    targetViewportPosition.x - 0.5f,
                    lockHorizontalDeadZone);

                if (Mathf.Abs(horizontalError) > THRESHOLD)
                {
                    yawCorrection += ViewportOffsetToAngle(horizontalError, GetHorizontalFieldOfView(targetCamera))
                        * targetTrackingWeight;
                }

                float verticalError = GetDeadZoneError(
                    targetViewportPosition.y - lockTargetViewportY,
                    lockVerticalDeadZone);

                if (Mathf.Abs(verticalError) > THRESHOLD)
                {
                    pitchCorrection -= ViewportOffsetToAngle(verticalError, targetCamera.fieldOfView)
                        * targetTrackingWeight;
                }
            }

            ApplyLockYawCorrection(yawCorrection);
            ApplyLockPitchCorrection(pitchCorrection);
        }

        private float GetBehindPlayerYawCorrection(float behindPlayerWeight)
        {
            if (behindPlayerWeight <= THRESHOLD)
            {
                return 0f;
            }

            Vector3 playerForward = Vector3.ProjectOnPlane(_sourceTarget.forward, Vector3.up);
            if (playerForward.sqrMagnitude <= THRESHOLD)
            {
                return 0f;
            }

            float playerYaw = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(_cinemachineTargetYaw, playerYaw) * behindPlayerWeight;
        }

        private float GetTargetYawCorrection(Vector3 lockPoint)
        {
            Vector3 toTarget = lockPoint - cinemachineCamera.Follow.position;
            Vector3 planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            if (planarToTarget.sqrMagnitude <= THRESHOLD)
            {
                return 0f;
            }

            float targetYaw = Mathf.Atan2(planarToTarget.x, planarToTarget.z) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(_cinemachineTargetYaw, targetYaw);
        }

        private void ApplyLockYawCorrection(float yawCorrection)
        {
            if (Mathf.Abs(yawCorrection) <= THRESHOLD)
            {
                _yawVelocity = 0f;
                return;
            }

            yawCorrection = Mathf.Clamp(
                yawCorrection,
                -lockMaxYawCorrection,
                lockMaxYawCorrection);

            _cinemachineTargetYaw = Mathf.SmoothDampAngle(
                _cinemachineTargetYaw,
                _cinemachineTargetYaw + yawCorrection,
                ref _yawVelocity,
                lockYawSmoothTime,
                lockYawSpeed,
                Time.deltaTime);
        }

        private void ApplyLockPitchCorrection(float pitchCorrection)
        {
            if (Mathf.Abs(pitchCorrection) <= THRESHOLD)
            {
                _pitchVelocity = 0f;
                return;
            }

            pitchCorrection = Mathf.Clamp(
                pitchCorrection,
                -lockMaxPitchCorrection,
                lockMaxPitchCorrection);

            _cinemachineTargetPitch = Mathf.SmoothDampAngle(
                _cinemachineTargetPitch,
                _cinemachineTargetPitch + pitchCorrection,
                ref _pitchVelocity,
                lockPitchSmoothTime,
                lockPitchSpeed,
                Time.deltaTime);
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
                throw new System.InvalidOperationException(
                    $"Target entity {entity.Id} ({entity.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            snapshot = command.Read();
            return snapshot.IsAlive;
        }

        private void BlendLockRig(float verticalArmLength, float cameraSide)
        {
            _switchTween?.Kill();
            _lockRigTween?.Kill();
            _lockRigTween = DOTween.Sequence()
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.VerticalArmLength,
                    value => cinemachineThirdPersonFollow.VerticalArmLength = value,
                    verticalArmLength,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraSide,
                    value => cinemachineThirdPersonFollow.CameraSide = value,
                    cameraSide,
                    lockRigBlendDuration))
                .SetEase(lockRigBlendEase);
        }

        private void BlendFreeRig()
        {
            _switchTween?.Kill();
            _lockRigTween?.Kill();
            _lockRigTween = DOTween.Sequence()
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.VerticalArmLength,
                    value => cinemachineThirdPersonFollow.VerticalArmLength = value,
                    _freeVerticalArmLength,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraSide,
                    value => cinemachineThirdPersonFollow.CameraSide = value,
                    _freeCameraSide,
                    lockRigBlendDuration))
                .Join(DOTween.To(
                    () => cinemachineThirdPersonFollow.CameraDistance,
                    value => cinemachineThirdPersonFollow.CameraDistance = value,
                    _freeCameraDistance,
                    lockRigBlendDuration))
                .SetEase(lockRigBlendEase);
        }

        private static float GetDeadZoneError(float offset, float deadZone)
        {
            if (Mathf.Abs(offset) <= deadZone)
            {
                return 0f;
            }

            return offset - Mathf.Sign(offset) * deadZone;
        }

        private float GetCloseTrackingWeight(Vector3 lockPoint)
        {
            Vector3 toTarget = lockPoint - cinemachineCamera.Follow.position;
            float planarDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            return Mathf.Clamp01((planarDistance - lockCloseTrackingDistance) / lockCloseTrackingBlendRange);
        }

        private static float ViewportOffsetToAngle(float viewportOffset, float fieldOfView)
        {
            float halfFieldOfViewTangent = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            return Mathf.Atan(viewportOffset * 2f * halfFieldOfViewTangent) * Mathf.Rad2Deg;
        }

        private static float GetHorizontalFieldOfView(Camera camera)
        {
            float verticalHalfAngle = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            return Mathf.Atan(Mathf.Tan(verticalHalfAngle) * camera.aspect) * 2f * Mathf.Rad2Deg;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

    }
}
