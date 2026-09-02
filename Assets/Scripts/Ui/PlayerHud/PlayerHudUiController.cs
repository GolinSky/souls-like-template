using System;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Interactions;
using SoulsLike.Services;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.PlayerHud
{
    public class PlayerHudUiController : UiController, IInitializable, ITickable, IPlayerHudPresenter, IGameStateObserver, IDisposable
    {
        private readonly HealthModel _healthModel;
        private readonly EquipmentComponent _equipmentComponent;
        private readonly InventoryComponent _inventoryComponent;
        private readonly ItemCatalog _itemCatalog;
        private readonly InteractionController _interactionController;
        private readonly IGameStateNotifier _gameStateNotifier;
        private PlayerHudUi _playerHudUi;
        private HealthStats _healthStats;

        public PlayerHudUiController(
            IUiService uiService,
            IGameStateNotifier gameStateNotifier,
            HealthModel healthModel,
            ItemCatalog itemCatalog,
            InteractionController interactionController,
            EquipmentComponent equipmentComponent,
            InventoryComponent inventoryComponent) : base(uiService)
        {
            _healthModel = healthModel;
            _itemCatalog = itemCatalog;
            _interactionController = interactionController;
            _gameStateNotifier = gameStateNotifier;
            _equipmentComponent = equipmentComponent;
            _inventoryComponent = inventoryComponent;
        }

        public void Initialize()
        {
            _playerHudUi = CreateUi<PlayerHudUi>();
            _playerHudUi.AssignPresenter(this);
            _healthStats = _healthModel.Stats;
            _healthModel.OnStatsChanged += OnStatsChanged;
            _interactionController.InteractionFailed += OnInteractionFailed;

            if (_equipmentComponent != null)
            {
                _equipmentComponent.LoadoutChanged += OnLoadoutChanged;
                _equipmentComponent.SlotChanged += OnEquipmentSlotChanged;
            }

            if (_inventoryComponent?.Model != null)
            {
                _inventoryComponent.Model.Changed += OnInventoryChanged;
            }

            _gameStateNotifier.RegisterObserver(this);
            OnGameStateChanged(_gameStateNotifier.CurrentGameState);
            UpdateStats();
            UpdateEquipment();
        }
        
        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
            _healthModel.OnStatsChanged -= OnStatsChanged;
            _interactionController.InteractionFailed -= OnInteractionFailed;

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

        public void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.Idle || newState == GameState.Ended)
            {
                _playerHudUi.Show();
            }
            else
            {
                _playerHudUi.Hide();
            }
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
            bool isDimmed = loadout.ActiveQuickItem != null && quickItemQuantity == 0;
            _playerHudUi.UpdateEquipment(
                rightIcon,
                leftIcon,
                quickItemIcon,
                quickItemQuantity,
                loadout.HandMode == HandMode.TwoHanded,
                isDimmed);
        }

        public void ShowAcquisition(string itemName, Sprite icon, int quantity)
        {
            _playerHudUi.ShowAcquisition(itemName, icon, quantity);
        }

        private void OnInteractionFailed(InteractionPrompt prompt)
        {
            _playerHudUi.ShowInteractionFailure(prompt.Text);
        }
    }
}
