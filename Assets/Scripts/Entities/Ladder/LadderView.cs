using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.Ladder
{
    public sealed class LadderView : MonoBehaviour, IEntityComponent
    {
        private const string CLIMB_PROMPT = "Climb ladder";
        private const string KICK_DOWN_PROMPT = "Kick down ladder";
        private const string LOCKED_PROMPT = "Ladder is locked";

        [SerializeField] private string saveIdentifier;
        [SerializeField] private bool startsLocked;
        [SerializeField] private Transform bottomMount;
        [SerializeField] private Transform topMount;
        [SerializeField] private Transform bottomExit;
        [SerializeField] private Transform topExit;
        [SerializeField] private Transform deployRoot;
        [SerializeField] private Vector3 lockedLocalPosition;
        [SerializeField] private Quaternion lockedLocalRotation = Quaternion.identity;
        [SerializeField] private Vector3 unlockedLocalPosition;
        [SerializeField] private Quaternion unlockedLocalRotation = Quaternion.identity;
        [SerializeField, Min(0f)] private float deployDurationSeconds = 0.75f;
        [SerializeField, Min(0.1f)] private float minOccupantSpacing = 1.5f;

        private readonly List<LadderClimber> _occupants = new();
        private Entity _entity;
        private LadderSystem _system;
        private bool _isUnlocked;
        private bool _isDeploying;
        private LadderClimber _unlockingClimber;

        public string SaveIdentifier => saveIdentifier;
        public bool StartsLocked => startsLocked;
        public bool IsUnlocked => !startsLocked || _isUnlocked;
        public bool IsDeploying => _isDeploying;
        public float Length => Vector3.Distance(bottomMount.position, topMount.position);
        public Entity Entity => _entity;

        public void Construct(Entity entity)
        {
            _entity = entity;
            _entity.RegisterComponent(this);
        }

        public void AssignSystem(LadderSystem system) => _system = system;

        private void OnDestroy()
        {
            _system?.Unregister(this);
        }

        public void DisposeEntity()
        {
            _unlockingClimber?.ForceDetach(LadderDetachReason.Disposed);
            foreach (LadderClimber occupant in new List<LadderClimber>(_occupants))
            {
                occupant.ForceDetach(LadderDetachReason.Disposed);
            }

            _entity?.UnRegisterComponent(this);
            _entity = null;
            _occupants.Clear();
        }

        public void ApplyPersistedUnlock(bool isUnlocked)
        {
            _isUnlocked = isUnlocked;
            ApplyDeployPose(IsUnlocked);
        }

        public bool CanInteract(IEntity actor, LadderEnd end)
        {
            if (!actor.TryGetComponent(out LadderClimber climber) || !climber.CanUseLadder())
            {
                return false;
            }

            if (_unlockingClimber != null || _isDeploying)
            {
                return false;
            }

            return IsUnlocked || end == LadderEnd.Top;
        }

        public InteractionPrompt GetPrompt(IEntity actor, LadderEnd end)
        {
            if (!IsUnlocked && end == LadderEnd.Top)
            {
                return new InteractionPrompt(KICK_DOWN_PROMPT);
            }

            return IsUnlocked ? new InteractionPrompt(CLIMB_PROMPT) : default;
        }

        public InteractionPrompt GetFailurePrompt(IEntity actor, LadderEnd end) =>
            !IsUnlocked && end == LadderEnd.Bottom
                ? new InteractionPrompt(LOCKED_PROMPT)
                : new InteractionPrompt("Cannot use ladder");

        public async UniTask InteractAsync(IEntity actor, LadderEnd end, CancellationToken token)
        {
            if (!actor.TryGetComponent(out LadderClimber climber))
            {
                throw new InvalidOperationException(
                    $"Entity {actor.Id} requires {nameof(LadderClimber)} to use a ladder.");
            }

            if (!IsUnlocked)
            {
                if (end != LadderEnd.Top || _unlockingClimber != null || _isDeploying)
                {
                    return;
                }

                _unlockingClimber = climber;
                try
                {
                    await climber.UnlockAsync(this, token);
                }
                finally
                {
                    if (_unlockingClimber == climber)
                    {
                        _unlockingClimber = null;
                    }
                }
                return;
            }

            await climber.AttachAsync(this, end, token);
        }

        public bool TryAcquire(LadderClimber climber, LadderEnd end)
        {
            if (!IsUnlocked || _isDeploying || _occupants.Contains(climber))
            {
                return false;
            }

            float entryDistance = end == LadderEnd.Bottom ? 0f : Length;
            foreach (LadderClimber occupant in _occupants)
            {
                if (Mathf.Abs(occupant.DistanceOnLadder - entryDistance) < minOccupantSpacing)
                {
                    return false;
                }
            }

            _occupants.Add(climber);
            return true;
        }

        public void Release(LadderClimber climber) => _occupants.Remove(climber);

        public float ClampDistance(LadderClimber climber, float requestedDistance)
        {
            float result = Mathf.Clamp(requestedDistance, 0f, Length);
            foreach (LadderClimber occupant in _occupants)
            {
                if (occupant == climber)
                {
                    continue;
                }

                float occupantDistance = occupant.DistanceOnLadder;
                if (requestedDistance > climber.DistanceOnLadder && requestedDistance > occupantDistance)
                {
                    result = Mathf.Min(result, occupantDistance - minOccupantSpacing);
                }
                else if (requestedDistance < climber.DistanceOnLadder && requestedDistance < occupantDistance)
                {
                    result = Mathf.Max(result, occupantDistance + minOccupantSpacing);
                }
            }

            return Mathf.Clamp(result, 0f, Length);
        }

        public LadderClimber FindNearestOccupant(LadderClimber source, bool above)
        {
            LadderClimber nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (LadderClimber occupant in _occupants)
            {
                if (occupant == source || occupant.DistanceOnLadder == source.DistanceOnLadder)
                {
                    continue;
                }

                bool isAbove = occupant.DistanceOnLadder > source.DistanceOnLadder;
                if (isAbove != above)
                {
                    continue;
                }

                float difference = Mathf.Abs(occupant.DistanceOnLadder - source.DistanceOnLadder);
                if (difference < nearestDistance)
                {
                    nearest = occupant;
                    nearestDistance = difference;
                }
            }

            return nearest;
        }

        public Vector3 SamplePosition(float distance) => Vector3.Lerp(
            bottomMount.position,
            topMount.position,
            Length <= Mathf.Epsilon ? 0f : Mathf.Clamp01(distance / Length));

        public Quaternion SampleRotation() => Quaternion.LookRotation(transform.forward, Vector3.up);

        public Transform GetExit(LadderEnd end) => end == LadderEnd.Top ? topExit : bottomExit;

        public async UniTask DeployAsync(CancellationToken token)
        {
            if (IsUnlocked || _isDeploying)
            {
                return;
            }

            _isDeploying = true;
            try
            {
                float elapsed = 0f;
                while (elapsed < deployDurationSeconds)
                {
                    token.ThrowIfCancellationRequested();
                    elapsed += Time.deltaTime;
                    float progress = deployDurationSeconds <= Mathf.Epsilon
                        ? 1f
                        : Mathf.Clamp01(elapsed / deployDurationSeconds);
                    deployRoot.localPosition = Vector3.Lerp(
                        lockedLocalPosition,
                        unlockedLocalPosition,
                        progress);
                    deployRoot.localRotation = Quaternion.Slerp(
                        lockedLocalRotation,
                        unlockedLocalRotation,
                        progress);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                token.ThrowIfCancellationRequested();
                ApplyDeployPose(true);
                _isUnlocked = true;
            }
            finally
            {
                _isDeploying = false;
                if (!IsUnlocked)
                {
                    ApplyDeployPose(false);
                }
            }
        }

        private void ApplyDeployPose(bool unlocked)
        {
            deployRoot.localPosition = unlocked ? unlockedLocalPosition : lockedLocalPosition;
            deployRoot.localRotation = unlocked ? unlockedLocalRotation : lockedLocalRotation;
        }
    }
}
