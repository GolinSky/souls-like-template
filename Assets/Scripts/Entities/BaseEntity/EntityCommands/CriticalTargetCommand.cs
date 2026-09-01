using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class CriticalTargetCommand : EntityCommand
    {
        private readonly EnemyActor _actor;
        private readonly EnemyAnimationController _animation;
        private readonly IHealthComponent _health;
        private readonly CombatDefenseComponent _defense;

        public CriticalTargetCommand(
            Entity entity,
            EnemyActor actor,
            EnemyAnimationController animation,
            IHealthComponent health,
            CombatDefenseComponent defense)
            : base(entity)
        {
            _actor = actor;
            _animation = animation;
            _health = health;
            _defense = defense;
        }

        public Transform ActorTransform => _actor.transform;
        public Vector3 Position => _actor.transform.position;
        public Vector3 Forward => _actor.transform.forward;
        public bool IsAlive => _health.Stats.IsAlive;
        public bool IsInvulnerable => _health.IsInvulnerable;
        public bool HasCriticalOpportunity => _defense.HasCriticalOpportunity;
        public bool IsBlocking => _defense.IsBlocking;
        public bool IsParrying => _defense.IsParryWindowActive;
        public bool IsInHitReaction => _defense.IsInHitReaction;
        public bool IsParryStunned => _defense.IsParryStunned;
        public bool IsInCriticalState => _defense.IsInCriticalState;
        public bool IsExecutingAction => _animation.IsActionRunning;
        public bool IsRiposteEligible => HasCriticalOpportunity;
        public bool IsBackstabEligible => !IsBlocking
            && !IsParrying
            && !IsInHitReaction
            && !IsParryStunned
            && !IsInCriticalState
            && !IsExecutingAction;

        public DamageResult PreviewDamage(in DamageRequest request) =>
            _health.CalculateDamage(request, _health.Stats);

        public DamageResult ApplyDamage(in DamageRequest request) =>
            _health.ApplyDamage(request);

        public void BeginCritical(HandMode handMode, bool lethal)
        {
            _animation.Interrupt();
            _defense.SetHitReaction(false);
            _defense.SetParryStunned(false);
            _defense.SetParryWindowActive(false);
            _defense.SetHyperArmor(false);
            _defense.ResetStance();
            _defense.SetCriticalState(true);
            _animation.PlayCriticalVictim(handMode, lethal);
        }

        public void EndCritical()
        {
            _defense.SetCriticalState(false);
            _animation.CompleteCriticalVictim();
        }
    }
}
