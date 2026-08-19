using System;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Ui.Inventory.Data
{
    public sealed class InventoryItemViewData
    {
        public InventoryEntryId EntryId { get; }
        public ItemId ItemId { get; }
        public ItemType ItemType { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string LoreDescription { get; }
        public Sprite Icon { get; }
        public float Weight { get; }
        public bool IsStackable { get; }
        public ItemStatSnapshot Stats { get; }
        public Sprite SkillIcon { get; }
        public int Quantity { get; }
        public bool IsEquipped { get; }
        public string EquipmentLabel { get; }
        public bool MeetsRequirements { get; }

        private InventoryItemViewData(
            InventoryEntry entry,
            ItemDefinition definition,
            ItemStatSnapshot stats,
            Sprite skillIcon,
            bool isEquipped,
            string equipmentLabel,
            bool meetsRequirements)
        {
            EntryId = entry.EntryId;
            ItemId = entry.ItemId;
            ItemType = definition.ItemType;
            DisplayName = definition.DisplayName;
            Description = definition.Description;
            LoreDescription = definition.LoreDescription;
            Icon = definition.Icon;
            Weight = definition.Weight;
            IsStackable = definition.IsStackable;
            Stats = stats;
            SkillIcon = skillIcon;
            Quantity = entry.Quantity;
            IsEquipped = isEquipped;
            EquipmentLabel = equipmentLabel;
            MeetsRequirements = meetsRequirements;
        }

        public static InventoryItemViewData Create(
            InventoryEntry entry,
            ItemCatalog itemCatalog,
            EquipmentComponent equipment,
            CharacterAttributeStats attributes)
        {
            ItemDefinition definition = itemCatalog.GetItem(entry.ItemId);
            ItemStatSnapshot stats = itemCatalog.GetStats(entry.ItemId);
            bool equipped = equipment.TryGetFirstAssignedSlot(
                entry.EntryId,
                out EquipmentSlotId slotId);
            string label = equipped ? GetShortSlotLabel(slotId) : string.Empty;
            AttributeRequirements requirements = stats.Requirements;
            bool meetsRequirements = attributes.Strength >= requirements.Strength
                && attributes.Dexterity >= requirements.Dexterity
                && attributes.Intelligence >= requirements.Intelligence
                && attributes.Faith >= requirements.Faith
                && attributes.Arcane >= requirements.Arcane;
            return new InventoryItemViewData(
                entry,
                definition,
                stats,
                itemCatalog.GetSkillIcon(entry.ItemId),
                equipped,
                label,
                meetsRequirements);
        }

        public InventoryPrimaryCategory PrimaryCategory => ItemType switch
        {
            ItemType.Weapon or ItemType.Shield or ItemType.Ammunition
                => InventoryPrimaryCategory.Weapons,
            ItemType.Armor => InventoryPrimaryCategory.Armor,
            ItemType.Talisman => InventoryPrimaryCategory.Talisman,
            ItemType.Consumable or ItemType.Material or ItemType.Currency
                => InventoryPrimaryCategory.Consumables,
            ItemType.KeyItem => InventoryPrimaryCategory.KeyItems,
            _ => throw new ArgumentOutOfRangeException(nameof(ItemType), ItemType, null)
        };

        private static string GetShortSlotLabel(EquipmentSlotId slotId)
        {
            return slotId switch
            {
                >= EquipmentSlotId.RightHand1 and <= EquipmentSlotId.RightHand3
                    => $"R{(int)slotId - (int)EquipmentSlotId.RightHand1 + 1}",
                >= EquipmentSlotId.LeftHand1 and <= EquipmentSlotId.LeftHand3
                    => $"L{(int)slotId - (int)EquipmentSlotId.LeftHand1 + 1}",
                >= EquipmentSlotId.QuickItem1 and <= EquipmentSlotId.QuickItem10
                    => $"Q{(int)slotId - (int)EquipmentSlotId.QuickItem1 + 1}",
                _ => EquipmentSlotCatalog.GetDisplayName(slotId)
            };
        }
    }
}
