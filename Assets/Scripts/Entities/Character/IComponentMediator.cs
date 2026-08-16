using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Movement;
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
        void NotifyAirborneMotion(float verticalVelocity, LandingType landingType);
        void NotifyHealthStatsChanged(HealthStats stats);
        void NotifyDamageApplied(DamageResult result);
        void NotifyDeath();
        void NotifyAttack(AttackType attackType, bool isLeftHandAttack);
        void SetChargedAttackSpeed(float speed);
        void NotifyAnimatorStateChanged(AnimatorStateMachineDto state);
        void NotifyTurn(float turnAmount);
        void NotifyAnimationMovement(Vector3 deltaPosition, Quaternion deltaRotation);
        void SetAnimationMovementContract(bool movementBlocked, bool useRootMotion);
        void SetSpeedMultiplier(SpeedMultiplierKey key, float multiplier);
        void RemoveSpeedMultiplier(SpeedMultiplierKey key);
        void NotifyEquipmentLoadoutChanged(EquipmentLoadout loadout);
    }
}
