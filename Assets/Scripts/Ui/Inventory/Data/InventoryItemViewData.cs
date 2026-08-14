using System;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;

namespace SoulsLike.Ui.Inventory.Data
{
    public sealed class InventoryItemViewData
    {
        public InventoryEntryId EntryId { get; }
        public ItemDefinition Definition { get; }
        public int Quantity { get; }
        public bool IsEquipped { get; }
        public string EquipmentLabel { get; }
        public bool MeetsRequirements { get; }

        public InventoryItemViewData(
            InventoryEntry entry,
            ItemDefinition definition,
            bool isEquipped,
            string equipmentLabel,
            bool meetsRequirements)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            EntryId = entry.EntryId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Quantity = entry.Quantity;
            IsEquipped = isEquipped;
            EquipmentLabel = equipmentLabel ?? throw new ArgumentNullException(nameof(equipmentLabel));
            MeetsRequirements = meetsRequirements;
        }

        public static InventoryItemViewData Create(
            InventoryEntry entry,
            ItemDefinition definition,
            EquipmentComponent equipment,
            CharacterAttributeStats attributes)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            bool equipped = equipment.TryGetFirstAssignedSlot(entry.EntryId, out EquipmentSlotId slotId);
            string label = equipped ? GetShortSlotLabel(slotId) : string.Empty;
            AttributeRequirements requirements = definition.Stats.Requirements;
            bool meetsRequirements = attributes.Strength >= requirements.Strength
                && attributes.Dexterity >= requirements.Dexterity
                && attributes.Intelligence >= requirements.Intelligence
                && attributes.Faith >= requirements.Faith
                && attributes.Arcane >= requirements.Arcane;
            return new InventoryItemViewData(
                entry,
                definition,
                equipped,
                label,
                meetsRequirements);
        }

        public InventoryPrimaryCategory PrimaryCategory => Definition.ItemType switch
        {
            ItemType.Weapon or ItemType.Shield or ItemType.Ammunition
                => InventoryPrimaryCategory.Weapons,
            ItemType.Armor => InventoryPrimaryCategory.Armor,
            ItemType.Talisman => InventoryPrimaryCategory.Talisman,
            ItemType.Consumable or ItemType.Material or ItemType.Currency
                => InventoryPrimaryCategory.Consumables,
            ItemType.KeyItem => InventoryPrimaryCategory.KeyItems,
            _ => throw new ArgumentOutOfRangeException(
                nameof(Definition.ItemType),
                Definition.ItemType,
                null)
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
