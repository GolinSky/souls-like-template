using System;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    [Serializable]
    public struct CharacterActionHitDefinition
    {
        [SerializeField, Min(0f)] private float damageMultiplier;
        [SerializeField, Min(0f)] private float guardDamage;
        [SerializeField, Min(0f)] private float poiseDamage;
        [SerializeField, Min(0f)] private float stanceDamage;
        [SerializeField] private ImpactLevel impactLevel;
        [SerializeField] private bool canBeBlocked;
        [SerializeField] private bool canBeParried;

        public CharacterActionHitDefinition(
            float damageMultiplier,
            float guardDamage,
            float poiseDamage,
            float stanceDamage,
            ImpactLevel impactLevel,
            bool canBeBlocked,
            bool canBeParried)
        {
            this.damageMultiplier = damageMultiplier;
            this.guardDamage = guardDamage;
            this.poiseDamage = poiseDamage;
            this.stanceDamage = stanceDamage;
            this.impactLevel = impactLevel;
            this.canBeBlocked = canBeBlocked;
            this.canBeParried = canBeParried;
        }

        public float DamageMultiplier => damageMultiplier;
        public float GuardDamage => guardDamage;
        public float PoiseDamage => poiseDamage;
        public float StanceDamage => stanceDamage;
        public ImpactLevel ImpactLevel => impactLevel;
        public bool CanBeBlocked => canBeBlocked;
        public bool CanBeParried => canBeParried;
    }
}
