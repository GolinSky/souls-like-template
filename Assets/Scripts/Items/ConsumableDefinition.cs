using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ConsumableDefinition", menuName = "Data/Items/Consumable")]
    public sealed class ConsumableDefinition : ItemDefinition
    {
        [SerializeField] private ItemUseType useType;
        [SerializeField, Min(0f)] private float effectAmount;
        [SerializeField, Min(0f)] private float durationSeconds;

        public override ItemType ItemType => ItemType.Consumable;
        public ItemUseType UseType => useType;
        public float EffectAmount => effectAmount;
        public float DurationSeconds => durationSeconds;
    }
}
