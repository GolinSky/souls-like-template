using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public interface IMovementComponent
    {
        void SetPosition(Vector3 position);
        void Move(Vector2 direction, float cameraYaw, bool sprint, bool crouchActionHeld);
        bool TryStartRoll(Vector2 moveInput, float cameraYaw, bool rollRequested, bool canInterruptAnimation);
        bool TryStartJump(bool jumpRequested, bool sprinting);
        void SetMovementBlocked(bool blocked);
        void ApplyAnimationMovement(Vector3 deltaPosition, Quaternion deltaRotation);
        void SetLockOnTarget(bool isLockedOn, Transform lockOnTarget);
        void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier);
        void RemoveSpeedMultiplier(SpeedMultiplierKey key);
    }
}
