using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Ui.Inventory.Data;
using VContainer.Unity;

namespace SoulsLike.Ui.Inventory
{
    public sealed class InventoryUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        IInventoryPresenter,
        IInventoryRoute
    {
        private readonly InventoryComponent _inventory;
        private readonly EquipmentComponent _equipment;
        private readonly ItemCatalog _itemCatalog;
        private readonly Character _character;
        private readonly IInputService _inputService;

        private InventoryUi _view;
        private InventoryPrimaryCategory _primaryCategory = InventoryPrimaryCategory.Weapons;
        private InventorySubCategory _subCategory = InventorySubCategory.MeleeWeapon;
        private bool _useSubCategoryFilter;
        private bool _isSelectionMode;
        private readonly HashSet<ItemType> _routeItemTypes = new();
        private Action<InventoryEntryId> _itemSelected;

        public event Action CloseRequested;

        public InventoryUiController(
            IUiService uiService,
            InventoryComponent inventory,
            EquipmentComponent equipment,
            ItemCatalog itemCatalog,
            Character character,
            IInputService inputService)
            : base(uiService)
        {
            _inventory = inventory;
            _equipment = equipment;
            _itemCatalog = itemCatalog;
            _character = character;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view = CreateUi<InventoryUi>();
            _view.AssignPresenter(this);
            _inventory.Model.Changed += HandleInventoryChanged;
            _equipment.SlotChanged += HandleEquipmentChanged;
            Refresh();
            _view.Hide();
        }

        public void Dispose()
        {
            _inventory.Model.Changed -= HandleInventoryChanged;
            _equipment.SlotChanged -= HandleEquipmentChanged;
        }

        public void Tick()
        {
            if (_view.IsHidden)
            {
                return;
            }

            if (_inputService.UIActions.Cancel.WasPressedThisFrame())
            {
                CloseInventory();
            }
            else if (_inputService.ToggleLoreAction.WasPressedThisFrame())
            {
                ToggleLoreView();
            }
            else if (_inputService.ToggleSimpleViewAction.WasPressedThisFrame())
            {
                ToggleSimpleView();
            }
        }

        public void SelectPrimaryCategory(InventoryPrimaryCategory category)
        {
            _primaryCategory = category;
            _useSubCategoryFilter = false;
            Refresh();
        }

        public void SelectSubCategory(InventorySubCategory subCategory)
        {
            _subCategory = subCategory;
            _useSubCategoryFilter = true;
            Refresh();
        }

        public void OnItemFocused(InventoryEntryId entryId)
        {
            InventoryItemViewData item = BuildViewData(_inventory.GetRequiredEntry(entryId));
            _view.DisplayItemDetails(item, _character.Attributes);

            int currentAttack = 0;
            EquippedItemContext activeRight = _equipment.BuildLoadout().AssignedRight;
            if (activeRight != null)
            {
                currentAttack = _itemCatalog.GetStats(activeRight.ItemId).PhysicalAttack;
            }

            _view.UpdateStatComparison(currentAttack, item.Stats.PhysicalAttack);
        }

        public void OnItemSubmitted(InventoryEntryId entryId)
        {
            OnItemFocused(entryId);
            if (!_isSelectionMode)
            {
                return;
            }

            _itemSelected?.Invoke(entryId);
            CloseRequested?.Invoke();
        }

        public void CloseInventory()
        {
            if (_view.IsHidden)
            {
                return;
            }

            CloseRequested?.Invoke();
        }

        public void ToggleLoreView() => _view.ToggleLoreView();
        public void ToggleSimpleView() => _view.ToggleSimpleView();

        public void Show()
        {
            _isSelectionMode = false;
            _routeItemTypes.Clear();
            _itemSelected = null;
            Refresh();
            _view.Show();
        }

        public void Open(
            IReadOnlyCollection<ItemType> itemTypes,
            Action<InventoryEntryId> itemSelected)
        {
            _isSelectionMode = true;
            _routeItemTypes.Clear();
            foreach (ItemType itemType in itemTypes)
            {
                _routeItemTypes.Add(itemType);
            }

            _itemSelected = itemSelected;
            Refresh();
            _view.Show();
        }

        public void Hide()
        {
            _isSelectionMode = false;
            _routeItemTypes.Clear();
            _itemSelected = null;
            _view.Hide();
        }

        private void Refresh()
        {
            var items = new List<InventoryItemViewData>();
            foreach (InventoryEntry entry in _inventory.Entries)
            {
                InventoryItemViewData item = BuildViewData(entry);
                if (_isSelectionMode && !_routeItemTypes.Contains(item.ItemType))
                {
                    continue;
                }

                if (!_isSelectionMode && item.PrimaryCategory != _primaryCategory)
                {
                    continue;
                }

                if (!_isSelectionMode
                    && _useSubCategoryFilter
                    && !MatchesSubCategory(item, _subCategory))
                {
                    continue;
                }

                items.Add(item);
            }

            _view.PopulateGrid(items);
            float equipWeight = CalculateEquipmentWeight();
            float maxEquipWeight = 45f + _character.Attributes.Endurance * 1.5f;
            _view.DisplayCharacterStats(_character, equipWeight, maxEquipWeight);
            if (items.Count > 0)
            {
                OnItemFocused(items[0].EntryId);
            }
        }

        private InventoryItemViewData BuildViewData(InventoryEntry entry)
        {
            return InventoryItemViewData.Create(
                entry,
                _itemCatalog,
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
                total += _itemCatalog.GetItem(entry.ItemId).Weight;
            }

            return total;
        }

        private bool MatchesSubCategory(
            InventoryItemViewData item,
            InventorySubCategory category)
        {
            return category switch
            {
                InventorySubCategory.MeleeWeapon => item.ItemType == ItemType.Weapon,
                InventorySubCategory.RangedWeapon => item.ItemType == ItemType.Ammunition,
                InventorySubCategory.Shield => item.ItemType == ItemType.Shield,
                InventorySubCategory.HeadArmor =>
                    _itemCatalog.GetItem(item.ItemId).CanEquipIn(EquipmentGroup.HeadArmor),
                InventorySubCategory.ChestArmor =>
                    _itemCatalog.GetItem(item.ItemId).CanEquipIn(EquipmentGroup.ChestArmor),
                InventorySubCategory.ArmArmor =>
                    _itemCatalog.GetItem(item.ItemId).CanEquipIn(EquipmentGroup.ArmArmor),
                InventorySubCategory.LegArmor =>
                    _itemCatalog.GetItem(item.ItemId).CanEquipIn(EquipmentGroup.LegArmor),
                InventorySubCategory.Talisman => item.ItemType == ItemType.Talisman,
                InventorySubCategory.CraftingMaterial => item.ItemType == ItemType.Material,
                InventorySubCategory.ConsumableItem => item.ItemType == ItemType.Consumable,
                InventorySubCategory.KeyItem => item.ItemType == ItemType.KeyItem,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }

        private void HandleInventoryChanged(InventoryChange change)
        {
            Refresh();
        }

        private void HandleEquipmentChanged(EquipmentSlotChange change)
        {
            Refresh();
        }
    }
}
