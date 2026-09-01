using System;

namespace SoulsLike.Entities.Combat
{
    [Serializable]
    public struct MeleeAttackData
    {
        public CharacterActionId ActionId;
        public float HealthDamage;
        public float GuardDamage;
        public float PoiseDamage;
        public float StanceDamage;
        public ImpactLevel ImpactLevel;
        public bool CanBeBlocked;
        public bool CanBeParried;
    }
}
