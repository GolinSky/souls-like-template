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
        
        [SerializeField] private float mouseSensitivityY;
        [SerializeField] private float mouseSensitivityX;
        
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;



        public void SetTarget(Transform target)
        {
            // targetCamera.transform.SetParent(target);
            // targetCamera.transform.localPosition = Vector3.zero;
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
            // get camera ray - crosshair like ray (center of screen)
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

        public void UpdateRotation(Vector2 look)
        {
            // if there is an input and camera position is not fixed
            if (look.sqrMagnitude >= THRESHOLD && !lockCameraPosition)
            {
                _cinemachineTargetYaw += look.x * Time.deltaTime * mouseSensitivityX;
                _cinemachineTargetPitch += look.y * Time.deltaTime * mouseSensitivityY;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            // Cinemachine will follow this target
            cinemachineCamera.Follow.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
            
        }
        
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

    }
}