using System;
using System.Collections.Generic;
using SoulsLike.Ui.Base;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UI.Base;

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
        [SerializeField] private CustomButtonToggle[] primaryCategoryToggles;
        [SerializeField] private CustomButtonToggle[] subCategoryToggles;

        [Header("Column 1: Grid Panel")]
        [SerializeField] private Transform gridContentParent;
        [SerializeField] private ScrollRect gridScrollRect;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private RectTransform[] emptySlotBackgrounds;

        [Header("Grid Header")]
        [SerializeField] private TMP_Text currentPrimaryCategoryText;
        [SerializeField] private TMP_Text selectedItemNameText;
        [SerializeField] private TMP_Text itemSelectionPositionText;

        [Header("Column 2: Item Details")]
        [SerializeField] private ItemDetailsUi itemDetailsUi;

        [Header("Column 2: Lore Card")]
        [SerializeField] private LoreCardUi loreCardUi;

        [Header("Column 3: Character Stats")]
        [SerializeField] private CharacterStatsUi characterStatsUi;
        [SerializeField] private TextMeshProUGUI runesHeldText;

        [Header("Footer Legend")]
        [SerializeField] private TMP_Text legendSelectText;
        [SerializeField] private TMP_Text legendBackText;
        [SerializeField] private TMP_Text legendToggleLoreText;
        [SerializeField] private TMP_Text legendSimpleViewText;

        private readonly List<InventorySlotUI> _spawnedSlots = new();
        private IInventoryPresenter _presenter;
        private UnityAction<bool>[] _primaryCategoryListeners;
        private UnityAction<bool>[] _subCategoryListeners;
        private InventoryPrimaryCategory _activePrimaryCategory = InventoryPrimaryCategory.Weapons;
        private bool _isSynchronizingSubCategorySelections;

        public CharacterStatsUi CharacterStats => characterStatsUi;
        public ItemDetailsUi ItemDetails => itemDetailsUi;
        public LoreCardUi LoreCard => loreCardUi;
        public static Color ColorParchmentPrimary => ItemDetailsUi.ColorParchmentPrimary;
        public static Color ColorUnmetRequirement => ItemDetailsUi.ColorUnmetRequirement;

        public void AssignPresenter(IInventoryPresenter presenter)
        {
            _presenter = presenter;
        }

        public override void Show()
        {
            base.Show();
            SelectFirstSlot();
        }

        public void PopulateGrid(IReadOnlyList<InventoryItemViewData> items)
        {
            ClearGrid();
            foreach (InventoryItemViewData item in items)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, gridContentParent);
                slot.Bind(item);
                slot.SlotSelected += HandleSlotSelected;
                slot.SlotSubmitted += HandleSlotSubmitted;
                _spawnedSlots.Add(slot);
            }

            UpdateEmptySlots(items.Count);
            ConfigureGridNavigation();
            if (IsActive)
            {
                SelectFirstSlot();
            }
        }

        public void ToggleLoreView() => viewStateController.ToggleLoreView();
        public void ToggleSimpleView() => viewStateController.ToggleSimpleView();

        public void DisplayHeldCurrency(int heldCurrency)
        {
            runesHeldText.text = heldCurrency.ToString("N0");
        }

        public void DisplayGridHeader(
            InventoryPrimaryCategory primaryCategory,
            int itemCount)
        {
            currentPrimaryCategoryText.text = primaryCategory switch
            {
                InventoryPrimaryCategory.KeyItems => "Key Items",
                _ => primaryCategory.ToString()
            };

            if (itemCount == 0)
            {
                selectedItemNameText.text = "No items";
                itemSelectionPositionText.text = "00/00";
            }
        }

        public void DisplaySelectedItem(
            InventoryItemViewData item,
            int selectedPosition,
            int itemCount)
        {
            selectedItemNameText.text = item.DisplayName;
            itemSelectionPositionText.text = $"{selectedPosition:D2}/{itemCount:D2}";
        }

        public void SetCategoryControlsVisible(bool isVisible)
        {
            primaryCategoryTabContainer.gameObject.SetActive(isVisible);
            subCategoryIconContainer.gameObject.SetActive(isVisible);
        }

        public void ClearGrid()
        {
            foreach (InventorySlotUI slot in _spawnedSlots)
            {
                slot.SlotSelected -= HandleSlotSelected;
                slot.SlotSubmitted -= HandleSlotSubmitted;
                slot.gameObject.SetActive(false);
                Destroy(slot.gameObject);
            }

            _spawnedSlots.Clear();
            UpdateEmptySlots(0);
        }

        protected override void Awake()
        {
            base.Awake();
            screenTitleText.text = "INVENTORY";// todo: use const
            InitializeCategoryControls();
        }

        private void OnDestroy()
        {
            RemoveCategoryListeners();
            ClearGrid();
        }

        private void InitializeCategoryControls()
        {
            _primaryCategoryListeners = new UnityAction<bool>[primaryCategoryToggles.Length];
            _subCategoryListeners = new UnityAction<bool>[subCategoryToggles.Length];

            for (int index = 0; index < primaryCategoryToggles.Length; index++)
            {
                CustomButtonToggle toggle = primaryCategoryToggles[index];
                InventoryPrimaryCategory category = (InventoryPrimaryCategory)index;
                toggle.SetText(category switch
                {
                    InventoryPrimaryCategory.KeyItems => "Key items",
                    InventoryPrimaryCategory.Talisman => "Talismans",
                    _ => category.ToString()
                });
                toggle.isOn = category == InventoryPrimaryCategory.Weapons;
                _primaryCategoryListeners[index] = isOn => HandlePrimaryCategoryValueChanged(category, isOn);
                toggle.onValueChanged.AddListener(_primaryCategoryListeners[index]);
            }

            for (int index = 0; index < subCategoryToggles.Length; index++)
            {
                CustomButtonToggle toggle = subCategoryToggles[index];
                InventorySubCategory category = (InventorySubCategory)index;
                // todo: create mapping in data or model
                toggle.SetText(category switch
                {
                    InventorySubCategory.MeleeWeapon => "Melee",
                    InventorySubCategory.RangedWeapon => "Ranged",
                    InventorySubCategory.Shield => "Shields",
                    InventorySubCategory.HeadArmor => "Head",
                    InventorySubCategory.ChestArmor => "Chest",
                    InventorySubCategory.ArmArmor => "Arms",
                    InventorySubCategory.LegArmor => "Legs",
                    InventorySubCategory.Talisman => "Talismans",
                    InventorySubCategory.CraftingMaterial => "Materials",
                    InventorySubCategory.ConsumableItem => "Consumables",
                    InventorySubCategory.KeyItem => "Key items",
                    _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
                });
                toggle.SetIsOnWithoutNotify(false);
                _subCategoryListeners[index] = isOn => HandleSubCategoryValueChanged(category, isOn);
                toggle.onValueChanged.AddListener(_subCategoryListeners[index]);
            }

            UpdateSubCategoryVisibility(InventoryPrimaryCategory.Weapons);
        }

        private void RemoveCategoryListeners()
        {
            if (_primaryCategoryListeners != null)
            {
                for (int index = 0; index < _primaryCategoryListeners.Length; index++)
                {
                    primaryCategoryToggles[index].onValueChanged.RemoveListener(_primaryCategoryListeners[index]);
                }
            }

            if (_subCategoryListeners != null)
            {
                for (int index = 0; index < _subCategoryListeners.Length; index++)
                {
                    subCategoryToggles[index].onValueChanged.RemoveListener(_subCategoryListeners[index]);
                }
            }
        }

        private void HandlePrimaryCategoryValueChanged(InventoryPrimaryCategory category, bool isOn)
        {
            if (!isOn)
            {
                return;
            }

            _activePrimaryCategory = category;
            ClearSubCategorySelections();
            UpdateSubCategoryVisibility(category);
            _presenter.SelectPrimaryCategory(category);
        }

        private void HandleSubCategoryValueChanged(InventorySubCategory category, bool isOn)
        {
            if (isOn)
            {
                _presenter.SelectSubCategory(category);
                return;
            }

            if (_isSynchronizingSubCategorySelections || HasSelectedSubCategory())
            {
                return;
            }

            _presenter.SelectPrimaryCategory(_activePrimaryCategory);
        }

        private void ClearSubCategorySelections()
        {
            _isSynchronizingSubCategorySelections = true;
            try
            {
                foreach (CustomButtonToggle toggle in subCategoryToggles)
                {
                    toggle.isOn = false;
                }
            }
            finally
            {
                _isSynchronizingSubCategorySelections = false;
            }
        }

        private bool HasSelectedSubCategory()
        {
            foreach (CustomButtonToggle toggle in subCategoryToggles)
            {
                if (toggle.isOn)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateSubCategoryVisibility(InventoryPrimaryCategory primaryCategory)
        {
            for (int index = 0; index < subCategoryToggles.Length; index++)
            {
                InventorySubCategory subCategory = (InventorySubCategory)index;
                subCategoryToggles[index].gameObject.SetActive(
                    IsSubCategoryOfPrimaryCategory(subCategory, primaryCategory));
            }

            ConfigureCategoryNavigation(primaryCategory);
        }

        private void ConfigureCategoryNavigation(InventoryPrimaryCategory primaryCategory)
        {
            var activeSubCategoryToggles = new List<CustomButtonToggle>();
            foreach (CustomButtonToggle toggle in subCategoryToggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    activeSubCategoryToggles.Add(toggle);
                }
            }

            ConfigurePrimaryCategoryNavigation(activeSubCategoryToggles);
            ConfigureSubCategoryNavigation(primaryCategory, activeSubCategoryToggles);
        }

        private void ConfigurePrimaryCategoryNavigation(
            IReadOnlyList<CustomButtonToggle> activeSubCategoryToggles)
        {
            for (int index = 0; index < primaryCategoryToggles.Length; index++)
            {
                UnityEngine.UI.Navigation navigation = primaryCategoryToggles[index].navigation;
                navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
                navigation.selectOnLeft = index > 0 ? primaryCategoryToggles[index - 1] : null;
                navigation.selectOnRight = index + 1 < primaryCategoryToggles.Length
                    ? primaryCategoryToggles[index + 1]
                    : null;
                navigation.selectOnDown = activeSubCategoryToggles.Count > 0
                    ? activeSubCategoryToggles[0]
                    : null;
                navigation.selectOnUp = null;
                primaryCategoryToggles[index].navigation = navigation;
            }
        }

        private void ConfigureSubCategoryNavigation(
            InventoryPrimaryCategory primaryCategory,
            IReadOnlyList<CustomButtonToggle> activeSubCategoryToggles)
        {
            CustomButtonToggle primaryCategoryToggle = primaryCategoryToggles[(int)primaryCategory];
            for (int index = 0; index < activeSubCategoryToggles.Count; index++)
            {
                UnityEngine.UI.Navigation navigation = activeSubCategoryToggles[index].navigation;
                navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
                navigation.selectOnLeft = index > 0 ? activeSubCategoryToggles[index - 1] : null;
                navigation.selectOnRight = index + 1 < activeSubCategoryToggles.Count
                    ? activeSubCategoryToggles[index + 1]
                    : null;
                navigation.selectOnUp = primaryCategoryToggle;
                navigation.selectOnDown = null;
                activeSubCategoryToggles[index].navigation = navigation;
            }
        }

        private static bool IsSubCategoryOfPrimaryCategory(
            InventorySubCategory subCategory,
            InventoryPrimaryCategory primaryCategory)
        {
            return primaryCategory switch
            {
                InventoryPrimaryCategory.Weapons => subCategory is InventorySubCategory.MeleeWeapon
                    or InventorySubCategory.RangedWeapon
                    or InventorySubCategory.Shield,
                InventoryPrimaryCategory.Armor => subCategory is InventorySubCategory.HeadArmor
                    or InventorySubCategory.ChestArmor
                    or InventorySubCategory.ArmArmor
                    or InventorySubCategory.LegArmor,
                InventoryPrimaryCategory.Talisman => subCategory == InventorySubCategory.Talisman,
                InventoryPrimaryCategory.Consumables => subCategory is InventorySubCategory.CraftingMaterial
                    or InventorySubCategory.ConsumableItem,
                InventoryPrimaryCategory.KeyItems => subCategory == InventorySubCategory.KeyItem,
                _ => throw new ArgumentOutOfRangeException(nameof(primaryCategory), primaryCategory, null)
            };
        }

        private void HandleSlotSelected(InventorySlotUI slot)
        {
            DisplaySelectedItem(
                slot.CurrentItem,
                _spawnedSlots.IndexOf(slot) + 1,
                _spawnedSlots.Count);
            _presenter.OnItemFocused(slot.CurrentItem.EntryId);
        }

        private void HandleSlotSubmitted(InventorySlotUI slot)
        {
            _presenter.OnItemSubmitted(slot.CurrentItem.EntryId);
        }

        private void ConfigureGridNavigation()
        {
            Selectable upCategoryTarget = GetUpCategoryTarget();
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
                _spawnedSlots[index].ConfigureNavigation(
                    up,
                    down,
                    left,
                    right,
                    index < GRID_COLUMN_COUNT ? upCategoryTarget : null);
            }
        }

        private void UpdateEmptySlots(int itemCount)
        {
            int requiredCellCount = Mathf.Max(
                emptySlotBackgrounds.Length,
                Mathf.CeilToInt(itemCount / (float)GRID_COLUMN_COUNT) * GRID_COLUMN_COUNT);
            int requiredEmptySlotCount = requiredCellCount - itemCount;

            for (int index = 0; index < emptySlotBackgrounds.Length; index++)
            {
                bool isRequired = index < requiredEmptySlotCount;
                emptySlotBackgrounds[index].gameObject.SetActive(isRequired);
                if (isRequired)
                {
                    emptySlotBackgrounds[index].SetAsLastSibling();
                }
            }
        }

        private Selectable GetUpCategoryTarget()
        {
            foreach (CustomButtonToggle toggle in subCategoryToggles)
            {
                if (toggle.gameObject.activeInHierarchy)
                {
                    return toggle;
                }
            }

            foreach (CustomButtonToggle toggle in primaryCategoryToggles)
            {
                if (toggle.isOn && toggle.gameObject.activeInHierarchy)
                {
                    return toggle;
                }
            }

            return null;
        }

        private void SelectFirstSlot()
        {
            if (_spawnedSlots.Count > 0)
            {
                _spawnedSlots[0].Select();
            }
        }

    }
}
