using System.Collections.Generic;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Equipment
{
    public class EquipmentUi : BaseUi
    {
        [Header("Zone 1: Header")]
        [SerializeField] private TMP_Text screenTitleText;
        [SerializeField] private TMP_Text playerSummaryText;

        [Header("Zone 2: Equipment Grid")]
        [SerializeField] private Transform equipmentGridContainer;
        [SerializeField] private List<EquipmentSlotUI> rightHandSlots = new List<EquipmentSlotUI>();
        [SerializeField] private List<EquipmentSlotUI> leftHandSlots = new List<EquipmentSlotUI>();
        [SerializeField] private List<EquipmentSlotUI> ammoSlots = new List<EquipmentSlotUI>();
        [SerializeField] private List<EquipmentSlotUI> armorSlots = new List<EquipmentSlotUI>();
        [SerializeField] private List<EquipmentSlotUI> talismanSlots = new List<EquipmentSlotUI>();
        [SerializeField] private List<EquipmentSlotUI> quickItemSlots = new List<EquipmentSlotUI>();

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

        private EquipmentSlotUI currentlySelectedSlot;

        public EquipmentSlotUI CurrentlySelectedSlot => currentlySelectedSlot;

        private void OnEnable()
        {
            RegisterSlots();
        }

        private void RegisterSlots()
        {
            List<EquipmentSlotUI> allSlots = GetAllSlots();
            foreach (var slot in allSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotHighlighted -= HandleSlotHighlighted;
                    slot.OnSlotHighlighted += HandleSlotHighlighted;
                    slot.OnSlotClicked -= HandleSlotClicked;
                    slot.OnSlotClicked += HandleSlotClicked;
                }
            }
        }

        public List<EquipmentSlotUI> GetAllSlots()
        {
            var list = new List<EquipmentSlotUI>();
            list.AddRange(rightHandSlots);
            list.AddRange(leftHandSlots);
            list.AddRange(ammoSlots);
            list.AddRange(armorSlots);
            list.AddRange(talismanSlots);
            list.AddRange(quickItemSlots);
            return list;
        }

        private void HandleSlotHighlighted(EquipmentSlotUI slot)
        {
            currentlySelectedSlot = slot;
            UpdateInspectorForSlot(slot);
        }

        private void HandleSlotClicked(EquipmentSlotUI slot)
        {
            currentlySelectedSlot = slot;
            if (slot != null && !slot.IsLocked)
            {
                ToggleInventoryPicker(true);
            }
        }

        public void ToggleInventoryPicker(bool show)
        {
            if (inventoryPickerOverlay != null)
            {
                inventoryPickerOverlay.SetActive(show);
            }
        }

        private void UpdateInspectorForSlot(EquipmentSlotUI slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                if (inspectorItemName != null) inspectorItemName.text = slot != null ? $"[Empty {slot.SlotCategory}]" : "Empty Slot";
                if (inspectorItemCategory != null) inspectorItemCategory.text = "-";
                if (inspectorItemIcon != null) inspectorItemIcon.enabled = false;
                return;
            }
        }
    }
}
