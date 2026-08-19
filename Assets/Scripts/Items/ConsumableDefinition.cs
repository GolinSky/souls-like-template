using System;
using UnityEngine;

namespace SoulsLike.Items
{
    [Serializable]
    public sealed class ConsumableDefinition
    {
        [SerializeField] private ItemId itemId;
        [SerializeField] private ItemUseType useType;
        [SerializeField, Min(0f)] private float effectAmount;
        [SerializeField, Min(0f)] private float durationSeconds;

        public ItemId ItemId => itemId;
        public ItemUseType UseType => useType;
        public float EffectAmount => effectAmount;
        public float DurationSeconds => durationSeconds;

        public void ValidateDefinition()
        {
            if (itemId == ItemId.None)
            {
                throw new InvalidOperationException("Consumable definition requires a non-None ItemId.");
            }
        }
    }
}
