using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Inventory;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquipmentModel
    {
        private readonly Dictionary<EquipmentSlotId, InventoryEntryId?> _assignments = new();
        private readonly Dictionary<EquipmentSlotGroup, int> _activeIndexes = new();

        public HandMode ActiveHandMode { get; private set; } = HandMode.OneHanded;

        public EquipmentModel()
        {
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                _assignments.Add(slotId, null);
            }

            _activeIndexes.Add(EquipmentSlotGroup.RightHandArmament, 0);
            _activeIndexes.Add(EquipmentSlotGroup.LeftHandArmament, 0);
            _activeIndexes.Add(EquipmentSlotGroup.QuickItem, 0);
        }

        public InventoryEntryId? GetAssignedEntryId(EquipmentSlotId slotId)
        {
            return _assignments[slotId];
        }

        public EquipmentSlotId GetActiveSlot(EquipmentSlotGroup group)
        {
            if (!_activeIndexes.TryGetValue(group, out int activeIndex))
            {
                throw new InvalidOperationException($"Equipment group '{group}' is not cyclable.");
            }

            return EquipmentSlotCatalog.GetSlots(group)[activeIndex];
        }

        internal InventoryEntryId? SetAssignment(
            EquipmentSlotId slotId,
            InventoryEntryId? entryId)
        {
            InventoryEntryId? previous = _assignments[slotId];
            _assignments[slotId] = entryId;
            return previous;
        }

        internal EquipmentSlotId AdvanceActiveSlot(EquipmentSlotGroup group)
        {
            if (!EquipmentSlotCatalog.IsCyclable(group))
            {
                throw new InvalidOperationException($"Equipment group '{group}' is not cyclable.");
            }

            IReadOnlyList<EquipmentSlotId> slots = EquipmentSlotCatalog.GetSlots(group);
            int nextIndex = (_activeIndexes[group] + 1) % slots.Count;
            _activeIndexes[group] = nextIndex;
            return slots[nextIndex];
        }

        internal void SetHandMode(HandMode handMode)
        {
            ActiveHandMode = handMode;
        }
    }
}
