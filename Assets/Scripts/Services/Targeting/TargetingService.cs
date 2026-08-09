using System;
using SoulsLike.Entities.Character;
using UnityEngine;

namespace SoulsLike.Services.Targeting
{
    public interface ITargetingService
    {
        TargetLockNode CurrentTarget { get; }
        bool IsLockedOn { get; }
        event Action<TargetLockNode> TargetChanged;

        bool TryAcquireTarget(Transform origin);
        bool IsCurrentTargetValid(Transform origin);
        void ClearTarget();
    }

    public class TargetingService : ITargetingService
    {
        private const float MAX_LOCK_ON_DISTANCE = 20f;

        public TargetLockNode CurrentTarget { get; private set; }
        public bool IsLockedOn => CurrentTarget != null;
        public event Action<TargetLockNode> TargetChanged;

        public bool TryAcquireTarget(Transform origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            TargetLockNode closestTarget = null;
            float closestDistanceSqr = MAX_LOCK_ON_DISTANCE * MAX_LOCK_ON_DISTANCE;

            TargetLockNode[] candidates = UnityEngine.Object.FindObjectsByType<TargetLockNode>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (TargetLockNode candidate in candidates)
            {
                if (candidate == null || candidate.TargetTransform == null)
                {
                    continue;
                }

                float distanceSqr = (candidate.TargetTransform.position - origin.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestTarget = candidate;
                }
            }

            SetCurrentTarget(closestTarget);
            return CurrentTarget != null;
        }

        public bool IsCurrentTargetValid(Transform origin)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            if (CurrentTarget == null || !CurrentTarget.isActiveAndEnabled || CurrentTarget.TargetTransform == null)
            {
                return false;
            }

            return (CurrentTarget.TargetTransform.position - origin.position).sqrMagnitude
                <= MAX_LOCK_ON_DISTANCE * MAX_LOCK_ON_DISTANCE;
        }

        public void ClearTarget()
        {
            SetCurrentTarget(null);
        }

        private void SetCurrentTarget(TargetLockNode target)
        {
            if (CurrentTarget == target)
            {
                return;
            }

            CurrentTarget = target;
            Action<TargetLockNode> targetChanged = TargetChanged;
            if (targetChanged != null)
            {
                targetChanged(CurrentTarget);
            }
        }
    }
}
