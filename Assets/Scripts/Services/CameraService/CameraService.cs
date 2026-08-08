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
        
        private Transform _lockOnTarget;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

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
            _lockOnTarget = lockNodeTarget;
        }

        public void ClearLockOnTarget()
        {
            _lockOnTarget = null;
        }

        public void RecenterCamera()
        {
            if (cinemachineCamera.Follow != null)
            {
                _cinemachineTargetYaw = cinemachineCamera.Follow.eulerAngles.y;
                _cinemachineTargetPitch = 0f;
            }
        }

        public void UpdateRotation(Vector2 look)
        {
            if (_lockOnTarget != null && cinemachineCamera.Follow != null)
            {
                // Lock-On Mode: Continuously rotate camera to track target
                Vector3 followPos = cinemachineCamera.Follow.position;
                Vector3 targetPos = _lockOnTarget.position;
                Vector3 toTarget = targetPos - followPos;

                if (toTarget.sqrMagnitude > 0.01f)
                {
                    float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                    
                    // Dynamic pitch based on distance and height elevation
                    float horizontalDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
                    float heightDiff = toTarget.y;
                    float targetPitch = -Mathf.Atan2(heightDiff, horizontalDistance) * Mathf.Rad2Deg;

                    _cinemachineTargetYaw = Mathf.LerpAngle(_cinemachineTargetYaw, targetYaw, Time.deltaTime * 10f);
                    _cinemachineTargetPitch = Mathf.LerpAngle(_cinemachineTargetPitch, targetPitch, Time.deltaTime * 10f);
                }
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
        
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

    }
}
