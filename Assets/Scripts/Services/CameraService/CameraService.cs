using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;

namespace SoulsLike.Services.CameraService
{
    public interface ICameraService
    {
        void SetTarget(Transform target);
        void UpdateRotation(Vector2 look);
        float GetYaw();
        float GetPitch();
        void SwitchAngle();
        Ray GetRay();
        void SetZoom(bool isZoomed);
        void SetLockOnTarget(Transform lockNodeTarget);
        void ClearLockOnTarget();
        void RecenterCamera();
        Camera GetMainCamera();
    }
    
    public class CameraService: MonoBehaviour, ICameraService
    {
        private const float THRESHOLD = 0.01f;
        
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
        [SerializeField, Range(0f, 0.45f)] private float lockHorizontalDeadZone = 0.12f;
        [SerializeField, Range(0f, 0.45f)] private float lockVerticalDeadZone = 0.08f;
        [SerializeField, Range(0f, 1f)] private float lockTargetViewportY = 0.58f;
        [SerializeField, Min(0f)] private float lockYawSpeed = 150f;
        [SerializeField, Min(0f)] private float lockPitchSpeed = 90f;
        [SerializeField] private float lockVerticalArmLength = 0.65f;
        [SerializeField, Range(0f, 1f)] private float lockCameraSide = 0.5f;
        [SerializeField, Min(0f)] private float lockRigBlendDuration = 0.2f;
        [SerializeField] private Ease lockRigBlendEase = Ease.OutSine;
        
        private Transform _lockOnTarget;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _freeVerticalArmLength;
        private float _freeCameraSide;
        private Tween _lockRigTween;

        public void SetTarget(Transform target)
        {
            cinemachineCamera.Follow = target;
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

        public void SetLockOnTarget(Transform lockNodeTarget)
        {
            if (_lockOnTarget == null)
            {
                _freeVerticalArmLength = cinemachineThirdPersonFollow.VerticalArmLength;
                _freeCameraSide = cinemachineThirdPersonFollow.CameraSide;
                BlendLockRig(lockVerticalArmLength, lockCameraSide);
            }

            _lockOnTarget = lockNodeTarget;
        }

        public void ClearLockOnTarget()
        {
            _lockOnTarget = null;
            BlendLockRig(_freeVerticalArmLength, _freeCameraSide);
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
            if (_lockOnTarget != null && cinemachineCamera.Follow != null)
            {
                UpdateLockOnRotation();
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

        private void UpdateLockOnRotation()
        {
            Vector3 targetViewportPosition = targetCamera.WorldToViewportPoint(_lockOnTarget.position);
            if (targetViewportPosition.z <= 0f)
            {
                RotateLockYawTowardTarget();
                return;
            }

            float horizontalError = GetDeadZoneError(
                targetViewportPosition.x - 0.5f,
                lockHorizontalDeadZone);
            float verticalError = GetDeadZoneError(
                targetViewportPosition.y - lockTargetViewportY,
                lockVerticalDeadZone);

            float horizontalFieldOfView = GetHorizontalFieldOfView(targetCamera.fieldOfView, targetCamera.aspect);
            float yawCorrection = ViewportOffsetToAngle(horizontalError, horizontalFieldOfView);
            float pitchCorrection = ViewportOffsetToAngle(verticalError, targetCamera.fieldOfView);

            _cinemachineTargetYaw = Mathf.MoveTowardsAngle(
                _cinemachineTargetYaw,
                _cinemachineTargetYaw + yawCorrection,
                lockYawSpeed * Time.deltaTime);
            _cinemachineTargetPitch = Mathf.MoveTowardsAngle(
                _cinemachineTargetPitch,
                _cinemachineTargetPitch - pitchCorrection,
                lockPitchSpeed * Time.deltaTime);
        }

        private void RotateLockYawTowardTarget()
        {
            Vector3 toTarget = _lockOnTarget.position - cinemachineCamera.Follow.position;
            Vector3 planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            if (planarToTarget.sqrMagnitude <= THRESHOLD)
            {
                return;
            }

            float targetYaw = Mathf.Atan2(planarToTarget.x, planarToTarget.z) * Mathf.Rad2Deg;
            _cinemachineTargetYaw = Mathf.MoveTowardsAngle(
                _cinemachineTargetYaw,
                targetYaw,
                lockYawSpeed * Time.deltaTime);
        }

        private void BlendLockRig(float verticalArmLength, float cameraSide)
        {
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

        private static float GetDeadZoneError(float offset, float deadZone)
        {
            if (Mathf.Abs(offset) <= deadZone)
            {
                return 0f;
            }

            return offset - Mathf.Sign(offset) * deadZone;
        }

        private static float ViewportOffsetToAngle(float viewportOffset, float fieldOfView)
        {
            float halfFieldOfViewTangent = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            return Mathf.Atan(viewportOffset * 2f * halfFieldOfViewTangent) * Mathf.Rad2Deg;
        }

        private static float GetHorizontalFieldOfView(float verticalFieldOfView, float aspect)
        {
            float halfVerticalFieldOfViewTangent = Mathf.Tan(verticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            return Mathf.Atan(halfVerticalFieldOfViewTangent * aspect) * 2f * Mathf.Rad2Deg;
        }
        
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

    }
}
