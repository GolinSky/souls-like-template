using System.Collections.Generic;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Data/Items/Weapon Database")]
    public sealed class WeaponDatabase : Data
    {
        [SerializeField] private List<WeaponDefinition> items = new();

        private Dictionary<ItemId, WeaponDefinition> _itemsById;

        public IReadOnlyList<WeaponDefinition> Items => items;

        public WeaponDefinition GetRequired(ItemId itemId)
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

        private Dictionary<ItemId, WeaponDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, WeaponDefinition>();
            foreach (WeaponDefinition definition in items)
            {
                result.Add(definition.ItemId, definition);
            }

            return result;
        }
    }
}
