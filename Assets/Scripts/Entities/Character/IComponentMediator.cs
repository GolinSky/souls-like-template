using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;

namespace SoulsLike.Entities.Character
{
    public interface IComponentMediator
    {
        void NotifyLocomotion(float speed, Vector2 blendDirection);
        void NotifyJump();
        void NotifyRoll(Vector2 direction);
        void NotifyBackStep();
        void NotifyCrouch(bool isCrouching);
        void NotifyGrounded(bool isGrounded);
        void NotifyWeaponFire();
        void NotifyHealthStatsChanged(HealthStats stats);
        void NotifyDamageApplied(DamageResult result);
        void NotifyDeath();
        void NotifyZoom(bool isZoomed);
        void NotifyAimTarget(Vector3 targetPosition);
        void NotifyTurn(float turnAmount);
        void NotifyAnimationMovement(Vector3 deltaPosition, Quaternion deltaRotation);
        void SetAnimationMovementContract(bool movementBlocked, bool useRootMotion);
        void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier);
        void RemoveSpeedMultiplier(SpeedMultiplierKey key);
    }
}
