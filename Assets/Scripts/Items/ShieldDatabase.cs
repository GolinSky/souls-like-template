using System;
using System.Collections.Generic;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ShieldDatabase", menuName = "Data/Items/Shield Database")]
    public sealed class ShieldDatabase : Data
    {
        [SerializeField] private List<ShieldDefinition> items = new();

        private Dictionary<ItemId, ShieldDefinition> _itemsById;

        public IReadOnlyList<ShieldDefinition> Items => items;

        public ShieldDefinition GetRequired(ItemId itemId)
        {
            EnsureIndex();
            if (!_itemsById.TryGetValue(itemId, out ShieldDefinition definition))
            {
                throw new KeyNotFoundException($"Shield database does not contain '{itemId}'.");
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

        private Dictionary<ItemId, ShieldDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, ShieldDefinition>();
            foreach (ShieldDefinition definition in items)
            {
                if (definition == null)
                {
                    throw new InvalidOperationException($"Shield database '{name}' contains a null definition.");
                }

                definition.ValidateDefinition();
                if (!result.TryAdd(definition.ItemId, definition))
                {
                    throw new InvalidOperationException(
                        $"Shield database '{name}' contains duplicate ItemId '{definition.ItemId}'.");
                }
            }

            return result;
        }
    }
}
