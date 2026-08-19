using System;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquippedItemContext
    {
        public EquipmentSlotId SlotId { get; }
        public InventoryEntry Entry { get; }
        public ItemId ItemId => Entry.ItemId;

        public EquippedItemContext(
            EquipmentSlotId slotId,
            InventoryEntry entry)
        {
            SlotId = slotId;
            Entry = entry;
        }
    }

    public readonly struct EquipmentLoadout
    {
        public readonly EquippedItemContext AssignedRight;
        public readonly EquippedItemContext AssignedLeft;
        public readonly EquippedItemContext ActiveQuickItem;
        public readonly EquippedItemContext EffectiveRight;
        public readonly EquippedItemContext EffectiveLeft;
        public readonly HandMode HandMode;

        public EquipmentLoadout(
            EquippedItemContext assignedRight,
            EquippedItemContext assignedLeft,
            EquippedItemContext activeQuickItem,
            HandMode handMode)
        {
            AssignedRight = assignedRight;
            AssignedLeft = assignedLeft;
            ActiveQuickItem = activeQuickItem;
            HandMode = handMode;
            EffectiveRight = assignedRight;
            EffectiveLeft = handMode == HandMode.TwoHanded ? null : assignedLeft;
        }
    }

    public readonly struct EquipmentSlotChange
    {
        public readonly EquipmentSlotId SlotId;
        public readonly InventoryEntryId? PreviousEntryId;
        public readonly InventoryEntryId? CurrentEntryId;

        public EquipmentSlotChange(
            EquipmentSlotId slotId,
            InventoryEntryId? previousEntryId,
            InventoryEntryId? currentEntryId)
        {
            SlotId = slotId;
            PreviousEntryId = previousEntryId;
            CurrentEntryId = currentEntryId;
        }
    }
}
