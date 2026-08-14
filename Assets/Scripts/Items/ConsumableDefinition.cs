using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ConsumableDefinition", menuName = "Data/Items/Consumable")]
    public sealed class ConsumableDefinition : ItemDefinition
    {
        [SerializeField] private ItemUseType _useType;
        [SerializeField, Min(0f)] private float _effectAmount;
        [SerializeField, Min(0f)] private float _durationSeconds;

        public override ItemType ItemType => ItemType.Consumable;
        public ItemUseType UseType => _useType;
        public float EffectAmount => _effectAmount;
        public float DurationSeconds => _durationSeconds;
    }
}
