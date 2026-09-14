using UnityEngine;

namespace SoulsLike.Items
{
    public sealed class ItemCatalog
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

        public ItemDefinition GetItem(ItemId itemId)
        {
            return _itemDatabase.GetRequired(itemId);
        }

        public WeaponDefinition GetWeapon(ItemId itemId)
        {
            return _weaponDatabase.GetRequired(itemId);
        }

        public ShieldDefinition GetShield(ItemId itemId)
        {
            return _shieldDatabase.GetRequired(itemId);
        }

        public ConsumableDefinition GetConsumable(ItemId itemId)
        {
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
    }
}
