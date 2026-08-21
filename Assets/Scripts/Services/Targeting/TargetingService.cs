using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using UnityEngine;

namespace SoulsLike.Services.Targeting
{
    public interface ITargetingService
    {
        long? CurrentTargetEntityId { get; }
        bool IsLockedOn { get; }
        event Action<long?> TargetChanged;

        bool TryAcquireTarget(Vector3 origin);
        bool IsCurrentTargetValid(Vector3 origin);
        bool TryGetCurrentTarget(out TargetingSnapshot snapshot);
        void ClearTarget();
    }

    public class TargetingService : ITargetingService
    {
        private const float MAX_LOCK_ON_DISTANCE = 20f;
        private readonly IEntityLocator _locator;
        private readonly List<IEntity> _candidates = new();

        public long? CurrentTargetEntityId { get; private set; }
        public bool IsLockedOn => CurrentTargetEntityId.HasValue;
        public event Action<long?> TargetChanged;

        public TargetingService(IEntityLocator locator) { _locator = locator; }

        public bool TryAcquireTarget(Vector3 origin)
        {
            long? closestTarget = null;
            float closestDistanceSqr = MAX_LOCK_ON_DISTANCE * MAX_LOCK_ON_DISTANCE;

            _locator.GetEntities(EntityType.Enemy, _candidates);
            foreach (IEntity candidate in _candidates)
            {
                if (!candidate.TryGetComponent<TargetingCommand>(out TargetingCommand command))
                {
                    throw new InvalidOperationException(
                        $"Enemy entity {candidate.Id} is missing {nameof(TargetingCommand)}.");
                }

                TargetingSnapshot snapshot = command.Read();
                if (!snapshot.IsAlive) continue;
                float distanceSqr = (snapshot.LockPoint - origin).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestTarget = snapshot.EntityId;
                }
            }

            SetCurrentTarget(closestTarget);
            return CurrentTargetEntityId.HasValue;
        }

        public bool IsCurrentTargetValid(Vector3 origin)
        {
            if (!TryGetCurrentTarget(out TargetingSnapshot snapshot) || !snapshot.IsAlive) return false;
            return (snapshot.LockPoint - origin).sqrMagnitude <= MAX_LOCK_ON_DISTANCE * MAX_LOCK_ON_DISTANCE;
        }

        public void ClearTarget()
        {
            SetCurrentTarget(null);
        }

        public bool TryGetCurrentTarget(out TargetingSnapshot snapshot)
        {
            snapshot = default;
            if (!CurrentTargetEntityId.HasValue
                || !_locator.TryGetEntity(CurrentTargetEntityId.Value, out IEntity entity))
            {
                SetCurrentTarget(null);
                return false;
            }

            if (!entity.TryGetComponent<TargetingCommand>(out TargetingCommand command))
            {
                throw new InvalidOperationException(
                    $"Target entity {entity.Id} ({entity.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            snapshot = command.Read();
            return true;
        }

        private void SetCurrentTarget(long? target)
        {
            if (CurrentTargetEntityId == target) return;
            CurrentTargetEntityId = target;
            Action<long?> targetChanged = TargetChanged;
            if (targetChanged != null)
            {
                targetChanged(CurrentTargetEntityId);
            }
        }
    }
}
