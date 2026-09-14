#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Inventory.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SoulsLike.Editor.Tests.Inventory
{
    public sealed class InventoryGridPresentationTests
    {
        private const string INVENTORY_UI_PREFAB_PATH = "Assets/Prefabs/Ui/Inventory/InventoryUi.prefab";

        private Scene _previewScene;
        private GameObject _instance;
        private InventoryUi _inventoryUi;
        private RectTransform[] _emptySlotBackgrounds;

        [SetUp]
        public void SetUp()
        {
            _previewScene = EditorSceneManager.NewPreviewScene();
            InventoryUi prefab = AssetDatabase.LoadAssetAtPath<InventoryUi>(INVENTORY_UI_PREFAB_PATH);
            _instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject, _previewScene);
            _inventoryUi = _instance.GetComponent<InventoryUi>();
            _inventoryUi.AssignPresenter(new TestPresenter());
            _emptySlotBackgrounds = (RectTransform[])typeof(InventoryUi)
                .GetField("emptySlotBackgrounds", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_inventoryUi);
        }

        [TearDown]
        public void TearDown()
        {
            if (_inventoryUi != null)
            {
                ClearGridWithExpectedEditModeDestroyLogs();
            }

            if (_instance != null)
            {
                UnityEngine.Object.DestroyImmediate(_instance);
            }

            if (_previewScene.IsValid())
            {
                EditorSceneManager.ClosePreviewScene(_previewScene);
            }
        }

        [TestCase(0, 25)]
        [TestCase(1, 24)]
        [TestCase(24, 1)]
        [TestCase(25, 0)]
        [TestCase(26, 4)]
        public void PopulateGrid_UsesEmptySlotsToFillMinimumAndPartialRows(
            int itemCount,
            int expectedEmptySlotCount)
        {
            _inventoryUi.PopulateGrid(CreateItems(itemCount));

            Assert.That(GetActiveItemSlots().Count, Is.EqualTo(itemCount));
            Assert.That(GetActiveEmptySlotCount(), Is.EqualTo(expectedEmptySlotCount));
            Assert.That(GetActiveItemSlots().Count + GetActiveEmptySlotCount(),
                Is.EqualTo(Mathf.Max(25, Mathf.CeilToInt(itemCount / 5f) * 5)));
            AssertEmptySlotsFollowItemSlots();
        }

        [Test]
        public void PopulateGrid_AfterRepopulationAndClear_UpdatesStaticEmptySlots()
        {
            _inventoryUi.PopulateGrid(CreateItems(26));
            for (int index = 0; index < 26; index++)
            {
                LogAssert.Expect(LogType.Error,
                    new Regex("^Destroy may not be called from edit mode!"));
            }

            _inventoryUi.PopulateGrid(CreateItems(1));

            Assert.That(GetActiveItemSlots().Count, Is.EqualTo(1));
            Assert.That(GetActiveEmptySlotCount(), Is.EqualTo(24));
            AssertEmptySlotsFollowItemSlots();

            ClearGridWithExpectedEditModeDestroyLogs();

            Assert.That(GetActiveItemSlots(), Is.Empty);
            Assert.That(GetActiveEmptySlotCount(), Is.EqualTo(25));
        }

        private List<InventorySlotUI> GetActiveItemSlots()
        {
            var activeSlots = new List<InventorySlotUI>();
            foreach (InventorySlotUI slot in _emptySlotBackgrounds[0]
                         .parent.GetComponentsInChildren<InventorySlotUI>())
            {
                activeSlots.Add(slot);
            }

            return activeSlots;
        }

        private int GetActiveEmptySlotCount()
        {
            int activeSlotCount = 0;
            foreach (RectTransform emptySlotBackground in _emptySlotBackgrounds)
            {
                if (emptySlotBackground.gameObject.activeSelf)
                {
                    activeSlotCount++;
                }
            }

            return activeSlotCount;
        }

        private void AssertEmptySlotsFollowItemSlots()
        {
            foreach (InventorySlotUI itemSlot in GetActiveItemSlots())
            {
                foreach (RectTransform emptySlotBackground in _emptySlotBackgrounds)
                {
                    if (emptySlotBackground.gameObject.activeSelf)
                    {
                        Assert.That(itemSlot.transform.GetSiblingIndex(),
                            Is.LessThan(emptySlotBackground.GetSiblingIndex()));
                    }
                }
            }
        }

        private void ClearGridWithExpectedEditModeDestroyLogs()
        {
            foreach (InventorySlotUI _ in GetActiveItemSlots())
            {
                LogAssert.Expect(LogType.Error,
                    new Regex("^Destroy may not be called from edit mode!"));
            }

            _inventoryUi.ClearGrid();
        }

        private static List<InventoryItemViewData> CreateItems(int count)
        {
            var items = new List<InventoryItemViewData>(count);
            for (int index = 0; index < count; index++)
            {
                items.Add(CreateItem(index));
            }

            return items;
        }

        private static InventoryItemViewData CreateItem(int index)
        {
            var definition = new ItemDefinition();
            SetField(definition, "itemId", ItemId.GreatSword);
            SetField(definition, "itemType", ItemType.Weapon);
            SetField(definition, "displayName", $"Test item {index}");
            SetField(definition, "description", string.Empty);
            SetField(definition, "loreDescription", string.Empty);
            SetField(definition, "maxStack", 1);

            ConstructorInfo constructor = typeof(InventoryItemViewData).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(InventoryEntry),
                    typeof(ItemDefinition),
                    typeof(ItemStatSnapshot),
                    typeof(Sprite),
                    typeof(bool),
                    typeof(string),
                    typeof(bool)
                },
                null);
            return (InventoryItemViewData)constructor.Invoke(new object[]
            {
                new InventoryEntry(InventoryEntryId.Create(), ItemId.GreatSword, 1),
                definition,
                ItemStatSnapshot.Empty,
                null,
                false,
                string.Empty,
                true
            });
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private sealed class TestPresenter : IInventoryPresenter
        {
            public void SelectPrimaryCategory(InventoryPrimaryCategory category) { }
            public void SelectSubCategory(InventorySubCategory category) { }
            public void OnItemFocused(InventoryEntryId entryId) { }
            public void OnItemSubmitted(InventoryEntryId entryId) { }
            public void CloseInventory() { }
            public void ToggleLoreView() { }
            public void ToggleSimpleView() { }
        }
    }
}
#endif
