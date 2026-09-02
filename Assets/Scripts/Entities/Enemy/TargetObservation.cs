using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public readonly struct TargetObservation
    {
        public long EntityId { get; }
        public Vector3 Position { get; }
        public Vector3 LockPoint { get; }
        public Vector3 Forward { get; }
        public float ObservedTime { get; }

        public TargetObservation(
            long entityId,
            Vector3 position,
            Vector3 lockPoint,
            Vector3 forward,
            float observedTime)
        {
            EntityId = entityId;
            Position = position;
            LockPoint = lockPoint;
            Forward = forward;
            ObservedTime = observedTime;
        }
    }
}
