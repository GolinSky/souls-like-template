using System;
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
            if (!_itemsById.TryGetValue(itemId, out WeaponDefinition definition))
            {
                throw new KeyNotFoundException($"Weapon database does not contain '{itemId}'.");
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

        private Dictionary<ItemId, WeaponDefinition> BuildIndex()
        {
            var result = new Dictionary<ItemId, WeaponDefinition>();
            foreach (WeaponDefinition definition in items)
            {
                if (definition == null)
                {
                    throw new InvalidOperationException($"Weapon database '{name}' contains a null definition.");
                }

                definition.ValidateDefinition();
                if (!result.TryAdd(definition.ItemId, definition))
                {
                    throw new InvalidOperationException(
                        $"Weapon database '{name}' contains duplicate ItemId '{definition.ItemId}'.");
                }
            }

            return result;
        }
    }
}
