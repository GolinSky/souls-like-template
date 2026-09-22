using System.Collections.Generic;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Data/Items/Item Database")]
    public sealed class ItemDatabase : Data
    {
        [SerializeField] private List<ItemDefinition> items = new();

        private Dictionary<ItemId, ItemDefinition> _itemsById;

        public IReadOnlyList<ItemDefinition> Items => items;

        public ItemDefinition GetRequired(ItemId itemId)
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

        private Dictionary<ItemId, ItemDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, ItemDefinition>();
            foreach (ItemDefinition definition in items)
            {
                result.Add(definition.ItemId, definition);
            }

            return result;
        }
    }
}
