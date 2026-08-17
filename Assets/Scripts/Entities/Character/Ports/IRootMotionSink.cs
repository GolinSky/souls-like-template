using UnityEngine;

namespace SoulsLike.Entities.Character.Ports
{
    public interface IRootMotionSink
    {
        void SetAnimationMotionContract(bool movementBlocked, bool useRootMotion);
        void ApplyRootMotion(Vector3 deltaPosition, Quaternion deltaRotation);
    }
}
