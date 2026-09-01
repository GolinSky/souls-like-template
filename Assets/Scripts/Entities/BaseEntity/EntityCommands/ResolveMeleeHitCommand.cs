using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class ResolveMeleeHitCommand : EntityCommand
    {
        private readonly Entity _target;
        private readonly IEntityLocator _entityLocator;
        private readonly IHealthComponent _health;
        private readonly CombatDefenseComponent _defense;

        public ResolveMeleeHitCommand(
            Entity target,
            IEntityLocator entityLocator,
            IHealthComponent health,
            CombatDefenseComponent defense)
            : base(target)
        {
            _target = target;
            _entityLocator = entityLocator;
            _health = health;
            _defense = defense;
        }

        public MeleeHitResult Execute(in MeleeHitRequest request)
        {
            HitDirection direction = ResolveDirection(request.AttackerPosition);
            DamageResult noDamage = new()
            {
                SourceEntityId = request.AttackerEntityId,
                NewStats = _health.Stats
            };

            if (!_entityLocator.TryGetEntity(request.AttackerEntityId, out IEntity attacker)
                || attacker.Id == _target.Id
                || attacker.EntityType == _target.EntityType
                || !attacker.TryGetComponent(out ApplyDamageCommand attackerApplyDamage)
                || !attackerApplyDamage.Stats.IsAlive
                || !_health.Stats.IsAlive
                || _defense.IsInCriticalState
                || _defense.IsInHitReaction
                || _defense.IsParryStunned)
            {
                return CreateResult(
                    request,
                    MeleeHitResultType.Ignored,
                    direction,
                    noDamage);
            }

            if (_health.IsInvulnerable)
            {
                return Publish(CreateResult(
                    request,
                    MeleeHitResultType.Invulnerable,
                    direction,
                    noDamage));
            }

            if (request.Attack.CanBeParried && _defense.IsParryWindowActive)
            {
                if (attacker.TryGetComponent(out CombatDefenseComponent attackerDefense))
                {
                    attackerDefense.SetParryStunned(true);
                    attackerDefense.SetCriticalOpportunity(true);
                }

                return Publish(CreateResult(
                    request,
                    MeleeHitResultType.Parried,
                    direction,
                    noDamage));
            }

            if (request.Attack.CanBeBlocked
                && _defense.IsBlocking
                && _defense.IsWithinGuardAngle(request.AttackerPosition))
            {
                _health.ConsumeStamina(request.Attack.GuardDamage);
                DamageResult guardResult = new()
                {
                    SourceEntityId = request.AttackerEntityId,
                    NewStats = _health.Stats
                };
                MeleeHitResultType guardType = _health.Stats.CurrentStamina <= 0f
                    ? MeleeHitResultType.GuardBroken
                    : MeleeHitResultType.Blocked;
                if (guardType == MeleeHitResultType.GuardBroken)
                {
                    _defense.BeginGuardBreak();
                }

                return Publish(CreateResult(request, guardType, direction, guardResult));
            }

            DamageResult damage = _health.ApplyDamage(new DamageRequest
            {
                SourceEntityId = request.AttackerEntityId,
                Amount = request.Attack.HealthDamage,
                HitPoint = request.ContactPoint,
                HitZone = request.HitZone
            });
            if (damage.Killed)
            {
                return Publish(CreateResult(
                    request,
                    MeleeHitResultType.Killed,
                    direction,
                    damage));
            }

            if (_defense.ApplyStanceDamage(request.Attack.StanceDamage))
            {
                return Publish(CreateResult(
                    request,
                    MeleeHitResultType.StanceBroken,
                    direction,
                    damage));
            }

            if (_defense.ApplyPoiseDamage(request.Attack.PoiseDamage))
            {
                return Publish(CreateResult(
                    request,
                    MeleeHitResultType.PoiseStaggered,
                    direction,
                    damage));
            }

            MeleeHitResultType hitType = direction == HitDirection.Back
                ? MeleeHitResultType.HitFromBack
                : MeleeHitResultType.Hit;
            return Publish(CreateResult(request, hitType, direction, damage));
        }

        private MeleeHitResult Publish(in MeleeHitResult result)
        {
            _defense.PublishHitResolved(result);
            return result;
        }

        private MeleeHitResult CreateResult(
            in MeleeHitRequest request,
            MeleeHitResultType type,
            HitDirection direction,
            in DamageResult damage) => new(
            request.AttackerEntityId,
            _target.Id,
            request.AttackInstanceId,
            type,
            direction,
            request.Attack.ImpactLevel,
            damage);

        private HitDirection ResolveDirection(Vector3 attackerPosition)
        {
            Vector3 localAttackerPosition = _defense.transform.InverseTransformPoint(
                attackerPosition);
            if (Mathf.Abs(localAttackerPosition.z) >= Mathf.Abs(localAttackerPosition.x))
            {
                return localAttackerPosition.z >= 0f
                    ? HitDirection.Front
                    : HitDirection.Back;
            }

            return localAttackerPosition.x >= 0f
                ? HitDirection.Right
                : HitDirection.Left;
        }
    }
}
