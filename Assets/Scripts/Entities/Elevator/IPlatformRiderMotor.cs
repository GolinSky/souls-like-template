using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public interface IPlatformRiderMotor
    {
        bool IsSupportedBy(Collider supportCollider);
        void ApplyPlatformDisplacement(Vector3 displacement);
        void SynchronizeAfterPlatformRide();
    }
}
