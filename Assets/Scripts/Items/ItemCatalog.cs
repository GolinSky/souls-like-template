using System;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Items
{
    public sealed class ItemCatalog : IInitializable
    {
        private readonly ItemDatabase _itemDatabase;
        private readonly WeaponDatabase _weaponDatabase;
        private readonly ShieldDatabase _shieldDatabase;
        private readonly ConsumableDatabase _consumableDatabase;

        public ItemCatalog(
            ItemDatabase itemDatabase,
            WeaponDatabase weaponDatabase,
            ShieldDatabase shieldDatabase,
            ConsumableDatabase consumableDatabase)
        {
            _itemDatabase = itemDatabase;
            _weaponDatabase = weaponDatabase;
            _shieldDatabase = shieldDatabase;
            _consumableDatabase = consumableDatabase;
        }

        public void Initialize()
        {
            ValidateCatalog();
        }

        public ItemDefinition GetItem(ItemId itemId)
        {
            return _itemDatabase.GetRequired(itemId);
        }

        public WeaponDefinition GetWeapon(ItemId itemId)
        {
            RequireItemType(itemId, ItemType.Weapon);
            return _weaponDatabase.GetRequired(itemId);
        }

        public ShieldDefinition GetShield(ItemId itemId)
        {
            RequireItemType(itemId, ItemType.Shield);
            return _shieldDatabase.GetRequired(itemId);
        }

        public ConsumableDefinition GetConsumable(ItemId itemId)
        {
            RequireItemType(itemId, ItemType.Consumable);
            return _consumableDatabase.GetRequired(itemId);
        }

        public ItemStatSnapshot GetStats(ItemId itemId)
        {
            return GetItem(itemId).ItemType switch
            {
                ItemType.Weapon => GetWeapon(itemId).Stats,
                ItemType.Shield => GetShield(itemId).Stats,
                _ => ItemStatSnapshot.Empty
            };
        }

        public Sprite GetSkillIcon(ItemId itemId)
        {
            return GetItem(itemId).ItemType == ItemType.Weapon
                ? GetWeapon(itemId).SkillIcon
                : null;
        }

        public void ValidateCatalog()
        {
            _itemDatabase.ValidateDatabase();
            _weaponDatabase.ValidateDatabase();
            _shieldDatabase.ValidateDatabase();
            _consumableDatabase.ValidateDatabase();

            foreach (ItemDefinition definition in _itemDatabase.Items)
            {
                switch (definition.ItemType)
                {
                    case ItemType.Weapon:
                        _weaponDatabase.GetRequired(definition.ItemId);
                        break;
                    case ItemType.Shield:
                        _shieldDatabase.GetRequired(definition.ItemId);
                        break;
                    case ItemType.Consumable:
                        _consumableDatabase.GetRequired(definition.ItemId);
                        break;
                }
            }

            ValidateForeignKeys(_weaponDatabase.Items, ItemType.Weapon);
            ValidateForeignKeys(_shieldDatabase.Items, ItemType.Shield);
            ValidateForeignKeys(_consumableDatabase.Items, ItemType.Consumable);
        }

        private void RequireItemType(ItemId itemId, ItemType requiredType)
        {
            ItemType itemType = GetItem(itemId).ItemType;
            if (itemType != requiredType)
            {
                throw new InvalidOperationException(
                    $"Item '{itemId}' is '{itemType}', not '{requiredType}'.");
            }
        }

        private void ValidateForeignKeys(
            System.Collections.Generic.IReadOnlyList<WeaponDefinition> definitions,
            ItemType requiredType)
        {
            foreach (WeaponDefinition definition in definitions)
            {
                RequireItemType(definition.ItemId, requiredType);
            }
        }

        private void ValidateForeignKeys(
            System.Collections.Generic.IReadOnlyList<ShieldDefinition> definitions,
            ItemType requiredType)
        {
            foreach (ShieldDefinition definition in definitions)
            {
                RequireItemType(definition.ItemId, requiredType);
            }
        }

        private void ValidateForeignKeys(
            System.Collections.Generic.IReadOnlyList<ConsumableDefinition> definitions,
            ItemType requiredType)
        {
            foreach (ConsumableDefinition definition in definitions)
            {
                RequireItemType(definition.ItemId, requiredType);
            }
        }
    }
}
