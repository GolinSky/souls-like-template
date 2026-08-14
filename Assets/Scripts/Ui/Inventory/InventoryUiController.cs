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
        IInventoryPresenter
    {
        private readonly InventoryComponent _inventory;
        private readonly EquipmentComponent _equipment;
        private readonly ItemDatabase _itemDatabase;
        private readonly Character _character;
        private readonly IInputService _inputService;
        private readonly ICoreGameOrchestrator _gameOrchestrator;

        private InventoryUi _view;
        private InventoryPrimaryCategory _primaryCategory = InventoryPrimaryCategory.Weapons;
        private InventorySubCategory _subCategory = InventorySubCategory.MeleeWeapon;
        private bool _useSubCategoryFilter;

        public InventoryUiController(
            IUiService uiService,
            InventoryComponent inventory,
            EquipmentComponent equipment,
            ItemDatabase itemDatabase,
            Character character,
            IInputService inputService,
            ICoreGameOrchestrator gameOrchestrator)
            : base(uiService)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _gameOrchestrator = gameOrchestrator ?? throw new ArgumentNullException(nameof(gameOrchestrator));
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
            if (_inputService.OpenInventoryAction.WasPressedThisFrame())
            {
                if (_view.IsHidden)
                {
                    TryOpen();
                }
                else
                {
                    CloseInventory();
                }
            }

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
                currentAttack = activeRight.Definition.Stats.PhysicalAttack;
            }

            _view.UpdateStatComparison(currentAttack, item.Definition.Stats.PhysicalAttack);
        }

        public void OnItemSubmitted(InventoryEntryId entryId)
        {
            OnItemFocused(entryId);
        }

        public void CloseInventory()
        {
            if (_view.IsHidden)
            {
                return;
            }

            _view.Hide();
            if (_gameOrchestrator.CurrentGameState == GameState.Paused)
            {
                _gameOrchestrator.ResumeGame();
            }
        }

        public void ToggleLoreView() => _view.ToggleLoreView();
        public void ToggleSimpleView() => _view.ToggleSimpleView();

        private void TryOpen()
        {
            if (_gameOrchestrator.CurrentGameState != GameState.Idle)
            {
                return;
            }

            Refresh();
            _view.Show();
            _gameOrchestrator.PauseGame();
        }

        private void Refresh()
        {
            var items = new List<InventoryItemViewData>();
            foreach (InventoryEntry entry in _inventory.Entries)
            {
                InventoryItemViewData item = BuildViewData(entry);
                if (item.PrimaryCategory != _primaryCategory)
                {
                    continue;
                }

                if (_useSubCategoryFilter && !MatchesSubCategory(item.Definition, _subCategory))
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
            ItemDefinition definition = _itemDatabase.GetRequired(entry.ItemId);
            return InventoryItemViewData.Create(
                entry,
                definition,
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

        private static bool MatchesSubCategory(
            ItemDefinition definition,
            InventorySubCategory category)
        {
            return category switch
            {
                InventorySubCategory.MeleeWeapon => definition.ItemType == ItemType.Weapon,
                InventorySubCategory.RangedWeapon => definition.ItemType == ItemType.Ammunition,
                InventorySubCategory.Shield => definition.ItemType == ItemType.Shield,
                InventorySubCategory.HeadArmor => definition.CanEquipIn(EquipmentGroup.HeadArmor),
                InventorySubCategory.ChestArmor => definition.CanEquipIn(EquipmentGroup.ChestArmor),
                InventorySubCategory.ArmArmor => definition.CanEquipIn(EquipmentGroup.ArmArmor),
                InventorySubCategory.LegArmor => definition.CanEquipIn(EquipmentGroup.LegArmor),
                InventorySubCategory.Talisman => definition.ItemType == ItemType.Talisman,
                InventorySubCategory.CraftingMaterial => definition.ItemType == ItemType.Material,
                InventorySubCategory.ConsumableItem => definition.ItemType == ItemType.Consumable,
                InventorySubCategory.KeyItem => definition.ItemType == ItemType.KeyItem,
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
