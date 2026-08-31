using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character;
using SoulsLike.Items;
using SoulsLike.Ui.Base;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public sealed class InventoryUi : BaseUi
    {
        private const int GRID_COLUMN_COUNT = 5;

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
        [SerializeField] private TMP_Text detailSkillFpCost;
        [SerializeField] private TMP_Text detailEffectDescription;

        [Header("Column 2: Lore Card State")]
        [SerializeField] private TMP_Text loreItemName;
        [SerializeField] private Image loreItemArtwork;
        [SerializeField] private TMP_Text loreFullText;

        [Header("Column 3: Character Stats")]
        [SerializeField] private CharacterStatsUi characterStatsUi;

        [Header("Footer Legend")]
        [SerializeField] private TMP_Text legendSelectText;
        [SerializeField] private TMP_Text legendBackText;
        [SerializeField] private TMP_Text legendToggleLoreText;
        [SerializeField] private TMP_Text legendSimpleViewText;

        public static readonly Color ColorParchmentPrimary = new(0.902f, 0.882f, 0.773f);
        public static readonly Color ColorUnmetRequirement = new(0.898f, 0.224f, 0.208f);

        private readonly List<InventorySlotUI> _spawnedSlots = new();
        private IInventoryPresenter _presenter;

        public CharacterStatsUi CharacterStats => characterStatsUi;

        public void AssignPresenter(IInventoryPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public override void Show()
        {
            base.Show();
            SelectFirstSlot();
        }

        public void PopulateGrid(IReadOnlyList<InventoryItemViewData> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            RequirePresenter();
            ClearGrid();
            foreach (InventoryItemViewData item in items)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, gridContentParent);
                slot.Bind(item);
                slot.SlotSelected += HandleSlotSelected;
                slot.SlotSubmitted += HandleSlotSubmitted;
                _spawnedSlots.Add(slot);
            }

            ConfigureGridNavigation();
            if (IsActive)
            {
                SelectFirstSlot();
            }
        }

        public void DisplayItemDetails(
            InventoryItemViewData item,
            CharacterAttributeStats attributes)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ItemStatSnapshot stats = item.Stats;
            detailItemArtwork.sprite = item.Icon;
            detailItemArtwork.enabled = item.Icon != null;
            detailItemName.text = item.DisplayName;
            detailItemType.text = item.ItemType.ToString();
            detailItemWeight.text = item.Weight.ToString("F1");
            detailAttackPhysical.text = stats.PhysicalAttack.ToString();
            detailAttackMagic.text = stats.MagicAttack.ToString();
            detailAttackFire.text = stats.FireAttack.ToString();
            detailAttackLightning.text = stats.LightningAttack.ToString();
            detailAttackHoly.text = stats.HolyAttack.ToString();
            detailAttackCritical.text = stats.Critical.ToString();
            detailGuardBoost.text = stats.GuardBoost.ToString("F0");
            detailScaleStr.text = FormatScaling(stats.Scaling.Strength);
            detailScaleDex.text = FormatScaling(stats.Scaling.Dexterity);
            detailScaleInt.text = FormatScaling(stats.Scaling.Intelligence);
            detailScaleFth.text = FormatScaling(stats.Scaling.Faith);
            detailScaleArc.text = FormatScaling(stats.Scaling.Arcane);
            SetRequirementField(detailReqStr, stats.Requirements.Strength, attributes.Strength);
            SetRequirementField(detailReqDex, stats.Requirements.Dexterity, attributes.Dexterity);
            SetRequirementField(detailReqInt, stats.Requirements.Intelligence, attributes.Intelligence);
            SetRequirementField(detailReqFth, stats.Requirements.Faith, attributes.Faith);
            SetRequirementField(detailReqArc, stats.Requirements.Arcane, attributes.Arcane);
            detailSkillName.text = string.IsNullOrWhiteSpace(stats.SkillName) ? "-" : stats.SkillName;
            detailSkillFpCost.text = stats.SkillFocusCost > 0 ? $"FP {stats.SkillFocusCost}" : "-";
            detailEffectDescription.text = item.Description;

            loreItemName.text = item.DisplayName;
            loreItemArtwork.sprite = item.Icon;
            loreItemArtwork.enabled = item.Icon != null;
            loreFullText.text = $"{item.Description}\n\n{item.LoreDescription}";
        }

        public void ToggleLoreView() => viewStateController.ToggleLoreView();
        public void ToggleSimpleView() => viewStateController.ToggleSimpleView();

        public void ClearGrid()
        {
            foreach (InventorySlotUI slot in _spawnedSlots)
            {
                slot.SlotSelected -= HandleSlotSelected;
                slot.SlotSubmitted -= HandleSlotSubmitted;
                Destroy(slot.gameObject);
            }

            _spawnedSlots.Clear();
        }

        protected override void Awake()
        {
            base.Awake();
            if (viewStateController == null
                || screenTitleText == null
                || primaryCategoryTabContainer == null
                || subCategoryIconContainer == null
                || gridContentParent == null
                || gridScrollRect == null
                || slotPrefab == null
                || characterStatsUi == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InventoryUi)} '{name}' has missing structural references.");
            }

            screenTitleText.text = "INVENTORY";
        }

        private void OnDestroy()
        {
            ClearGrid();
        }

        private void HandleSlotSelected(InventorySlotUI slot)
        {
            RequirePresenter().OnItemFocused(slot.CurrentItem.EntryId);
        }

        private void HandleSlotSubmitted(InventorySlotUI slot)
        {
            RequirePresenter().OnItemSubmitted(slot.CurrentItem.EntryId);
        }

        private void ConfigureGridNavigation()
        {
            for (int index = 0; index < _spawnedSlots.Count; index++)
            {
                InventorySlotUI up = index >= GRID_COLUMN_COUNT
                    ? _spawnedSlots[index - GRID_COLUMN_COUNT]
                    : null;
                InventorySlotUI down = index + GRID_COLUMN_COUNT < _spawnedSlots.Count
                    ? _spawnedSlots[index + GRID_COLUMN_COUNT]
                    : null;
                InventorySlotUI left = index % GRID_COLUMN_COUNT > 0
                    ? _spawnedSlots[index - 1]
                    : null;
                InventorySlotUI right = index % GRID_COLUMN_COUNT < GRID_COLUMN_COUNT - 1
                    && index + 1 < _spawnedSlots.Count
                    ? _spawnedSlots[index + 1]
                    : null;
                _spawnedSlots[index].ConfigureNavigation(up, down, left, right);
            }
        }

        private void SelectFirstSlot()
        {
            if (_spawnedSlots.Count > 0)
            {
                _spawnedSlots[0].Select();
            }
        }

        private IInventoryPresenter RequirePresenter()
        {
            return _presenter ?? throw new InvalidOperationException(
                $"{nameof(InventoryUi)} requires a presenter before use.");
        }

        private static string FormatScaling(Items.ScalingGrade grade)
        {
            return grade == Items.ScalingGrade.None ? "-" : grade.ToString();
        }

        private static void SetRequirementField(TMP_Text field, int requiredValue, int playerValue)
        {
            field.text = requiredValue > 0 ? requiredValue.ToString() : "-";
            field.color = playerValue < requiredValue
                ? ColorUnmetRequirement
                : ColorParchmentPrimary;
        }
    }
}
