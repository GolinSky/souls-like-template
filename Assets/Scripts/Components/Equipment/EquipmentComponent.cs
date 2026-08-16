using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquipmentComponent : BaseComponent<EquipmentModel>, IInitializable, IDisposable
    {
        private IComponentMediator _componentMediator;
        private InventoryComponent _inventory;
        private ItemDatabase _itemDatabase;

        public event Action<EquipmentSlotChange> SlotChanged;
        public event Action<EquipmentLoadout> LoadoutChanged;

        //todo: avoid other component dependency (InventoryComponent)
        [Inject]
        public void InjectDependencies(InventoryComponent inventory, ItemDatabase itemDatabase)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
        }

        public void Initialize()
        {
            _inventory.Model.Changed += HandleInventoryChanged;
        }

        public void Dispose()
        {
            if (_inventory != null)
            {
                _inventory.Model.Changed -= HandleInventoryChanged;
            }
        }

        public void Assign(EquipmentSlotId slotId, InventoryEntryId entryId)
        {
            InventoryEntry entry = _inventory.GetRequiredEntry(entryId);
            ItemDefinition definition = _itemDatabase.GetRequired(entry.ItemId);
            EquipmentGroup requiredGroup = EquipmentSlotCatalog.GetCompatibilityGroup(slotId);
            if (!definition.CanEquipIn(requiredGroup))
            {
                throw new InvalidOperationException(
                    $"Item '{definition.DisplayName}' cannot be assigned to '{slotId}'.");
            }

            if (TryGetFirstAssignedSlot(entryId, out EquipmentSlotId existingSlot)
                && existingSlot != slotId)
            {
                throw new InvalidOperationException(
                    $"Inventory entry '{entryId}' is already assigned to '{existingSlot}'.");
            }

            InventoryEntryId? previous = Model.SetAssignment(slotId, entryId);
            PublishSlotChange(slotId, previous, entryId);
        }

        public void Unequip(EquipmentSlotId slotId)
        {
            InventoryEntryId? previous = Model.GetAssignedEntryId(slotId);
            if (!previous.HasValue)
            {
                return;
            }

            Model.SetAssignment(slotId, null);
            PublishSlotChange(slotId, previous, null);
        }

        public EquipmentSlotId SwitchActive(EquipmentSlotGroup group)
        {
            EquipmentSlotId previousActiveSlot = Model.GetActiveSlot(group);
            EquipmentSlotId activeSlot = Model.AdvanceActiveSlot(group);
            if (activeSlot != previousActiveSlot)
            {
                PublishLoadoutChanged();
            }

            return activeSlot;
        }

        public bool TrySwitchHandMode(out HandMode handMode)
        {
            EquippedItemContext right = ResolveActiveItem(EquipmentSlotGroup.RightHandArmament);
            if (right == null
                || right.Definition is not WeaponDefinition weaponDefinition
                || !weaponDefinition.CanTwoHand)
            {
                handMode = Model.ActiveHandMode;
                return false;
            }

            handMode = Model.ActiveHandMode switch
            {
                HandMode.OneHanded => HandMode.TwoHanded,
                HandMode.TwoHanded => HandMode.OneHanded,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(Model.ActiveHandMode),
                    Model.ActiveHandMode,
                    null)
            };
            Model.SetHandMode(handMode);
            PublishLoadoutChanged();
            return true;
        }

        public void SetMediator(IComponentMediator componentMediator)
        {
            _componentMediator = componentMediator
                ?? throw new ArgumentNullException(nameof(componentMediator));
        }

        public InventoryEntryId? GetAssignedEntryId(EquipmentSlotId slotId)
        {
            return Model.GetAssignedEntryId(slotId);
        }

        public bool IsEquipped(InventoryEntryId entryId)
        {
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(slotId) == entryId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetFirstAssignedSlot(
            InventoryEntryId entryId,
            out EquipmentSlotId slotId)
        {
            foreach (EquipmentSlotId candidate in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(candidate) == entryId)
                {
                    slotId = candidate;
                    return true;
                }
            }

            slotId = default;
            return false;
        }

        public EquipmentLoadout BuildLoadout()
        {
            return new EquipmentLoadout(
                ResolveActiveItem(EquipmentSlotGroup.RightHandArmament),
                ResolveActiveItem(EquipmentSlotGroup.LeftHandArmament),
                ResolveActiveItem(EquipmentSlotGroup.QuickItem),
                Model.ActiveHandMode);
        }

        public EquippedItemContext ResolveSlot(EquipmentSlotId slotId)
        {
            InventoryEntryId? entryId = Model.GetAssignedEntryId(slotId);
            if (!entryId.HasValue)
            {
                return null;
            }

            InventoryEntry entry = _inventory.GetRequiredEntry(entryId.Value);
            ItemDefinition definition = _itemDatabase.GetRequired(entry.ItemId);
            return new EquippedItemContext(slotId, entry, definition);
        }

        public IReadOnlyList<InventoryEntry> GetCompatibleEntries(EquipmentSlotId slotId)
        {
            EquipmentGroup group = EquipmentSlotCatalog.GetCompatibilityGroup(slotId);
            var result = new List<InventoryEntry>();
            foreach (InventoryEntry entry in _inventory.Entries)
            {
                if (_itemDatabase.GetRequired(entry.ItemId).CanEquipIn(group))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private EquippedItemContext ResolveActiveItem(EquipmentSlotGroup group)
        {
            return ResolveSlot(Model.GetActiveSlot(group));
        }

        private void PublishSlotChange(
            EquipmentSlotId slotId,
            InventoryEntryId? previous,
            InventoryEntryId? current)
        {
            SlotChanged?.Invoke(new EquipmentSlotChange(slotId, previous, current));
            PublishLoadoutChanged();
        }

        private void PublishLoadoutChanged()
        {
            NormalizeHandMode();
            EquipmentLoadout loadout = BuildLoadout();
            LoadoutChanged?.Invoke(loadout);
            if (_componentMediator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentComponent)} requires its component mediator before publishing changes.");
            }

            _componentMediator.NotifyEquipmentLoadoutChanged(loadout);
        }

        private void NormalizeHandMode()
        {
            if (Model.ActiveHandMode != HandMode.TwoHanded)
            {
                return;
            }

            EquippedItemContext right = ResolveActiveItem(
                EquipmentSlotGroup.RightHandArmament);
            if (right?.Definition is WeaponDefinition { CanTwoHand: true })
            {
                return;
            }

            Model.SetHandMode(HandMode.OneHanded);
        }

        private void HandleInventoryChanged(InventoryChange change)
        {
            if (change.Type != InventoryChangeType.Removed)
            {
                return;
            }

            var slotsToClear = new List<EquipmentSlotId>();
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(slotId) == change.Entry.EntryId)
                {
                    slotsToClear.Add(slotId);
                }
            }

            foreach (EquipmentSlotId slotId in slotsToClear)
            {
                Unequip(slotId);
            }
        }
    }
}
