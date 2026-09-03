using System;
using System.Collections.Generic;
using SoulsLike;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Navigation;
using VContainer.Unity;

namespace SoulsLike.Ui.PauseNavigation
{
    public sealed class PauseNavigationUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        IPauseNavigationPresenter,
        IPauseMenuRouter
    {
        private readonly ICoreGameOrchestrator _gameOrchestrator;
        private readonly IInputService _inputService;
        private readonly IEquipmentRoute _equipmentRoute;
        private readonly IInventoryRoute _inventoryRoute;
        private readonly ISystemRoute _systemRoute;

        private static readonly ItemType[] _leftHandItemTypes  =
        {
            ItemType.Weapon,
            ItemType.Shield
        };

        private static readonly ItemType[] _rightHandItemTypes = { ItemType.Weapon };
        private static readonly ItemType[] _ammunitionItemTypes = { ItemType.Ammunition };
        private static readonly ItemType[] _armorItemTypes = { ItemType.Armor };
        private static readonly ItemType[] _talismanItemTypes = { ItemType.Talisman };
        private static readonly ItemType[] _consumableItemTypes = { ItemType.Consumable };

        private PauseNavigationUi _view;
        private UiRouteStack _routeStack;

        public PauseNavigationUiController(
            IUiService uiService,
            ICoreGameOrchestrator gameOrchestrator,
            IInputService inputService,
            IEquipmentRoute equipmentRoute,
            IInventoryRoute inventoryRoute,
            ISystemRoute systemRoute)
            : base(uiService)
        {
            _gameOrchestrator = gameOrchestrator;
            _inputService = inputService;
            _equipmentRoute = equipmentRoute;
            _inventoryRoute = inventoryRoute;
            _systemRoute = systemRoute;
        }

        public void Initialize()
        {
            _view = CreateUi<PauseNavigationUi>();
            _view.AssignPresenter(this);
            _view.Hide();
            _routeStack = new UiRouteStack(_view.Show, _view.Hide);

            _equipmentRoute.CloseRequested += HandleEquipmentCloseRequested;
            _equipmentRoute.InventoryRequested += HandleEquipmentInventoryRequested;
            _inventoryRoute.CloseRequested += HandleInventoryCloseRequested;
            _systemRoute.CloseRequested += HandleSystemCloseRequested;
            _systemRoute.ResumeRequested += HandleSystemResumeRequested;
        }

        public void Dispose()
        {
            _equipmentRoute.CloseRequested -= HandleEquipmentCloseRequested;
            _equipmentRoute.InventoryRequested -= HandleEquipmentInventoryRequested;
            _inventoryRoute.CloseRequested -= HandleInventoryCloseRequested;
            _systemRoute.CloseRequested -= HandleSystemCloseRequested;
            _systemRoute.ResumeRequested -= HandleSystemResumeRequested;
        }

        public void Tick()
        {
            if (_gameOrchestrator.CurrentGameState == GameState.Paused
                && _inputService.UiBackAction.WasPressedThisFrame())
            {
                _inputService.ConsumeUiBack();
                HandleUiBack();
                return;
            }

            if (_gameOrchestrator.CurrentGameState == GameState.Paused
                && _inputService.CharacterActions.Pause.WasPressedThisFrame())
            {
                HandleUiBack();
                return;
            }

            if (_gameOrchestrator.CurrentGameState == GameState.Idle
                && _inputService.CharacterActions.Pause.WasPressedThisFrame())
            {
                TogglePauseNavigation();
                return;
            }

            if (_inputService.OpenEquipmentAction.WasPressedThisFrame())
            {
                OpenRouteFromGameplay(_equipmentRoute);
            }
            else if (_inputService.OpenInventoryAction.WasPressedThisFrame())
            {
                OpenRouteFromGameplay(_inventoryRoute);
            }
        }

        public void OpenEquipment()
        {
            OpenRoute(_equipmentRoute);
        }

        public void OpenInventory()
        {
            OpenRoute(_inventoryRoute);
        }

        public void OpenSystem()
        {
            OpenRoute(_systemRoute);
        }

        private void OpenRoute(IPauseNavigationRoute route)
        {
            _routeStack.Open(route);
        }

        private void HandleEquipmentCloseRequested()
        {
            CloseRoute();
        }

        private void HandleInventoryCloseRequested()
        {
            CloseRoute();
        }

        private void HandleSystemCloseRequested()
        {
            CloseRoute();
        }

        private void HandleSystemResumeRequested()
        {
            _routeStack.CloseAll();
            _view.Hide();
            _gameOrchestrator.ResumeGame();
        }

        private void HandleEquipmentInventoryRequested(EquipmentSlotId slotId)
        {
            _routeStack.Open(
                _inventoryRoute,
                () => _inventoryRoute.Open(GetItemTypes(slotId), _equipmentRoute.SelectItem));
        }

        private void CloseRoute()
        {
            _routeStack.CloseTop();
        }

        private void HandleUiBack()
        {
            if (_routeStack.HasOpenRoutes)
            {
                _routeStack.CloseTop();
                return;
            }

            _view.Hide();
            _gameOrchestrator.ResumeGame();
        }

        private void TogglePauseNavigation()
        {
            if (_gameOrchestrator.CurrentGameState == GameState.Idle)
            {
                _view.Show();
                _gameOrchestrator.PauseGame();
            }
        }

        private void OpenRouteFromGameplay(IPauseNavigationRoute route)
        {
            if (_gameOrchestrator.CurrentGameState != GameState.Idle)
            {
                return;
            }

            _gameOrchestrator.PauseGame();
            OpenRoute(route);
        }

        private static IReadOnlyCollection<ItemType> GetItemTypes(EquipmentSlotId slotId)
        {
            if (slotId is >= EquipmentSlotId.RightHand1 and <= EquipmentSlotId.RightHand3)
            {
                return _rightHandItemTypes;
            }

            if (slotId is >= EquipmentSlotId.LeftHand1 and <= EquipmentSlotId.LeftHand3)
            {
                return _leftHandItemTypes;
            }

            if (slotId is >= EquipmentSlotId.Arrow1 and <= EquipmentSlotId.Bolt2)
            {
                return _ammunitionItemTypes;
            }

            if (slotId is >= EquipmentSlotId.Head and <= EquipmentSlotId.Legs)
            {
                return _armorItemTypes;
            }

            if (slotId is >= EquipmentSlotId.Talisman1 and <= EquipmentSlotId.Talisman4)
            {
                return _talismanItemTypes;
            }

            return _consumableItemTypes;
        }
    }
}
