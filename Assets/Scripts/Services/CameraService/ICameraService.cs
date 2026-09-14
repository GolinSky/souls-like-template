using SoulsLike.Services.Settings;
using UnityEngine;

namespace SoulsLike.Services.CameraService
{
    public interface ICameraService
    {
        void SetTarget(Transform target);
        void UpdateFollowTarget(bool grounded, float verticalVelocity);
        void UpdateRotation(Vector2 look);
        float GetYaw();
        void SwitchAngle();
        void SetLockOnTarget(long? targetEntityId);
        void ClearLockOnTarget();
        void RecenterCamera();
        Camera GetMainCamera();
        void GenerateImpulse(Vector3 position, Vector3 velocity);
        void ApplySettings(CameraSettingsData settings);
    }
}
