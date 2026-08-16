using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Ui.Inventory.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.Equipment
{
    public sealed class EquipmentUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        IEquipmentPresenter,
        IEquipmentRoute
    {
        private readonly EquipmentComponent _equipment;
        private readonly InventoryComponent _inventory;
        private readonly ItemDatabase _itemDatabase;
        private readonly Character _character;
        private readonly IInputService _inputService;

        private EquipmentUi _view;
        private EquipmentSlotId? _selectedSlotId;

        public event Action CloseRequested;
        public event Action<EquipmentSlotId> InventoryRequested;

        public EquipmentUiController(
            IUiService uiService,
            EquipmentComponent equipment,
            InventoryComponent inventory,
            ItemDatabase itemDatabase,
            Character character,
            IInputService inputService)
            : base(uiService)
        {
            _equipment = equipment;
            _inventory = inventory;
            _itemDatabase = itemDatabase;
            _character = character;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view = CreateUi<EquipmentUi>();
            _view.AssignPresenter(this);
            _equipment.SlotChanged += HandleEquipmentChanged;
            _equipment.LoadoutChanged += HandleLoadoutChanged;
            _inventory.Model.Changed += HandleInventoryChanged;
            Refresh();
            _view.Hide();
        }

        public void Dispose()
        {
            _equipment.SlotChanged -= HandleEquipmentChanged;
            _equipment.LoadoutChanged -= HandleLoadoutChanged;
            _inventory.Model.Changed -= HandleInventoryChanged;
        }

        public void Tick()
        {
            if (_view.IsHidden)
            {
                return;
            }

            if (_inputService.UIActions.Cancel.WasPressedThisFrame())
            {
                CloseEquipment();
            }
            else if (_inputService.UnequipAction.WasPressedThisFrame())
            {
                UnequipSelectedSlot();
            }
        }

        public void FocusSlot(EquipmentSlotId slotId)
        {
            _selectedSlotId = slotId;
            EquippedItemContext context = _equipment.ResolveSlot(slotId);
            InventoryItemViewData item = context == null ? null : BuildViewData(context.Entry);
            _view.DisplaySlot(slotId, item, _character.Attributes);
        }

        public void SubmitSlot(EquipmentSlotId slotId)
        {
            _selectedSlotId = slotId;
            InventoryRequested?.Invoke(slotId);
        }

        public void Show()
        {
            Refresh();
            _view.Show();
        }

        public void Hide()
        {
            _view.Hide();
        }

        public void FocusCandidate(InventoryEntryId entryId)
        {
            EquipmentSlotId slotId = _selectedSlotId.Value;
            InventoryEntry candidateEntry = _inventory.GetRequiredEntry(entryId);
            InventoryItemViewData candidate = BuildViewData(candidateEntry);
            EquippedItemContext current = _equipment.ResolveSlot(slotId);
            int currentAttack = current == null ? 0 : current.Definition.Stats.PhysicalAttack;
            float currentWeight = current == null ? 0f : current.Definition.Weight;
            _view.DisplaySlot(slotId, candidate, _character.Attributes);
            _view.UpdateComparison(
                currentAttack,
                candidate.Definition.Stats.PhysicalAttack,
                candidate.Definition.Weight - currentWeight);
        }

        public void SubmitCandidate(InventoryEntryId entryId)
        {
            SelectItem(entryId);
        }

        public void SelectItem(InventoryEntryId entryId)
        {
            EquipmentSlotId slotId = _selectedSlotId.Value;
            _equipment.Assign(slotId, entryId);
            Refresh();
            FocusSlot(slotId);
        }

        public void UnequipSelectedSlot()
        {
            if (!_selectedSlotId.HasValue)
            {
                return;
            }

            EquipmentSlotId slotId = _selectedSlotId.Value;
            _equipment.Unequip(slotId);
            Refresh();
            FocusSlot(slotId);
        }

        public void CancelPicker()
        {
            FocusSlot(_selectedSlotId.Value);
        }

        public void CloseEquipment()
        {
            if (_view.IsHidden)
            {
                return;
            }

            CloseRequested?.Invoke();
        }

        private void Refresh()
        {
            var items = new Dictionary<EquipmentSlotId, InventoryItemViewData>();
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                EquippedItemContext context = _equipment.ResolveSlot(slotId);
                if (context != null)
                {
                    items.Add(slotId, BuildViewData(context.Entry));
                }
            }

            _view.RefreshSlots(items);
            EquipmentLoadout loadout = _equipment.BuildLoadout();
            float equipWeight = CalculateEquipmentWeight();
            float maxEquipWeight = 45f + _character.Attributes.Endurance * 1.5f;
            _view.DisplayCharacterStatus(_character, loadout, equipWeight, maxEquipWeight);
        }

        private InventoryItemViewData BuildViewData(InventoryEntry entry)
        {
            return InventoryItemViewData.Create(
                entry,
                _itemDatabase.GetRequired(entry.ItemId),
                _equipment,
                _character.Attributes);
        }

        private float CalculateEquipmentWeight()
        {
            float total = 0f;
            var countedEntries = new HashSet<InventoryEntryId>();
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                InventoryEntryId? entryId = _equipment.GetAssignedEntryId(slotId);
                if (!entryId.HasValue || !countedEntries.Add(entryId.Value))
                {
                    continue;
                }

                InventoryEntry entry = _inventory.GetRequiredEntry(entryId.Value);
                total += _itemDatabase.GetRequired(entry.ItemId).Weight;
            }

            return total;
        }

        private void HandleEquipmentChanged(EquipmentSlotChange change)
        {
            Refresh();
        }

        private void HandleLoadoutChanged(EquipmentLoadout loadout)
        {
            Refresh();
        }

        private void HandleInventoryChanged(InventoryChange change)
        {
            Refresh();
        }
    }
}
