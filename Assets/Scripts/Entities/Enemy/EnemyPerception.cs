using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyPerception
    {
        private readonly IEntityLocator _entityLocator;
        private readonly List<IEntity> _players = new();

        public EnemyPerception(IEntityLocator entityLocator)
        {
            _entityLocator = entityLocator;
        }

        public EnemyMemory? Memory { get; private set; }

        public bool TryObserve(
            Vector3 origin,
            Vector3 forward,
            EnemyBehaviourProfile profile,
            float now,
            out TargetingSnapshot observedTarget)
        {
            observedTarget = default;
            float closestDistanceSqr = profile.PerceptionRange * profile.PerceptionRange;
            bool found = false;

            _entityLocator.GetEntities(EntityType.Player, _players);
            foreach (IEntity player in _players)
            {
                if (!player.TryGetComponent(out TargetingCommand targeting))
                {
                    throw new InvalidOperationException(
                        $"Player entity {player.Id} is missing {nameof(TargetingCommand)}.");
                }

                TargetingSnapshot snapshot = targeting.Read();
                Vector3 toTarget = snapshot.LockPoint - (origin + Vector3.up * profile.EyeHeight);
                float distanceSqr = toTarget.sqrMagnitude;
                if (!snapshot.IsAlive
                    || distanceSqr > closestDistanceSqr
                    || Vector3.Angle(forward, toTarget) > profile.FieldOfView * 0.5f
                    || !HasLineOfSight(origin, snapshot, profile))
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                observedTarget = snapshot;
                found = true;
            }

            if (!found)
            {
                return false;
            }

            Remember(observedTarget, now, true);
            return true;
        }

        public bool TryResolveRememberedTarget(out TargetingSnapshot snapshot)
        {
            snapshot = default;
            if (!Memory.HasValue
                || !_entityLocator.TryGetEntity(Memory.Value.EntityId, out IEntity entity))
            {
                return false;
            }

            if (!entity.TryGetComponent(out TargetingCommand targeting))
            {
                throw new InvalidOperationException(
                    $"Target entity {entity.Id} ({entity.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            snapshot = targeting.Read();
            return snapshot.IsAlive;
        }

        public bool HasRecentMemory(float now, EnemyBehaviourProfile profile) =>
            Memory.HasValue
            && now - Memory.Value.LastSeenTime <= profile.TargetMemorySeconds;

        public bool HasLineOfSight(
            Vector3 origin,
            in TargetingSnapshot target,
            EnemyBehaviourProfile profile)
        {
            Vector3 eye = origin + Vector3.up * profile.EyeHeight;
            if (!Physics.Linecast(
                    eye,
                    target.LockPoint,
                    out RaycastHit hit,
                    profile.LineOfSightMask,
                    QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return _entityLocator.TryGetEntity(hit, out IEntity hitEntity)
                && hitEntity.Id == target.EntityId;
        }

        public void RegisterDamageStimulus(long sourceEntityId, float now)
        {
            if (!_entityLocator.TryGetEntity(sourceEntityId, out IEntity source))
            {
                return;
            }

            if (!source.TryGetComponent(out TargetingCommand targeting))
            {
                throw new InvalidOperationException(
                    $"Damage source entity {source.Id} ({source.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            TargetingSnapshot snapshot = targeting.Read();
            if (snapshot.IsAlive)
            {
                Remember(snapshot, now, false);
            }
        }

        public void Clear()
        {
            Memory = null;
        }

        private void Remember(in TargetingSnapshot snapshot, float now, bool hadLineOfSight)
        {
            Memory = new EnemyMemory(
                snapshot.EntityId,
                snapshot.Position,
                snapshot.Forward,
                now,
                hadLineOfSight);
        }
    }
}
