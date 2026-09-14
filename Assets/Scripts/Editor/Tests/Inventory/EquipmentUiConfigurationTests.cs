#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Ui.Equipment;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Inventory
{
    public sealed class EquipmentUiConfigurationTests
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab";

        [TestCase("screenTitleText")]
        [TestCase("playerSummaryText")]
        [TestCase("equipmentGridContainer")]
        [TestCase("inventoryPickerOverlay")]
        [TestCase("inventoryPickerGridContainer")]
        [TestCase("comparisonPanel")]
        [TestCase("inventoryPickerSlotPrefab")]
        [TestCase("characterStatsUi")]
        [TestCase("loreCardUi")]
        [TestCase("selectedSlotText")]
        [TestCase("selectedItemNameText")]
        [TestCase("loadoutSummaryText")]
        [TestCase("actionPromptsText")]
        public void EquipmentPrefab_HasRequiredReference(string propertyName)
        {
            using var serialized = new SerializedObject(LoadView());
            Assert.That(serialized.FindProperty(propertyName).objectReferenceValue, Is.Not.Null,
                $"{PREFAB_PATH}: '{propertyName}' must be assigned.");
        }

        [TestCase("rightHandSlots", 3)]
        [TestCase("leftHandSlots", 3)]
        [TestCase("ammoSlots", 4)]
        [TestCase("armorSlots", 4)]
        [TestCase("talismanSlots", 4)]
        [TestCase("quickItemSlots", 10)]
        public void EquipmentPrefab_HasRequiredSlotTopology(string propertyName, int expectedCount)
        {
            using var serialized = new SerializedObject(LoadView());
            SerializedProperty slots = serialized.FindProperty(propertyName);
            Assert.That(slots.arraySize, Is.EqualTo(expectedCount), propertyName);
            for (int index = 0; index < slots.arraySize; index++)
            {
                Assert.That(slots.GetArrayElementAtIndex(index).objectReferenceValue, Is.Not.Null,
                    $"{PREFAB_PATH}: '{propertyName}[{index}]' must be assigned.");
            }
        }

        private static EquipmentUi LoadView()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, PREFAB_PATH);
            EquipmentUi view = prefab.GetComponent<EquipmentUi>();
            Assert.That(view, Is.Not.Null, PREFAB_PATH);
            return view;
        }
    }
}
#endif
