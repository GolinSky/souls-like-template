using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyGroupCoordinator
    {
        private const float ALERT_THROTTLE_SECONDS = 0.25f;

        private readonly int _maxPressureSlots;
        private readonly float _pressureSlotTimeoutSeconds;
        private readonly List<Member> _members = new();
        private readonly Dictionary<EnemyActor, float> _pressureSlots = new();
        private readonly List<EnemyActor> _expiredActors = new();

        public EnemyGroupCoordinator(int maxPressureSlots, float pressureSlotTimeoutSeconds)
        {
            _maxPressureSlots = maxPressureSlots;
            _pressureSlotTimeoutSeconds = pressureSlotTimeoutSeconds;
        }

        public void Register(EnemyActor actor, EnemyController controller)
        {
            if (FindMember(actor) != null)
            {
                return;
            }

            _members.Add(new Member(actor, controller));
            actor.Despawned += OnActorDespawned;
        }

        public void Unregister(EnemyActor actor)
        {
            for (int index = _members.Count - 1; index >= 0; index--)
            {
                if (_members[index].Actor != actor)
                {
                    continue;
                }

                actor.Despawned -= OnActorDespawned;
                _members.RemoveAt(index);
                break;
            }

            _pressureSlots.Remove(actor);
        }

        public void BroadcastAllyAlert(
            EnemyActor source,
            Vector3 position,
            float now)
        {
            Member sourceMember = FindMember(source);
            if (sourceMember == null
                || !source.BehaviourProfile.SharesAllyAlerts
                || now < sourceMember.NextAlertTime)
            {
                return;
            }

            sourceMember.NextAlertTime = now + ALERT_THROTTLE_SECONDS;
            long sourceEntityId = source.Entity.Id;
            foreach (Member member in _members)
            {
                if (member.Actor == source || !member.Actor.BehaviourProfile.SharesAllyAlerts)
                {
                    continue;
                }

                member.Controller.ReceiveAllyAlert(position, sourceEntityId, now);
            }
        }

        public bool TryAcquirePressureSlot(EnemyActor actor, float now)
        {
            ExpirePressureSlots(now);
            if (_pressureSlots.ContainsKey(actor))
            {
                _pressureSlots[actor] = now + _pressureSlotTimeoutSeconds;
                return true;
            }

            if (_pressureSlots.Count >= _maxPressureSlots)
            {
                return false;
            }

            _pressureSlots.Add(actor, now + _pressureSlotTimeoutSeconds);
            return true;
        }

        public void ReleasePressureSlot(EnemyActor actor)
        {
            _pressureSlots.Remove(actor);
        }

        public void RenewPressureSlot(EnemyActor actor, float now)
        {
            if (_pressureSlots.ContainsKey(actor))
            {
                _pressureSlots[actor] = now + _pressureSlotTimeoutSeconds;
            }
        }

        public void Tick(float now)
        {
            ExpirePressureSlots(now);
        }

        public void ReleaseAllPressureSlots()
        {
            _pressureSlots.Clear();
        }

        public void Clear()
        {
            _pressureSlots.Clear();
            foreach (Member member in _members)
            {
                member.Actor.Despawned -= OnActorDespawned;
            }

            _members.Clear();
        }

        private Member FindMember(EnemyActor actor)
        {
            foreach (Member member in _members)
            {
                if (member.Actor == actor)
                {
                    return member;
                }
            }

            return null;
        }

        private void ExpirePressureSlots(float now)
        {
            _expiredActors.Clear();
            foreach ((EnemyActor actor, float expiresAt) in _pressureSlots)
            {
                if (now >= expiresAt)
                {
                    _expiredActors.Add(actor);
                }
            }

            foreach (EnemyActor actor in _expiredActors)
            {
                _pressureSlots.Remove(actor);
            }
        }

        private void OnActorDespawned(EnemyActor actor)
        {
            Unregister(actor);
        }

        private sealed class Member
        {
            public Member(EnemyActor actor, EnemyController controller)
            {
                Actor = actor;
                Controller = controller;
            }

            public EnemyActor Actor { get; }
            public EnemyController Controller { get; }
            public float NextAlertTime { get; set; }
        }
    }
}
