using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using SoulsLike.Services;
using SoulsLike.Ui.Status;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.LevelUp
{
    public sealed class LevelUpUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        ILevelUpPresenter,
        ILevelUpRoute
    {
        private const int MAX_ATTRIBUTE_VALUE = 99;
        private const int ATTRIBUTE_COUNT = 8;
        private const float NAV_THRESHOLD = 0.5f;

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

        private LevelUpUi _view;
        private readonly int[] _allocatedPoints = new int[ATTRIBUTE_COUNT];
        private int _selectedAttributeIndex;
        private bool _hasNavigatedThisFrame;

        public event Action CloseRequested;

        public LevelUpUiController(
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
            _view = CreateUi<LevelUpUi>();
            _view.AssignPresenter(this);
            _character.CurrencyChanged += HandleCurrencyChanged;
            _character.AttributesChanged += HandleAttributesChanged;
            Refresh();
            _view.Hide();
        }

        public void Dispose()
        {
            _character.CurrencyChanged -= HandleCurrencyChanged;
            _character.AttributesChanged -= HandleAttributesChanged;
        }

        public void Show()
        {
            Array.Clear(_allocatedPoints, 0, _allocatedPoints.Length);
            _selectedAttributeIndex = 0;
            Refresh();
            _view.Show();
        }

        public void Hide()
        {
            _view.Hide();
        }

        public void Back()
        {
            if (!_view.IsHidden)
            {
                Array.Clear(_allocatedPoints, 0, _allocatedPoints.Length);
                CloseRequested?.Invoke();
            }
        }

        public void SelectAttribute(int index)
        {
            if (_view.IsHidden)
            {
                return;
            }

            _selectedAttributeIndex = Mathf.Clamp(index, 0, ATTRIBUTE_COUNT - 1);
            Refresh();
        }

        public void IncrementAttribute(int index)
        {
            if (_view.IsHidden || index < 0 || index >= ATTRIBUTE_COUNT)
            {
                return;
            }

            int baseValue = GetAttributeBaseValue(index);
            if (baseValue + _allocatedPoints[index] >= MAX_ATTRIBUTE_VALUE)
            {
                return;
            }

            _allocatedPoints[index]++;
            _selectedAttributeIndex = index;
            Refresh();
        }

        public void DecrementAttribute(int index)
        {
            if (_view.IsHidden || index < 0 || index >= ATTRIBUTE_COUNT)
            {
                return;
            }

            if (_allocatedPoints[index] <= 0)
            {
                return;
            }

            _allocatedPoints[index]--;
            _selectedAttributeIndex = index;
            Refresh();
        }

        public void Confirm()
        {
            if (_view.IsHidden)
            {
                return;
            }

            int totalAllocated = GetTotalAllocatedPoints();
            if (totalAllocated <= 0)
            {
                return;
            }

            int currentLevel = LevelUpUiFormatter.CalculateLevel(_character.Attributes);
            int totalCost = LevelUpUiFormatter.CalculateTotalRuneCost(currentLevel, totalAllocated);
            if (_character.HeldCurrency < totalCost)
            {
                return;
            }

            CharacterAttributeStats projected = BuildProjectedAttributes();
            _character.LevelUp(projected, totalCost);
            Array.Clear(_allocatedPoints, 0, _allocatedPoints.Length);
            Refresh();
        }

        public void Tick()
        {
            if (_view.IsHidden)
            {
                return;
            }

            if (_inputService.UiBackAction.WasPressedThisFrame())
            {
                _inputService.ConsumeUiBack();
                Back();
                return;
            }

            HandleNavigationInput();
        }

        private void HandleNavigationInput()
        {
            Vector2 nav = _inputService.UIActions.Navigate.ReadValue<Vector2>();
            if (nav.sqrMagnitude < NAV_THRESHOLD * NAV_THRESHOLD)
            {
                _hasNavigatedThisFrame = false;
                return;
            }

            if (_hasNavigatedThisFrame)
            {
                return;
            }

            _hasNavigatedThisFrame = true;

            if (nav.y > NAV_THRESHOLD)
            {
                int newIndex = _selectedAttributeIndex <= 0 ? ATTRIBUTE_COUNT - 1 : _selectedAttributeIndex - 1;
                SelectAttribute(newIndex);
            }
            else if (nav.y < -NAV_THRESHOLD)
            {
                int newIndex = _selectedAttributeIndex >= ATTRIBUTE_COUNT - 1 ? 0 : _selectedAttributeIndex + 1;
                SelectAttribute(newIndex);
            }
            else if (nav.x > NAV_THRESHOLD)
            {
                IncrementAttribute(_selectedAttributeIndex);
            }
            else if (nav.x < -NAV_THRESHOLD)
            {
                DecrementAttribute(_selectedAttributeIndex);
            }
        }

        private void HandleCurrencyChanged(int newCurrency)
        {
            if (!_view.IsHidden)
            {
                Refresh();
            }
        }

        private void HandleAttributesChanged(CharacterAttributeStats newAttributes)
        {
            if (!_view.IsHidden)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            CharacterAttributeStats currentStats = _character.Attributes;
            CharacterAttributeStats nextStats = BuildProjectedAttributes();

            int currentLevel = LevelUpUiFormatter.CalculateLevel(currentStats);
            int nextLevel = LevelUpUiFormatter.CalculateLevel(nextStats);
            int totalAllocated = GetTotalAllocatedPoints();
            int runesNeeded = LevelUpUiFormatter.CalculateTotalRuneCost(currentLevel, totalAllocated);
            bool isAffordable = _character.HeldCurrency >= runesNeeded;
            int projectedRunes = isAffordable ? _character.HeldCurrency - runesNeeded : _character.HeldCurrency;

            _view.DisplayProfile(
                currentLevel,
                nextLevel,
                _character.HeldCurrency,
                projectedRunes,
                runesNeeded,
                isAffordable);

            int[] currentValues = GetAttributeValues(currentStats);
            int[] nextValues = GetAttributeValues(nextStats);
            for (int i = 0; i < ATTRIBUTE_COUNT; i++)
            {
                _view.DisplayAttributeRow(i, currentValues[i], nextValues[i], i == _selectedAttributeIndex);
            }

            RefreshBaseStats(currentStats, nextStats);
            RefreshArmamentAttacks(currentStats, nextStats);
            RefreshDefenses(currentStats, nextStats, currentLevel, nextLevel);
            RefreshBodyStats(currentStats, nextStats);

            _view.SetConfirmInteractable(totalAllocated > 0 && isAffordable);
            _view.SetPrompt("Choose attribute to level up");
        }

        private void RefreshBaseStats(CharacterAttributeStats currentStats, CharacterAttributeStats nextStats)
        {
            float currentHp = _character.HealthStats.MaxHealth;
            float nextHp = LevelUpUiFormatter.CalculateProjectedHealth(currentHp, currentStats.Vigor, nextStats.Vigor);

            float currentFp = _character.HealthStats.MaxFocus;
            float nextFp = LevelUpUiFormatter.CalculateProjectedFocus(currentFp, currentStats.Mind, nextStats.Mind);

            float currentStamina = _character.HealthStats.MaxStamina;
            float nextStamina = LevelUpUiFormatter.CalculateProjectedStamina(currentStamina, currentStats.Endurance, nextStats.Endurance);

            float currentEquipLoad = LevelUpUiFormatter.CalculateProjectedEquipLoad(currentStats.Endurance);
            float nextEquipLoad = LevelUpUiFormatter.CalculateProjectedEquipLoad(nextStats.Endurance);

            float currentPoise = Mathf.Round(_combatDefense.CurrentPoise);
            float nextPoise = currentPoise;

            float currentDiscovery = 100f + currentStats.Arcane;
            float nextDiscovery = 100f + nextStats.Arcane;

            _view.DisplayBaseStats(
                currentHp, nextHp,
                currentFp, nextFp,
                currentStamina, nextStamina,
                currentEquipLoad, nextEquipLoad,
                currentPoise, nextPoise,
                currentDiscovery, nextDiscovery);
        }

        private void RefreshArmamentAttacks(CharacterAttributeStats currentStats, CharacterAttributeStats nextStats)
        {
            var currentAttacks = BuildArmamentAttacks(currentStats);
            var nextAttacks = BuildArmamentAttacks(nextStats);
            _view.DisplayArmamentAttacks(currentAttacks, nextAttacks);
        }

        private void RefreshDefenses(
            CharacterAttributeStats currentStats,
            CharacterAttributeStats nextStats,
            int currentLevel,
            int nextLevel)
        {
            float levelDiff = nextLevel - currentLevel;
            var currentDefenses = new float[8];
            var nextDefenses = new float[8];

            for (int i = 0; i < 8; i++)
            {
                currentDefenses[i] = 100f + currentLevel * 0.5f;
                nextDefenses[i] = currentDefenses[i] + levelDiff * 0.5f;
            }

            _view.DisplayDefenses(currentDefenses, nextDefenses);
        }

        private void RefreshBodyStats(CharacterAttributeStats currentStats, CharacterAttributeStats nextStats)
        {
            var currentBody = new float[]
            {
                100f + currentStats.Vigor * 2f,
                100f + currentStats.Endurance * 2f,
                100f + currentStats.Mind * 2f,
                100f + currentStats.Arcane * 2f
            };

            var nextBody = new float[]
            {
                100f + nextStats.Vigor * 2f,
                100f + nextStats.Endurance * 2f,
                100f + nextStats.Mind * 2f,
                100f + nextStats.Arcane * 2f
            };

            _view.DisplayBodyStats(currentBody, nextBody);
        }

        private IReadOnlyList<int> BuildArmamentAttacks(CharacterAttributeStats stats)
        {
            var attacks = new int[_armamentSlots.Length];
            int strBonus = stats.Strength - 10;
            int dexBonus = stats.Dexterity - 10;

            for (int index = 0; index < _armamentSlots.Length; index++)
            {
                InventoryEntryId? entryId = _equipment.GetAssignedEntryId(_armamentSlots[index]);
                if (!entryId.HasValue)
                {
                    continue;
                }

                InventoryEntry entry = _inventory.GetRequiredEntry(entryId.Value);
                int baseAttack = StatusUiFormatter.GetTotalAttack(_itemCatalog.GetStats(entry.ItemId));
                attacks[index] = Mathf.Max(0, baseAttack + strBonus / 2 + dexBonus / 2);
            }

            return attacks;
        }

        private CharacterAttributeStats BuildProjectedAttributes()
        {
            CharacterAttributeStats current = _character.Attributes;
            return new CharacterAttributeStats(
                current.Vigor + _allocatedPoints[0],
                current.Mind + _allocatedPoints[1],
                current.Endurance + _allocatedPoints[2],
                current.Strength + _allocatedPoints[3],
                current.Dexterity + _allocatedPoints[4],
                current.Intelligence + _allocatedPoints[5],
                current.Faith + _allocatedPoints[6],
                current.Arcane + _allocatedPoints[7]);
        }

        private int GetAttributeBaseValue(int index)
        {
            CharacterAttributeStats current = _character.Attributes;
            return index switch
            {
                0 => current.Vigor,
                1 => current.Mind,
                2 => current.Endurance,
                3 => current.Strength,
                4 => current.Dexterity,
                5 => current.Intelligence,
                6 => current.Faith,
                7 => current.Arcane,
                _ => 10
            };
        }

        private static int[] GetAttributeValues(CharacterAttributeStats stats)
        {
            return new[]
            {
                stats.Vigor,
                stats.Mind,
                stats.Endurance,
                stats.Strength,
                stats.Dexterity,
                stats.Intelligence,
                stats.Faith,
                stats.Arcane
            };
        }

        private int GetTotalAllocatedPoints()
        {
            int sum = 0;
            for (int i = 0; i < ATTRIBUTE_COUNT; i++)
            {
                sum += _allocatedPoints[i];
            }

            return sum;
        }
    }
}
