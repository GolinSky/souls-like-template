using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Movement
{
    public interface IMovementComponent
    {
        void SetPosition(Vector3 position);
        void Move(Vector2 direction, float cameraYaw, bool sprint, bool jumpRequested, bool rollRequested, bool crouchActionToggled);
        void ChangeState(MovementState newState);
    }
}
