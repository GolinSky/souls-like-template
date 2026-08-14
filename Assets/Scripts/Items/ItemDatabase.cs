using System;
using System.Collections.Generic;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Data/Items/Item Database")]
    public sealed class ItemDatabase : Data
    {
        [SerializeField] private List<ItemDefinition> _items = new();

        private Dictionary<ItemId, ItemDefinition> _itemsById;

        public IReadOnlyList<ItemDefinition> Items => _items;

        public ItemDefinition GetRequired(ItemId itemId)
        {
            EnsureIndex();
            if (!_itemsById.TryGetValue(itemId, out ItemDefinition definition))
            {
                throw new KeyNotFoundException($"Item database does not contain '{itemId}'.");
            }

            return definition;
        }

        public bool TryGet(ItemId itemId, out ItemDefinition definition)
        {
            EnsureIndex();
            return _itemsById.TryGetValue(itemId, out definition);
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

        private Dictionary<ItemId, ItemDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, ItemDefinition>();
            foreach (ItemDefinition definition in _items)
            {
                if (definition == null)
                {
                    throw new InvalidOperationException($"Item database '{name}' contains a null definition.");
                }

                definition.ValidateDefinition();
                if (!result.TryAdd(definition.ItemId, definition))
                {
                    throw new InvalidOperationException(
                        $"Item database '{name}' contains duplicate ItemId '{definition.ItemId}'.");
                }
            }

            return result;
        }
    }
}
