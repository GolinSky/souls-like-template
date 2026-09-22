using SoulsLike.Entities.Elevator;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class PlatformRideCommand : EntityCommand
    {
        private readonly IPlatformRiderMotor _motor;

        public PlatformRideCommand(Entity entity, IPlatformRiderMotor motor)
            : base(entity)
        {
            _motor = motor;
        }

        public bool IsSupportedBy(Collider supportCollider) => _motor.IsSupportedBy(supportCollider);

        public void ApplyPlatformDisplacement(Vector3 displacement) =>
            _motor.ApplyPlatformDisplacement(displacement);

        public void SynchronizeAfterPlatformRide() => _motor.SynchronizeAfterPlatformRide();
    }
}
