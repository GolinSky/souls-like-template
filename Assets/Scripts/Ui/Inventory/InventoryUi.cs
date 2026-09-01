using System;
using System.Collections.Generic;
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
        [SerializeField] private ItemDetailsUi itemDetailsUi;

        [Header("Column 2: Lore Card")]
        [SerializeField] private LoreCardUi loreCardUi;

        [Header("Column 3: Character Stats")]
        [SerializeField] private CharacterStatsUi characterStatsUi;

        [Header("Footer Legend")]
        [SerializeField] private TMP_Text legendSelectText;
        [SerializeField] private TMP_Text legendBackText;
        [SerializeField] private TMP_Text legendToggleLoreText;
        [SerializeField] private TMP_Text legendSimpleViewText;

        private readonly List<InventorySlotUI> _spawnedSlots = new();
        private IInventoryPresenter _presenter;

        public CharacterStatsUi CharacterStats => characterStatsUi;
        public ItemDetailsUi ItemDetails => itemDetailsUi;
        public LoreCardUi LoreCard => loreCardUi;
        public static Color ColorParchmentPrimary => ItemDetailsUi.ColorParchmentPrimary;
        public static Color ColorUnmetRequirement => ItemDetailsUi.ColorUnmetRequirement;

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
                || itemDetailsUi == null
                || loreCardUi == null
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

    }
}
