using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services.CameraService;
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
        private const float MAX_LOCK_ON_ANGLE = 60f;
        private const float DISTANCE_WEIGHT = 15f;

        private readonly IEntityLocator _locator;
        private readonly ICameraService _cameraService;
        private readonly List<IEntity> _candidates = new();

        public long? CurrentTargetEntityId { get; private set; }
        public bool IsLockedOn => CurrentTargetEntityId.HasValue;
        public event Action<long?> TargetChanged;

        public TargetingService(IEntityLocator locator, ICameraService cameraService)
        {
            _locator = locator;
            _cameraService = cameraService;
        }

        public bool TryAcquireTarget(Vector3 origin)
        {
            long? bestTarget = null;
            float bestScore = float.MaxValue;

            Camera camera = _cameraService.GetMainCamera();
            Transform cameraTransform = camera != null ? camera.transform : null;
            Vector3 cameraPosition = cameraTransform != null ? cameraTransform.position : origin;
            Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;

            _locator.GetEntities(EntityType.Enemy, _candidates);
            foreach (IEntity candidate in _candidates)
            {
                if (!candidate.TryGetComponent<TargetingCommand>(out TargetingCommand command))
                {
                    throw new InvalidOperationException(
                        $"Enemy entity {candidate.Id} is missing {nameof(TargetingCommand)}.");
                }

                TargetingSnapshot snapshot = command.Read();
                if (!snapshot.IsAlive || !snapshot.IsVisible) continue;

                float distanceSqr = (snapshot.LockPoint - origin).sqrMagnitude;
                if (distanceSqr > MAX_LOCK_ON_DISTANCE * MAX_LOCK_ON_DISTANCE) continue;

                float distance = Mathf.Sqrt(distanceSqr);
                float angle = 0f;

                if (cameraTransform != null)
                {
                    Vector3 toTarget = snapshot.LockPoint - cameraPosition;
                    if (Vector3.Dot(cameraForward, toTarget) <= 0f)
                    {
                        continue;
                    }

                    angle = Vector3.Angle(cameraForward, toTarget);
                    if (angle > MAX_LOCK_ON_ANGLE)
                    {
                        continue;
                    }
                }

                float score = angle + (distance / MAX_LOCK_ON_DISTANCE) * DISTANCE_WEIGHT;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = snapshot.EntityId;
                }
            }

            SetCurrentTarget(bestTarget);
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
