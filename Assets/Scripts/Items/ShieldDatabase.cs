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

        private Dictionary<ItemId, ShieldDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, ShieldDefinition>();
            foreach (ShieldDefinition definition in items)
            {
                result.Add(definition.ItemId, definition);
            }

            return result;
        }
    }
}
