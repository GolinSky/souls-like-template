using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyPerception
    {
        private const int DAMAGE_PRIORITY = 400;
        private const int SIGHT_PRIORITY = 300;
        private const int SECONDARY_STIMULUS_PRIORITY = 200;

        private readonly IEntityLocator _entityLocator;
        private readonly List<IEntity> _players = new();
        private TargetObservation? _candidateObservation;
        private float _candidateObservedSince;

        public EnemyPerception(IEntityLocator entityLocator)
        {
            _entityLocator = entityLocator;
        }

        public TargetObservation? CurrentObservation { get; private set; }
        public EnemyMemory? Memory { get; private set; }
        public EnemyStimulus? Stimulus { get; private set; }

        public bool TryObserve(
            Vector3 origin,
            Vector3 forward,
            EnemyBehaviourProfile profile,
            float now,
            out TargetObservation observation)
        {
            observation = default;
            CurrentObservation = null;
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
                if (!snapshot.IsAlive
                    || toTarget.sqrMagnitude > closestDistanceSqr
                    || !IsWithinAwareness(forward, toTarget, profile)
                    || !HasLineOfSight(origin, snapshot, profile))
                {
                    continue;
                }

                closestDistanceSqr = toTarget.sqrMagnitude;
                observation = new TargetObservation(
                    snapshot.EntityId,
                    snapshot.Position,
                    snapshot.LockPoint,
                    snapshot.Forward,
                    now);
                found = true;
            }

            if (!found)
            {
                _candidateObservation = null;
                return false;
            }

            CurrentObservation = observation;
            if (!_candidateObservation.HasValue
                || _candidateObservation.Value.EntityId != observation.EntityId)
            {
                _candidateObservation = observation;
                _candidateObservedSince = now;
            }

            if (now - _candidateObservedSince < profile.SightConfirmationSeconds)
            {
                return false;
            }

            var stimulus = new EnemyStimulus(
                EnemyStimulusType.Sight,
                observation.LockPoint,
                observation.EntityId,
                1f,
                SIGHT_PRIORITY,
                now,
                now + profile.SightForgetSeconds);
            RegisterStimulus(stimulus, observation);
            return true;
        }

        public bool TryGetRecentMemory(float now, out EnemyMemory memory)
        {
            memory = default;
            if (!Memory.HasValue || now > Memory.Value.ForgetTime)
            {
                return false;
            }

            memory = Memory.Value;
            return true;
        }

        public bool IsRememberedTargetAlive()
        {
            if (!Memory.HasValue
                || Memory.Value.StimulusType is not (EnemyStimulusType.Sight or EnemyStimulusType.Damage)
                || !Memory.Value.EntityId.HasValue)
            {
                return true;
            }

            return _entityLocator.TryGetEntity(Memory.Value.EntityId.Value, out IEntity entity)
                && entity.TryGetComponent(out TargetingCommand targeting)
                && targeting.IsAlive;
        }

        public bool HasLineOfSight(
            Vector3 origin,
            in TargetObservation target,
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

        public bool RegisterStimulus(in EnemyStimulus stimulus)
        {
            return RegisterStimulus(stimulus, null);
        }

        public bool RegisterAllyAlertStimulus(
            Vector3 position,
            float strength,
            long? sourceEntityId,
            EnemyBehaviourProfile profile,
            float now)
        {
            return RegisterStimulus(new EnemyStimulus(
                EnemyStimulusType.AllyAlert,
                position,
                sourceEntityId,
                strength,
                SECONDARY_STIMULUS_PRIORITY,
                now,
                now + profile.AllyForgetSeconds));
        }

        public bool RegisterDamageStimulus(
            long sourceEntityId,
            EnemyBehaviourProfile profile,
            float now)
        {
            if (!_entityLocator.TryGetEntity(sourceEntityId, out IEntity source)
                || !source.TryGetComponent(out TargetingCommand targeting))
            {
                return false;
            }

            TargetingSnapshot snapshot = targeting.Read();
            if (!snapshot.IsAlive)
            {
                return false;
            }

            var observation = new TargetObservation(
                snapshot.EntityId,
                snapshot.Position,
                snapshot.LockPoint,
                snapshot.Forward,
                now);
            var stimulus = new EnemyStimulus(
                EnemyStimulusType.Damage,
                observation.LockPoint,
                observation.EntityId,
                1f,
                DAMAGE_PRIORITY,
                now,
                now + profile.DamageForgetSeconds);
            return RegisterStimulus(stimulus, observation);
        }

        public void ClearMemory()
        {
            CurrentObservation = null;
            Memory = null;
            Stimulus = null;
            _candidateObservation = null;
        }

        private bool RegisterStimulus(
            in EnemyStimulus stimulus,
            TargetObservation? observation)
        {
            if (Stimulus.HasValue
                && Stimulus.Value.ForgetTime >= stimulus.Time
                && Stimulus.Value.Priority > stimulus.Priority)
            {
                return false;
            }

            Stimulus = stimulus;
            Memory = observation.HasValue
                ? new EnemyMemory(observation.Value, stimulus)
                : new EnemyMemory(stimulus);
            return true;
        }

        private static bool IsWithinAwareness(
            Vector3 forward,
            Vector3 toTarget,
            EnemyBehaviourProfile profile)
        {
            Vector3 horizontalTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            float horizontalDistance = horizontalTarget.magnitude;
            float horizontalAngle = horizontalDistance > 0f
                ? Vector3.Angle(Vector3.ProjectOnPlane(forward, Vector3.up), horizontalTarget)
                : 0f;
            float verticalAngle = horizontalDistance > 0f
                ? Vector3.Angle(horizontalTarget, toTarget)
                : 90f;
            return (horizontalDistance <= profile.CloseAwarenessRange
                    || horizontalAngle <= profile.FieldOfView * 0.5f)
                && verticalAngle <= profile.VerticalFieldOfView * 0.5f;
        }

        private bool HasLineOfSight(
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
    }
}
