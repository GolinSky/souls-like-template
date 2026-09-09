using System;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class EquipmentLeftHandCompatibilityTests
    {
        private const string ITEM_DATABASE_PATH = "Assets/Settings/Items/ItemDatabase.asset";
        private const string WEAPON_DATABASE_PATH = "Assets/Settings/Items/WeaponDatabase.asset";
        private const string SHIELD_DATABASE_PATH = "Assets/Settings/Items/ShieldDatabase.asset";
        private const string CONSUMABLE_DATABASE_PATH = "Assets/Settings/Items/ConsumableDatabase.asset";

        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Assign_LeftHandWeapon_RejectsBeforeProfileSelectionAndExcludesWeaponFromCompatibleEntries()
        {
            EquipmentComponent equipment = CreateEquipmentComponent(
                out InventoryComponent inventory);
            InventoryEntry weapon = inventory.Add(ItemId.GreatSword, 1).Single();
            InventoryEntry shield = inventory.Add(ItemId.WoodenShield, 1).Single();

            Assert.That(
                () => equipment.Assign(EquipmentSlotId.LeftHand1, weapon.EntryId),
                Throws.TypeOf<InvalidOperationException>());

            string[] compatibleEntryIds = equipment
                .GetCompatibleEntries(EquipmentSlotId.LeftHand1)
                .Select(entry => entry.EntryId.Value)
                .ToArray();
            Assert.That(compatibleEntryIds, Does.Not.Contain(weapon.EntryId.Value));
            Assert.That(compatibleEntryIds, Does.Contain(shield.EntryId.Value));
        }

        [Test]
        public void IsCompatible_RightHandWeaponAndLeftHandShield_AreSupported()
        {
            ItemCatalog catalog = CreateItemCatalog();

            Assert.That(
                EquipmentSlotCatalog.IsCompatible(
                    catalog.GetItem(ItemId.GreatSword),
                    EquipmentSlotId.RightHand1),
                Is.True);
            Assert.That(
                EquipmentSlotCatalog.IsCompatible(
                    catalog.GetItem(ItemId.WoodenShield),
                    EquipmentSlotId.LeftHand1),
                Is.True);
        }

        private EquipmentComponent CreateEquipmentComponent(out InventoryComponent inventory)
        {
            ItemCatalog catalog = CreateItemCatalog();
            _gameObject = new GameObject("Equipment Left-Hand Compatibility Test");
            inventory = _gameObject.AddComponent<InventoryComponent>();
            inventory.Model = new InventoryModel();
            inventory.InjectDependencies(catalog, null);

            var equipment = _gameObject.AddComponent<EquipmentComponent>();
            equipment.Model = new EquipmentModel();
            equipment.InjectDependencies(inventory, catalog, null, null, null);
            return equipment;
        }

        private static ItemCatalog CreateItemCatalog()
        {
            return new ItemCatalog(
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(ITEM_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<WeaponDatabase>(WEAPON_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<ShieldDatabase>(SHIELD_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<ConsumableDatabase>(CONSUMABLE_DATABASE_PATH));
        }
    }
}
