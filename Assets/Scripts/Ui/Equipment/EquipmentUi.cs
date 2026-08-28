using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Items;
using SoulsLike.Ui.Base;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Equipment
{
    public sealed class EquipmentUi : BaseUi
    {
        private const int PICKER_COLUMN_COUNT = 5;

        [Header("Zone 1: Header")]
        [SerializeField] private TMP_Text screenTitleText;
        [SerializeField] private TMP_Text playerSummaryText;

        [Header("Zone 2: Equipment Grid")]
        [SerializeField] private Transform equipmentGridContainer;
        [SerializeField] private List<EquipmentSlotUI> rightHandSlots = new();
        [SerializeField] private List<EquipmentSlotUI> leftHandSlots = new();
        [SerializeField] private List<EquipmentSlotUI> ammoSlots = new();
        [SerializeField] private List<EquipmentSlotUI> armorSlots = new();
        [SerializeField] private List<EquipmentSlotUI> talismanSlots = new();
        [SerializeField] private List<EquipmentSlotUI> quickItemSlots = new();

        [Header("Zone 3: Item Inspector Card")]
        [SerializeField] private Image inspectorItemIcon;
        [SerializeField] private TMP_Text inspectorItemName;
        [SerializeField] private TMP_Text inspectorItemCategory;
        [SerializeField] private TMP_Text inspectorSkillName;
        [SerializeField] private TMP_Text inspectorSkillFpCost;
        [SerializeField] private TMP_Text inspectorAttackSummary;
        [SerializeField] private TMP_Text inspectorReqStr;
        [SerializeField] private TMP_Text inspectorReqDex;
        [SerializeField] private TMP_Text inspectorReqInt;
        [SerializeField] private TMP_Text inspectorReqFth;
        [SerializeField] private TMP_Text inspectorReqArc;
        [SerializeField] private TMP_Text inspectorScalingText;
        [SerializeField] private TMP_Text inspectorWeightText;

        [Header("Zone 4: Character Status Panel")]
        [SerializeField] private TMP_Text vitalsHpText;
        [SerializeField] private TMP_Text vitalsFpText;
        [SerializeField] private TMP_Text vitalsStaminaText;
        [SerializeField] private TMP_Text equipLoadText;
        [SerializeField] private TMP_Text weightClassBadgeText;
        [SerializeField] private Image equipLoadFillBar;
        [SerializeField] private TMP_Text attackPowerRightText;
        [SerializeField] private TMP_Text attackPowerLeftText;
        [SerializeField] private TMP_Text defenseNegationText;
        [SerializeField] private TMP_Text resistancesText;

        [Header("Zone 5: Bottom Action Bar")]
        [SerializeField] private TMP_Text actionPromptsText;

        [Header("Inventory Picker Overlay")]
        [SerializeField] private GameObject inventoryPickerOverlay;
        [SerializeField] private Transform inventoryPickerGridContainer;
        [SerializeField] private GameObject comparisonPanel;
        [SerializeField] private InventorySlotUI inventoryPickerSlotPrefab;

        private readonly Dictionary<EquipmentSlotId, EquipmentSlotUI> _slotsById = new();
        private readonly List<InventorySlotUI> _pickerSlots = new();
        private IEquipmentPresenter _presenter;
        private EquipmentSlotUI _selectedSlot;

        public bool IsPickerOpen => inventoryPickerOverlay.activeSelf;

        public void AssignPresenter(IEquipmentPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public override void Show()
        {
            base.Show();
            _slotsById[EquipmentSlotId.RightHand1].Select();
        }

        public void RefreshSlots(IReadOnlyDictionary<EquipmentSlotId, InventoryItemViewData> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (KeyValuePair<EquipmentSlotId, EquipmentSlotUI> pair in _slotsById)
            {
                items.TryGetValue(pair.Key, out InventoryItemViewData item);
                pair.Value.Bind(pair.Key, item);
            }
        }

        public void DisplaySlot(
            EquipmentSlotId slotId,
            InventoryItemViewData item,
            CharacterAttributeStats attributes)
        {
            if (item == null)
            {
                inspectorItemIcon.enabled = false;
                inspectorItemName.text = $"[Empty {EquipmentSlotCatalog.GetDisplayName(slotId)}]";
                inspectorItemCategory.text = "-";
                inspectorSkillName.text = "-";
                inspectorSkillFpCost.text = "-";
                inspectorAttackSummary.text = "-";
                inspectorReqStr.text = "-";
                inspectorReqDex.text = "-";
                inspectorReqInt.text = "-";
                inspectorReqFth.text = "-";
                inspectorReqArc.text = "-";
                inspectorScalingText.text = "-";
                inspectorWeightText.text = "Weight -";
                return;
            }

            ItemStatSnapshot stats = item.Stats;
            inspectorItemIcon.sprite = item.Icon;
            inspectorItemIcon.enabled = item.Icon != null;
            inspectorItemName.text = item.DisplayName;
            inspectorItemCategory.text = item.ItemType.ToString();
            inspectorSkillName.text = string.IsNullOrWhiteSpace(stats.SkillName)
                ? "-"
                : stats.SkillName;
            inspectorSkillFpCost.text = stats.SkillFocusCost > 0
                ? $"FP {stats.SkillFocusCost}"
                : "-";
            inspectorAttackSummary.text = $"Physical {stats.PhysicalAttack}";
            SetRequirement(inspectorReqStr, stats.Requirements.Strength, attributes.Strength);
            SetRequirement(inspectorReqDex, stats.Requirements.Dexterity, attributes.Dexterity);
            SetRequirement(inspectorReqInt, stats.Requirements.Intelligence, attributes.Intelligence);
            SetRequirement(inspectorReqFth, stats.Requirements.Faith, attributes.Faith);
            SetRequirement(inspectorReqArc, stats.Requirements.Arcane, attributes.Arcane);
            inspectorScalingText.text = $"STR {FormatScaling(stats.Scaling.Strength)}  "
                + $"DEX {FormatScaling(stats.Scaling.Dexterity)}";
            inspectorWeightText.text = $"Weight {item.Weight:F1}";
        }

        public void DisplayCharacterStatus(
            Character character,
            float equipWeight,
            float maxEquipWeight,
            int rightAttack,
            int leftAttack)
        {
            vitalsHpText.text = $"HP {character.HealthStats.CurrentHealth:F0} / {character.HealthStats.MaxHealth:F0}";
            vitalsFpText.text = $"FP {character.HealthStats.CurrentFocus:F0} / {character.HealthStats.MaxFocus:F0}";
            vitalsStaminaText.text = $"Stamina {character.HealthStats.DisplayCurrentStamina:F0} / {character.HealthStats.MaxStamina:F0}";
            equipLoadText.text = $"{equipWeight:F1} / {maxEquipWeight:F1}";
            float loadRatio = maxEquipWeight <= 0f ? 0f : equipWeight / maxEquipWeight;
            equipLoadFillBar.fillAmount = Mathf.Clamp01(loadRatio);
            weightClassBadgeText.text = loadRatio switch
            {
                < 0.3f => "Light Load",
                <= 0.7f => "Medium Load",
                <= 1f => "Heavy Load",
                _ => "Overencumbered"
            };
            attackPowerRightText.text = $"R-Armament {rightAttack}";
            attackPowerLeftText.text = $"L-Armament {leftAttack}";
            defenseNegationText.text = "Defense Negation -";
            resistancesText.text = "Resistances -";
            playerSummaryText.text = $"Runes {character.HeldCurrency:N0}";
        }

        public void ShowPicker(IReadOnlyList<InventoryItemViewData> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            ClearPicker();
            inventoryPickerOverlay.SetActive(true);
            comparisonPanel.SetActive(true);
            foreach (InventoryItemViewData candidate in candidates)
            {
                InventorySlotUI slot = Instantiate(inventoryPickerSlotPrefab, inventoryPickerGridContainer);
                slot.Bind(candidate);
                slot.SlotSelected += HandleCandidateFocused;
                slot.SlotSubmitted += HandleCandidateSubmitted;
                _pickerSlots.Add(slot);
            }

            ConfigurePickerNavigation();
            if (_pickerSlots.Count > 0)
            {
                _pickerSlots[0].Select();
            }
        }

        public void HidePicker()
        {
            ClearPicker();
            inventoryPickerOverlay.SetActive(false);
            comparisonPanel.SetActive(false);
            if (_selectedSlot != null)
            {
                _selectedSlot.Select();
            }
        }

        public void UpdateComparison(int currentAttack, int candidateAttack, float weightDelta)
        {
            int attackDelta = candidateAttack - currentAttack;
            inspectorAttackSummary.text = $"Physical {candidateAttack} ({attackDelta:+#;-#;0})";
            inspectorWeightText.text = $"Weight Δ {weightDelta:+0.0;-0.0;0.0}";
        }

        protected override void Awake()
        {
            base.Awake();
            ValidateReferences();
            BuildSlotMap();
            ConfigureSlotNavigation();
            inventoryPickerOverlay.SetActive(false);
            comparisonPanel.SetActive(false);
            screenTitleText.text = "EQUIPMENT";
            actionPromptsText.text = "Select   Back   Remove   Switch Display";
        }

        private void OnDestroy()
        {
            foreach (EquipmentSlotUI slot in _slotsById.Values)
            {
                slot.SlotFocused -= HandleSlotFocused;
                slot.SlotSubmitted -= HandleSlotSubmitted;
            }

            ClearPicker();
        }

        private void BuildSlotMap()
        {
            AddSlots(rightHandSlots, EquipmentSlotId.RightHand1);
            AddSlots(leftHandSlots, EquipmentSlotId.LeftHand1);
            AddSlots(ammoSlots, EquipmentSlotId.Arrow1);
            AddSlots(armorSlots, EquipmentSlotId.Head);
            AddSlots(talismanSlots, EquipmentSlotId.Talisman1);
            AddSlots(quickItemSlots, EquipmentSlotId.QuickItem1);
        }

        private void AddSlots(List<EquipmentSlotUI> slots, EquipmentSlotId firstSlotId)
        {
            for (int index = 0; index < slots.Count; index++)
            {
                EquipmentSlotId slotId = (EquipmentSlotId)((int)firstSlotId + index);
                EquipmentSlotUI slot = slots[index];
                _slotsById.Add(slotId, slot);
                slot.Bind(slotId, null);
                slot.SlotFocused += HandleSlotFocused;
                slot.SlotSubmitted += HandleSlotSubmitted;
            }
        }

        private void ConfigureSlotNavigation()
        {
            var rows = new List<IReadOnlyList<EquipmentSlotUI>>
            {
                rightHandSlots,
                leftHandSlots,
                ammoSlots,
                armorSlots,
                talismanSlots,
                quickItemSlots.GetRange(0, 5),
                quickItemSlots.GetRange(5, 5)
            };

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                IReadOnlyList<EquipmentSlotUI> row = rows[rowIndex];
                for (int column = 0; column < row.Count; column++)
                {
                    EquipmentSlotUI up = rowIndex > 0
                        ? rows[rowIndex - 1][Math.Min(column, rows[rowIndex - 1].Count - 1)]
                        : null;
                    EquipmentSlotUI down = rowIndex + 1 < rows.Count
                        ? rows[rowIndex + 1][Math.Min(column, rows[rowIndex + 1].Count - 1)]
                        : null;
                    EquipmentSlotUI left = column > 0 ? row[column - 1] : null;
                    EquipmentSlotUI right = column + 1 < row.Count ? row[column + 1] : null;
                    row[column].ConfigureNavigation(up, down, left, right);
                }
            }
        }

        private void ConfigurePickerNavigation()
        {
            for (int index = 0; index < _pickerSlots.Count; index++)
            {
                InventorySlotUI up = index >= PICKER_COLUMN_COUNT
                    ? _pickerSlots[index - PICKER_COLUMN_COUNT]
                    : null;
                InventorySlotUI down = index + PICKER_COLUMN_COUNT < _pickerSlots.Count
                    ? _pickerSlots[index + PICKER_COLUMN_COUNT]
                    : null;
                InventorySlotUI left = index % PICKER_COLUMN_COUNT > 0
                    ? _pickerSlots[index - 1]
                    : null;
                InventorySlotUI right = index % PICKER_COLUMN_COUNT < PICKER_COLUMN_COUNT - 1
                    && index + 1 < _pickerSlots.Count
                    ? _pickerSlots[index + 1]
                    : null;
                _pickerSlots[index].ConfigureNavigation(up, down, left, right);
            }
        }

        private void HandleSlotFocused(EquipmentSlotUI slot)
        {
            _selectedSlot = slot;
            RequirePresenter().FocusSlot(slot.SlotId);
        }

        private void HandleSlotSubmitted(EquipmentSlotUI slot)
        {
            _selectedSlot = slot;
            RequirePresenter().SubmitSlot(slot.SlotId);
        }

        private void HandleCandidateFocused(InventorySlotUI slot)
        {
            RequirePresenter().FocusCandidate(slot.CurrentItem.EntryId);
        }

        private void HandleCandidateSubmitted(InventorySlotUI slot)
        {
            RequirePresenter().SubmitCandidate(slot.CurrentItem.EntryId);
        }

        private void ClearPicker()
        {
            foreach (InventorySlotUI slot in _pickerSlots)
            {
                slot.SlotSelected -= HandleCandidateFocused;
                slot.SlotSubmitted -= HandleCandidateSubmitted;
                Destroy(slot.gameObject);
            }

            _pickerSlots.Clear();
        }

        private IEquipmentPresenter RequirePresenter()
        {
            return _presenter ?? throw new InvalidOperationException(
                $"{nameof(EquipmentUi)} requires a presenter before use.");
        }

        private void ValidateReferences()
        {
            if (screenTitleText == null
                || playerSummaryText == null
                || equipmentGridContainer == null
                || inventoryPickerOverlay == null
                || inventoryPickerGridContainer == null
                || comparisonPanel == null
                || inventoryPickerSlotPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentUi)} '{name}' has missing structural references.");
            }

            if (rightHandSlots.Count != 3
                || leftHandSlots.Count != 3
                || ammoSlots.Count != 4
                || armorSlots.Count != 4
                || talismanSlots.Count != 4
                || quickItemSlots.Count != 10)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentUi)} '{name}' has an invalid equipment-slot topology.");
            }
        }

        private static string FormatScaling(SoulsLike.Items.ScalingGrade grade)
        {
            return grade == SoulsLike.Items.ScalingGrade.None ? "-" : grade.ToString();
        }

        private static void SetRequirement(TMP_Text field, int requiredValue, int currentValue)
        {
            field.text = requiredValue > 0 ? requiredValue.ToString() : "-";
            field.color = currentValue < requiredValue
                ? InventoryUi.ColorUnmetRequirement
                : InventoryUi.ColorParchmentPrimary;
        }
    }
}
