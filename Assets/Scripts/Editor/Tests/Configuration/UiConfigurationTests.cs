#if UNITY_EDITOR
using System;
using NUnit.Framework;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.PauseNavigation;
using SoulsLike.Ui.Settings;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Configuration
{
    public sealed class UiConfigurationTests
    {
        [TestCase("Assets/Prefabs/Ui/Inventory/InventoryUi.prefab", typeof(InventoryUi))]
        [TestCase("Assets/Prefabs/Ui/Inventory/InventorySlot.prefab", typeof(InventorySlotUI))]
        [TestCase("Assets/Prefabs/Ui/Inventory/InventorySlotInventory.prefab", typeof(InventorySlotUI))]
        [TestCase("Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab", typeof(EquipmentUi))]
        [TestCase("Assets/Prefabs/Ui/Settings/SettingsUi.prefab", typeof(SettingsUi))]
        [TestCase("Assets/Prefabs/Ui/PauseNavigation/PauseNavigationUi.prefab", typeof(PauseNavigationUi))]
        public void UiPrefab_HasRequiredReferences(string path, Type rootType)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            Assert.That(prefab.GetComponent(rootType), Is.Not.Null, $"{path}: {rootType.Name}");

            foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
            {
                switch (component)
                {
                    case InventorySlotUI slot:
                        AssertReferences(slot, path, "backgroundBox", "focusFrame", "equippedBadgeBox",
                            "unmetRequirementOverlay", "itemIcon", "quantityText", "equippedBadgeText", "ashOfWarIcon");
                        break;
                    case EquipmentSlotUI slot:
                        AssertReferences(slot, path, "iconImage", "borderImage", "selectionHighlight",
                            "lockOverlay", "quantityText", "emptyIcon", "equippedBadge");
                        break;
                    case InventoryViewStateController state:
                        AssertReferences(state, path, "gridColumnGroup", "detailsColumnGroup", "loreCardGroup", "statsColumnGroup");
                        break;
                    case InventoryUi inventory:
                        AssertReferenceArray(inventory, path, "primaryCategoryToggles");
                        AssertReferenceArray(inventory, path, "subCategoryToggles");
                        break;
                    case SettingsUi settings:
                        AssertReferences(settings, path, "audioTabButton", "cameraTabButton", "graphicsTabButton",
                            "controlsTabButton", "applyButton", "defaultsButton", "backButton", "displayConfirmationPanel",
                            "displayConfirmationText", "keepDisplayButton", "revertDisplayButton", "unsavedChangesPanel",
                            "applyUnsavedButton", "discardUnsavedButton", "continueEditingButton");
                        AssertReferenceArray(settings, path, "options");
                        break;
                    case SettingsOptionUi option:
                        using (var serialized = new SerializedObject(option))
                        {
                            bool hasControl = serialized.FindProperty("slider").objectReferenceValue != null
                                || serialized.FindProperty("toggle").objectReferenceValue != null
                                || serialized.FindProperty("actionButton").objectReferenceValue != null;
                            Assert.That(hasControl, Is.True, $"{path}: '{option.name}' requires an option control.");
                        }
                        break;
                    case PauseNavigationUi pause:
                        AssertReferences(pause, path, "openEquipmentButton", "openInventoryButton", "openSystemButton");
                        break;
                }
            }
        }

        private static void AssertReferences(Component component, string path, params string[] propertyNames)
        {
            using var serialized = new SerializedObject(component);
            foreach (string propertyName in propertyNames)
            {
                Assert.That(serialized.FindProperty(propertyName).objectReferenceValue, Is.Not.Null,
                    $"{path}: {component.GetType().Name} '{component.name}' requires '{propertyName}'.");
            }
        }

        private static void AssertReferenceArray(Component component, string path, string propertyName)
        {
            using var serialized = new SerializedObject(component);
            SerializedProperty array = serialized.FindProperty(propertyName);
            Assert.That(array.arraySize, Is.GreaterThan(0), $"{path}: '{propertyName}' must contain controls.");
            for (int index = 0; index < array.arraySize; index++)
            {
                Assert.That(array.GetArrayElementAtIndex(index).objectReferenceValue, Is.Not.Null,
                    $"{path}: '{propertyName}[{index}]' must be assigned.");
            }
        }
    }
}
#endif
