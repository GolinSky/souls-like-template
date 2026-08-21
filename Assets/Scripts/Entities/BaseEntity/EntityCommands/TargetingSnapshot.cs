using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public readonly struct TargetingSnapshot
    {
        public long EntityId { get; }
        public EntityType EntityType { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public Vector3 LockPoint { get; }
        public bool IsAlive { get; }

        public TargetingSnapshot(
            long entityId,
            EntityType entityType,
            Vector3 position,
            Vector3 forward,
            Vector3 lockPoint,
            bool isAlive)
        {
            EntityId = entityId;
            EntityType = entityType;
            Position = position;
            Forward = forward;
            LockPoint = lockPoint;
            IsAlive = isAlive;
        }
    }
}
