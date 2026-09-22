using System.Collections.Generic;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ConsumableDatabase", menuName = "Data/Items/Consumable Database")]
    public sealed class ConsumableDatabase : Data
    {
        [SerializeField] private List<ConsumableDefinition> items = new();

        private Dictionary<ItemId, ConsumableDefinition> _itemsById;

        public IReadOnlyList<ConsumableDefinition> Items => items;

        public ConsumableDefinition GetRequired(ItemId itemId)
        {
            EnsureIndex();
            return _itemsById[itemId];
        }

        public void RebuildIndex()
        {
            _itemsById = BuildIndex();
        }

        private void EnsureIndex()
        {
            if (_itemsById == null)
            {
                _itemsById = BuildIndex();
            }
        }

        private Dictionary<ItemId, ConsumableDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, ConsumableDefinition>();
            foreach (ConsumableDefinition definition in items)
            {
                result.Add(definition.ItemId, definition);
            }

            return result;
        }
    }
}
