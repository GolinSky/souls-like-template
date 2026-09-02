using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    [CreateAssetMenu(fileName = "CharacterActionDefinition", menuName = "Combat/Character Action")]
    public sealed class CharacterActionDefinition : ScriptableObject
    {
        [SerializeField] private CharacterActionId actionId;
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [SerializeField, Min(0f)] private float staminaCost;
        [SerializeField] private bool usesRootMotion = true;
        [SerializeField, Min(0f)] private float windupTurnSpeed = 120f;
        [SerializeField, Min(0f)] private float activeTurnSpeed;
        [SerializeField, Min(0f)] private float recoveryTurnSpeed = 45f;
        [Header("Melee Hit")]
        [SerializeField, Min(0f)] private float guardDamage = 20f;
        [SerializeField, Min(0f)] private float poiseDamage = 20f;
        [SerializeField, Min(0f)] private float stanceDamage = 15f;
        [SerializeField] private ImpactLevel impactLevel = ImpactLevel.Light;
        [SerializeField] private bool canBeBlocked = true;
        [SerializeField] private bool canBeParried = true;
        [SerializeField] private CharacterActionHitDefinition[] hitDefinitions = { };
        [SerializeField] private CharacterActionDefinition[] followUps = { };

        public CharacterActionId ActionId => actionId;
        public float DamageMultiplier => damageMultiplier;
        public float StaminaCost => staminaCost;
        public bool UsesRootMotion => usesRootMotion;
        public float WindupTurnSpeed => windupTurnSpeed;
        public float ActiveTurnSpeed => activeTurnSpeed;
        public float RecoveryTurnSpeed => recoveryTurnSpeed;
        public float GuardDamage => guardDamage;
        public float PoiseDamage => poiseDamage;
        public float StanceDamage => stanceDamage;
        public ImpactLevel ImpactLevel => impactLevel;
        public bool CanBeBlocked => canBeBlocked;
        public bool CanBeParried => canBeParried;
        public CharacterActionHitDefinition[] HitDefinitions => hitDefinitions;
        public CharacterActionDefinition[] FollowUps => followUps;

        public CharacterActionHitDefinition GetHitDefinition(int hitIndex)
        {
            if (hitDefinitions != null && hitDefinitions.Length > 0)
            {
                if (hitIndex >= 0 && hitIndex < hitDefinitions.Length)
                {
                    return hitDefinitions[hitIndex];
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Invalid hitIndex {hitIndex} for action {actionId}. Configured hit definitions count is {hitDefinitions.Length}.", this);
#endif
                return default;
            }

            return new CharacterActionHitDefinition(
                damageMultiplier,
                guardDamage,
                poiseDamage,
                stanceDamage,
                impactLevel,
                canBeBlocked,
                canBeParried);
        }
    }
}
