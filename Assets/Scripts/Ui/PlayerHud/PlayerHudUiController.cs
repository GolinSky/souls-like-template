using System;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Services.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.PlayerHud
{
    public class PlayerHudUiController : UiController, IInitializable, ITickable, IPlayerHudPresenter, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly HealthModel _healthModel;
        private readonly EquipmentComponent _equipmentComponent;
        private readonly InventoryComponent _inventoryComponent;
        private readonly ItemCatalog _itemCatalog;
        private PlayerHudUi _playerHudUi;
        private HealthStats _healthStats;

        public PlayerHudUiController(
            IUiService uiService,
            HealthModel healthModel,
            ItemCatalog itemCatalog,
            EquipmentComponent equipmentComponent = null,
            InventoryComponent inventoryComponent = null,
            ITargetingService targetingService = null) : base(uiService)
        {
            _healthModel = healthModel;
            _itemCatalog = itemCatalog;
            _equipmentComponent = equipmentComponent;
            _inventoryComponent = inventoryComponent;
            _targetingService = targetingService;
        }

        public void Initialize()
        {
            _playerHudUi = CreateUi<PlayerHudUi>();
            _playerHudUi.AssignPresenter(this);
            _healthStats = _healthModel.Stats;
            _healthModel.OnStatsChanged += OnStatsChanged;

            if (_equipmentComponent != null)
            {
                _equipmentComponent.LoadoutChanged += OnLoadoutChanged;
                _equipmentComponent.SlotChanged += OnEquipmentSlotChanged;
            }

            if (_inventoryComponent?.Model != null)
            {
                _inventoryComponent.Model.Changed += OnInventoryChanged;
            }

            _playerHudUi.Show();
            UpdateStats();
            UpdateEquipment();
        }

        public void Tick()
        {
            if (_playerHudUi == null) return;

            UpdateStats();
        }

        private void OnStatsChanged(HealthStats stats)
        {
            _healthStats = stats;
            UpdateStats();
        }

        private void OnLoadoutChanged(EquipmentLoadout loadout)
        {
            UpdateEquipment();
        }

        private void OnEquipmentSlotChanged(EquipmentSlotChange change)
        {
            UpdateEquipment();
        }

        private void OnInventoryChanged(InventoryChange change)
        {
            UpdateEquipment();
        }

        private void UpdateStats()
        {
            if (_playerHudUi == null) return;

            _playerHudUi.UpdateStats(_healthStats);
        }

        private void UpdateEquipment()
        {
            if (_playerHudUi == null || _equipmentComponent == null) return;

            EquipmentLoadout loadout = _equipmentComponent.BuildLoadout();
            Sprite rightIcon = loadout.AssignedRight == null
                ? null
                : _itemCatalog.GetItem(loadout.AssignedRight.ItemId).Icon;
            Sprite leftIcon = loadout.AssignedLeft == null
                ? null
                : _itemCatalog.GetItem(loadout.AssignedLeft.ItemId).Icon;
            Sprite quickItemIcon = loadout.ActiveQuickItem == null
                ? null
                : _itemCatalog.GetItem(loadout.ActiveQuickItem.ItemId).Icon;
            int quickItemQuantity = loadout.ActiveQuickItem?.Entry.Quantity ?? 0;
            _playerHudUi.UpdateEquipment(
                rightIcon,
                leftIcon,
                quickItemIcon,
                quickItemQuantity,
                loadout.HandMode == HandMode.TwoHanded);
        }

        public void Dispose()
        {
            _healthModel.OnStatsChanged -= OnStatsChanged;

            if (_equipmentComponent != null)
            {
                _equipmentComponent.LoadoutChanged -= OnLoadoutChanged;
                _equipmentComponent.SlotChanged -= OnEquipmentSlotChanged;
            }

            if (_inventoryComponent?.Model != null)
            {
                _inventoryComponent.Model.Changed -= OnInventoryChanged;
            }

            if (_playerHudUi != null)
            {
                _playerHudUi.Hide();
            }
        }
    }
}
