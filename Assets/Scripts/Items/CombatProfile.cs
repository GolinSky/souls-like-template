using UnityEngine;
using SoulsLike.Entities.Combat;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "CombatProfile", menuName = "Data/Items/Combat Profile")]
    public sealed class CombatProfile : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float LightAttackMultiplier { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float HeavyAttackMultiplier { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float LightAttackStaminaCost { get; private set; } = 14f;
        [field: SerializeField] public float LightAttackStaminaStartThreshold { get; private set; } = 0f;
        [field: SerializeField, Min(0f)] public float HeavyAttackStaminaCost { get; private set; } = 30f;
        [field: SerializeField] public float HeavyAttackStaminaStartThreshold { get; private set; } = 0f;
        [field: SerializeField, Min(0f)] public float StaminaCostMultiplier { get; private set; } = 1f;

        [Header("Light Attack Hit")]
        [SerializeField, Min(0f)] private float lightGuardDamage = 20f;
        [SerializeField, Min(0f)] private float lightPoiseDamage = 20f;
        [SerializeField, Min(0f)] private float lightStanceDamage = 15f;
        [SerializeField] private ImpactLevel lightImpactLevel = ImpactLevel.Light;
        [SerializeField] private bool lightCanBeBlocked = true;
        [SerializeField] private bool lightCanBeParried = true;

        [Header("Heavy Attack Hit")]
        [SerializeField, Min(0f)] private float heavyGuardDamage = 40f;
        [SerializeField, Min(0f)] private float heavyPoiseDamage = 45f;
        [SerializeField, Min(0f)] private float heavyStanceDamage = 35f;
        [SerializeField] private ImpactLevel heavyImpactLevel = ImpactLevel.Heavy;
        [SerializeField] private bool heavyCanBeBlocked = true;
        [SerializeField] private bool heavyCanBeParried = true;

        public float LightGuardDamage => lightGuardDamage;
        public float LightPoiseDamage => lightPoiseDamage;
        public float LightStanceDamage => lightStanceDamage;
        public ImpactLevel LightImpactLevel => lightImpactLevel;
        public bool LightCanBeBlocked => lightCanBeBlocked;
        public bool LightCanBeParried => lightCanBeParried;
        public float HeavyGuardDamage => heavyGuardDamage;
        public float HeavyPoiseDamage => heavyPoiseDamage;
        public float HeavyStanceDamage => heavyStanceDamage;
        public ImpactLevel HeavyImpactLevel => heavyImpactLevel;
        public bool HeavyCanBeBlocked => heavyCanBeBlocked;
        public bool HeavyCanBeParried => heavyCanBeParried;
    }
}
