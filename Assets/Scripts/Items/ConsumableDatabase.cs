using System;
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
            if (!_itemsById.TryGetValue(itemId, out ConsumableDefinition definition))
            {
                throw new KeyNotFoundException($"Consumable database does not contain '{itemId}'.");
            }

            return definition;
        }

        public void ValidateDatabase()
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
                if (definition == null)
                {
                    throw new InvalidOperationException(
                        $"Consumable database '{name}' contains a null definition.");
                }

                definition.ValidateDefinition();
                if (!result.TryAdd(definition.ItemId, definition))
                {
                    throw new InvalidOperationException(
                        $"Consumable database '{name}' contains duplicate ItemId '{definition.ItemId}'.");
                }
            }

            return result;
        }
    }
}
