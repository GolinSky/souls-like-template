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
        [SerializeField] private CharacterActionDefinition[] followUps = { };

        public CharacterActionId ActionId => actionId;
        public float DamageMultiplier => damageMultiplier;
        public float StaminaCost => staminaCost;
        public bool UsesRootMotion => usesRootMotion;
        public float WindupTurnSpeed => windupTurnSpeed;
        public float ActiveTurnSpeed => activeTurnSpeed;
        public float RecoveryTurnSpeed => recoveryTurnSpeed;
        public CharacterActionDefinition[] FollowUps => followUps;
    }
}
