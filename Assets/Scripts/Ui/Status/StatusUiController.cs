using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Ui.PauseNavigation;
using VContainer.Unity;

namespace SoulsLike.Ui.Status
{
    public sealed class StatusUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        IStatusPresenter,
        IStatusRoute
    {
        private static readonly EquipmentSlotId[] _armamentSlots =
        {
            EquipmentSlotId.RightHand1,
            EquipmentSlotId.RightHand2,
            EquipmentSlotId.RightHand3,
            EquipmentSlotId.LeftHand1,
            EquipmentSlotId.LeftHand2,
            EquipmentSlotId.LeftHand3
        };

        private readonly Character _character;
        private readonly HealthModel _healthModel;
        private readonly EquipmentComponent _equipment;
        private readonly InventoryComponent _inventory;
        private readonly ItemCatalog _itemCatalog;
        private readonly CombatDefenseComponent _combatDefense;
        private readonly IInputService _inputService;

        private StatusUi _view;
        private bool _isSimpleView;
        private bool _isHelpVisible;
        private float _displayedPoise = float.NaN;
        private float _equipmentWeight;
        private float _equipmentCapacity;

        public event Action CloseRequested;

        public StatusUiController(
            IUiService uiService,
            Character character,
            HealthModel healthModel,
            EquipmentComponent equipment,
            InventoryComponent inventory,
            ItemCatalog itemCatalog,
            CombatDefenseComponent combatDefense,
            IInputService inputService)
            : base(uiService)
        {
            _character = character;
            _healthModel = healthModel;
            _equipment = equipment;
            _inventory = inventory;
            _itemCatalog = itemCatalog;
            _combatDefense = combatDefense;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view = CreateUi<StatusUi>();
            _view.AssignPresenter(this);
            _healthModel.OnStatsChanged += HandleHealthStatsChanged;
            _character.CurrencyChanged += HandleCurrencyChanged;
            _equipment.SlotChanged += HandleEquipmentChanged;
            _equipment.LoadoutChanged += HandleLoadoutChanged;
            _inventory.Model.Changed += HandleInventoryChanged;
            Refresh();
            _view.Hide();
        }

        public void Dispose()
        {
            _healthModel.OnStatsChanged -= HandleHealthStatsChanged;
            _character.CurrencyChanged -= HandleCurrencyChanged;
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

            if (_inputService.ToggleSimpleViewAction.WasPressedThisFrame())
            {
                ToggleSimpleView();
            }

            float currentPoise = RoundDisplayPoise(_combatDefense.CurrentPoise);
            if (!UnityEngine.Mathf.Approximately(_displayedPoise, currentPoise))
            {
                _displayedPoise = currentPoise;
                _view.DisplayPoise(currentPoise);
            }
        }

        public void Show()
        {
            Refresh();
            _view.Show();
        }

        public void Hide()
        {
            _isSimpleView = false;
            _isHelpVisible = false;
            _view.SetSimpleView(false);
            _view.SetHelpVisible(false);
            _view.Hide();
        }

        public void Back()
        {
            if (!_view.IsHidden)
            {
                CloseRequested?.Invoke();
            }
        }

        public void ToggleSimpleView()
        {
            if (_view.IsHidden)
            {
                return;
            }

            _isSimpleView = !_isSimpleView;
            _view.SetSimpleView(_isSimpleView);
        }

        public void ToggleHelp()
        {
            if (_view.IsHidden)
            {
                return;
            }

            _isHelpVisible = !_isHelpVisible;
            _view.SetHelpVisible(_isHelpVisible);
        }

        private void Refresh()
        {
            _equipmentWeight = CalculateEquipmentWeight();
            _equipmentCapacity = 45f + _character.Attributes.Endurance * 1.5f;
            _displayedPoise = RoundDisplayPoise(_combatDefense.CurrentPoise);
            _view.DisplayProfile(_character.HeldCurrency);
            _view.DisplayAttributes(_character.Attributes);
            _view.DisplayBaseStats(
                _character.HealthStats,
                _equipmentWeight,
                _equipmentCapacity,
                _displayedPoise);
            _view.DisplayArmamentAttacks(BuildArmamentAttacks());
            _view.DisplayUnavailableValues();
        }

        private IReadOnlyList<int> BuildArmamentAttacks()
        {
            var attacks = new int[_armamentSlots.Length];
            for (int index = 0; index < _armamentSlots.Length; index++)
            {
                InventoryEntryId? entryId = _equipment.GetAssignedEntryId(_armamentSlots[index]);
                if (!entryId.HasValue)
                {
                    continue;
                }

                InventoryEntry entry = _inventory.GetRequiredEntry(entryId.Value);
                attacks[index] = StatusUiFormatter.GetTotalAttack(
                    _itemCatalog.GetStats(entry.ItemId));
            }

            return attacks;
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

        private void HandleHealthStatsChanged(HealthStats healthStats)
        {
            if (!_view.IsHidden)
            {
                _view.DisplayBaseStats(
                    healthStats,
                    _equipmentWeight,
                    _equipmentCapacity,
                    _displayedPoise);
            }
        }

        private void HandleCurrencyChanged(int heldCurrency)
        {
            if (!_view.IsHidden)
            {
                _view.DisplayProfile(heldCurrency);
            }
        }

        private void HandleEquipmentChanged(EquipmentSlotChange change)
        {
            RefreshIfVisible();
        }

        private void HandleLoadoutChanged(EquipmentLoadout loadout)
        {
            RefreshIfVisible();
        }

        private void HandleInventoryChanged(InventoryChange change)
        {
            RefreshIfVisible();
        }

        private void RefreshIfVisible()
        {
            if (!_view.IsHidden)
            {
                Refresh();
            }
        }

        private static float RoundDisplayPoise(float poise)
        {
            return (float)Math.Round(poise, MidpointRounding.ToEven);
        }
    }
}
