#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Items;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Items
{
    public sealed class ItemCatalogTests
    {
        [Test]
        public void CatalogAssets_HaveMatchingDefinitionsAndForeignKeys()
        {
            ItemDatabase items = LoadDatabase<ItemDatabase>();
            WeaponDatabase weapons = LoadDatabase<WeaponDatabase>();
            ShieldDatabase shields = LoadDatabase<ShieldDatabase>();
            ConsumableDatabase consumables = LoadDatabase<ConsumableDatabase>();
            var catalog = new ItemCatalog(items, weapons, shields, consumables);

            foreach (ItemDefinition definition in items.Items)
            {
                switch (definition.ItemType)
                {
                    case ItemType.Weapon:
                        Assert.That(catalog.GetWeapon(definition.ItemId).ItemId, Is.EqualTo(definition.ItemId));
                        break;
                    case ItemType.Shield:
                        Assert.That(catalog.GetShield(definition.ItemId).ItemId, Is.EqualTo(definition.ItemId));
                        break;
                    case ItemType.Consumable:
                        Assert.That(catalog.GetConsumable(definition.ItemId).ItemId, Is.EqualTo(definition.ItemId));
                        break;
                }
            }

            AssertItemTypes(items, weapons.Items.Select(definition => definition.ItemId), ItemType.Weapon);
            AssertItemTypes(items, shields.Items.Select(definition => definition.ItemId), ItemType.Shield);
            AssertItemTypes(items, consumables.Items.Select(definition => definition.ItemId), ItemType.Consumable);
        }

        private static void AssertItemTypes(ItemDatabase items, IEnumerable<ItemId> ids, ItemType expectedType)
        {
            foreach (ItemId id in ids)
            {
                Assert.That(items.GetRequired(id).ItemType, Is.EqualTo(expectedType),
                    $"Item '{id}' must belong to '{expectedType}'.");
            }
        }

        private static T LoadDatabase<T>() where T : ScriptableObject
        {
            string path = $"Assets/Settings/Items/{typeof(T).Name}.asset";
            T database = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(database, Is.Not.Null, path);
            return database;
        }
    }
}
#endif
