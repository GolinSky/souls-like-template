using SoulsLike.Entities.Character.Components.Health;

namespace SoulsLike.Entities.Combat
{
    public readonly struct MeleeHitResult
    {
        public long AttackerEntityId { get; }
        public long DefenderEntityId { get; }
        public int AttackInstanceId { get; }
        public MeleeHitResultType Type { get; }
        public HitDirection Direction { get; }
        public ImpactLevel ImpactLevel { get; }
        public DamageResult Damage { get; }

        public MeleeHitResult(
            long attackerEntityId,
            long defenderEntityId,
            int attackInstanceId,
            MeleeHitResultType type,
            HitDirection direction,
            ImpactLevel impactLevel,
            in DamageResult damage)
        {
            AttackerEntityId = attackerEntityId;
            DefenderEntityId = defenderEntityId;
            AttackInstanceId = attackInstanceId;
            Type = type;
            Direction = direction;
            ImpactLevel = impactLevel;
            Damage = damage;
        }
    }
}
