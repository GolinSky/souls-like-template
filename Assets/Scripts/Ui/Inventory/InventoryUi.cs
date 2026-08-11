using System;
using System.Collections.Generic;
using MPUIKIT;
using SoulsLike.Ui.Base;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public class InventoryUi : BaseUi
    {
        [Header("View State Controller")]
        [SerializeField] private InventoryViewStateController viewStateController;

        [Header("Header Navigation")]
        [SerializeField] private TMP_Text screenTitleText;
        [SerializeField] private Transform primaryCategoryTabContainer;
        [SerializeField] private Transform subCategoryIconContainer;

        [Header("Column 1: Grid Panel")]
        [SerializeField] private Transform gridContentParent;
        [SerializeField] private ScrollRect gridScrollRect;
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("Column 2: Item Details")]
        [SerializeField] private Image detailItemArtwork;
        [SerializeField] private TMP_Text detailItemName;
        [SerializeField] private TMP_Text detailItemType;
        [SerializeField] private TMP_Text detailItemWeight;
        [SerializeField] private TMP_Text detailAttackPhysical;
        [SerializeField] private TMP_Text detailAttackMagic;
        [SerializeField] private TMP_Text detailAttackFire;
        [SerializeField] private TMP_Text detailAttackLightning;
        [SerializeField] private TMP_Text detailAttackHoly;
        [SerializeField] private TMP_Text detailAttackCritical;
        [SerializeField] private TMP_Text detailGuardBoost;
        [SerializeField] private TMP_Text detailScaleStr;
        [SerializeField] private TMP_Text detailScaleDex;
        [SerializeField] private TMP_Text detailScaleInt;
        [SerializeField] private TMP_Text detailScaleFth;
        [SerializeField] private TMP_Text detailScaleArc;
        [SerializeField] private TMP_Text detailReqStr;
        [SerializeField] private TMP_Text detailReqDex;
        [SerializeField] private TMP_Text detailReqInt;
        [SerializeField] private TMP_Text detailReqFth;
        [SerializeField] private TMP_Text detailReqArc;
        [SerializeField] private TMP_Text detailSkillName;
        [SerializeField] private Image detailSkillIcon;
        [SerializeField] private TMP_Text detailSkillFpCost;
        [SerializeField] private TMP_Text detailEffectDescription;

        [Header("Column 2: Lore Card State")]
        [SerializeField] private TMP_Text loreItemName;
        [SerializeField] private Image loreItemArtwork;
        [SerializeField] private TMP_Text loreFullText;

        [Header("Column 3: Character Stats & Delta Comparison")]
        [SerializeField] private TMP_Text statVigor;
        [SerializeField] private TMP_Text statMind;
        [SerializeField] private TMP_Text statEndurance;
        [SerializeField] private TMP_Text statStrength;
        [SerializeField] private TMP_Text statDexterity;
        [SerializeField] private TMP_Text statIntelligence;
        [SerializeField] private TMP_Text statFaith;
        [SerializeField] private TMP_Text statArcane;
        [SerializeField] private TMP_Text statR1Attack;
        [SerializeField] private TMP_Text statL1Attack;
        [SerializeField] private TMP_Text statEquipLoadText;
        [SerializeField] private Image statEquipLoadBar;
        [SerializeField] private TMP_Text statPoise;

        [Header("Footer Legend")]
        [SerializeField] private TMP_Text legendSelectText;
        [SerializeField] private TMP_Text legendBackText;
        [SerializeField] private TMP_Text legendToggleLoreText;
        [SerializeField] private TMP_Text legendSimpleViewText;

        // Color Constants (Elden Ring Palette)
        public static readonly Color ColorParchmentPrimary = new Color(0.902f, 0.882f, 0.773f); // #E6E1C5
        public static readonly Color ColorParchmentSubdued = new Color(0.620f, 0.596f, 0.522f); // #9E9885
        public static readonly Color ColorStatBuff        = new Color(0.384f, 0.710f, 0.965f); // #62B5F6 Soft Blue
        public static readonly Color ColorStatNerf        = new Color(0.937f, 0.325f, 0.314f); // #EF5350 Soft Red
        public static readonly Color ColorUnmetRequirement = new Color(0.898f, 0.224f, 0.208f); // #E53935 Red

        private IInventoryPresenter _presenter;
        private List<InventorySlotUI> _spawnedSlots = new List<InventorySlotUI>();

        public void Initialize(IInventoryPresenter presenter)
        {
            _presenter = presenter;
        }

        public void PopulateGrid(List<InventoryItemSO> items, Func<InventoryItemSO, bool> isEquippedCheck, Func<InventoryItemSO, bool> meetsReqCheck)
        {
            ClearGrid();

            if (items == null || slotPrefab == null || gridContentParent == null) return;

            foreach (var item in items)
            {
                var slot = Instantiate(slotPrefab, gridContentParent);
                bool isEq = isEquippedCheck != null && isEquippedCheck(item);
                bool meetsReq = meetsReqCheck == null || meetsReqCheck(item);

                slot.Bind(item, 1, isEq, "R1", meetsReq);
                slot.OnSlotSelected += HandleSlotSelected;
                slot.OnSlotClicked += HandleSlotClicked;

                _spawnedSlots.Add(slot);
            }
        }

        public void ClearGrid()
        {
            foreach (var slot in _spawnedSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotSelected -= HandleSlotSelected;
                    slot.OnSlotClicked -= HandleSlotClicked;
                    Destroy(slot.gameObject);
                }
            }
            _spawnedSlots.Clear();
        }

        private void HandleSlotSelected(InventorySlotUI slot)
        {
            if (slot == null || slot.CurrentItem == null) return;
            DisplayItemDetails(slot.CurrentItem, slot.MeetsRequirements);
            _presenter?.OnItemFocused(slot.CurrentItem);
        }

        private void HandleSlotClicked(InventorySlotUI slot)
        {
            if (slot == null || slot.CurrentItem == null) return;
            _presenter?.OnItemSubmitted(slot.CurrentItem);
        }

        public void DisplayItemDetails(InventoryItemSO item, bool meetsRequirements = true)
        {
            if (item == null) return;

            // Details Column
            if (detailItemArtwork != null) detailItemArtwork.sprite = item.itemIcon;
            if (detailItemName != null) detailItemName.text = item.itemName;
            if (detailItemType != null) detailItemType.text = item.itemTypeLabel;
            if (detailItemWeight != null) detailItemWeight.text = item.weight.ToString("F1");

            if (detailAttackPhysical != null) detailAttackPhysical.text = item.physicalAttack.ToString();
            if (detailAttackMagic != null) detailAttackMagic.text = item.magicAttack.ToString();
            if (detailAttackFire != null) detailAttackFire.text = item.fireAttack.ToString();
            if (detailAttackLightning != null) detailAttackLightning.text = item.lightningAttack.ToString();
            if (detailAttackHoly != null) detailAttackHoly.text = item.holyAttack.ToString();
            if (detailAttackCritical != null) detailAttackCritical.text = item.critical.ToString();
            if (detailGuardBoost != null) detailGuardBoost.text = item.guardBoost.ToString("F0");

            if (detailScaleStr != null) detailScaleStr.text = item.scaleStrength.ToString();
            if (detailScaleDex != null) detailScaleDex.text = item.scaleDexterity.ToString();
            if (detailScaleInt != null) detailScaleInt.text = item.scaleIntelligence.ToString();
            if (detailScaleFth != null) detailScaleFth.text = item.scaleFaith.ToString();
            if (detailScaleArc != null) detailScaleArc.text = item.scaleArcane.ToString();

            // Requirements with unmet coloring
            SetRequirementField(detailReqStr, item.reqStrength, 10);
            SetRequirementField(detailReqDex, item.reqDexterity, 10);
            SetRequirementField(detailReqInt, item.reqIntelligence, 10);
            SetRequirementField(detailReqFth, item.reqFaith, 10);
            SetRequirementField(detailReqArc, item.reqArcane, 10);

            if (detailSkillName != null) detailSkillName.text = item.skillName;
            if (detailSkillIcon != null) detailSkillIcon.sprite = item.skillIcon;
            if (detailSkillFpCost != null) detailSkillFpCost.text = item.fpCost > 0 ? $"FP {item.fpCost}" : "-";
            if (detailEffectDescription != null) detailEffectDescription.text = item.effectDescription;

            // Lore Card Column
            if (loreItemName != null) loreItemName.text = item.itemName;
            if (loreItemArtwork != null) loreItemArtwork.sprite = item.itemIcon;
            if (loreFullText != null) loreFullText.text = $"{item.effectDescription}\n\n{item.loreDescription}";
        }

        private void SetRequirementField(TMP_Text field, int requiredVal, int playerVal)
        {
            if (field == null) return;
            field.text = requiredVal > 0 ? requiredVal.ToString() : "-";
            field.color = playerVal < requiredVal ? ColorUnmetRequirement : ColorParchmentPrimary;
        }

        public void UpdateStatComparison(int currentR1, int candidateR1, float currentWeight, float candidateWeight)
        {
            // Stat Delta formatting (Soft Blue for buff, Soft Red for nerf)
            if (statR1Attack != null)
            {
                int delta = candidateR1 - currentR1;
                if (delta > 0)
                {
                    statR1Attack.text = $"{candidateR1} (+{delta})";
                    statR1Attack.color = ColorStatBuff;
                }
                else if (delta < 0)
                {
                    statR1Attack.text = $"{candidateR1} ({delta})";
                    statR1Attack.color = ColorStatNerf;
                }
                else
                {
                    statR1Attack.text = candidateR1.ToString();
                    statR1Attack.color = ColorParchmentPrimary;
                }
            }
        }
    }
}
