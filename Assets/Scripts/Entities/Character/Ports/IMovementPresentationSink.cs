using UnityEngine;
using SoulsLike.Entities.Character.Components.Movement;

namespace SoulsLike.Entities.Character.Ports
{
    public interface IMovementPresentationSink
    {
        void SetLocomotion(float speed, Vector2 blendDirection);
        void SetTurn(float turnAmount);
        void SetGrounded(bool grounded);
        void SetAirborneMotion(float verticalVelocity, LandingType landingType);
        void PlayJump();
        void PlayRoll(Vector2 direction);
        void PlayBackStep();
        void SetCrouch(bool crouching);
    }
}
