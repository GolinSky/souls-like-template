using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "CombatProfile", menuName = "Data/Items/Combat Profile")]
    public sealed class CombatProfile : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float LightAttackMultiplier { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float HeavyAttackMultiplier { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f)] public float StaminaCostMultiplier { get; private set; } = 1f;
    }
}
