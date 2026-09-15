#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Ui.Grace;
using SoulsLike.Ui.LevelUp;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace SoulsLike.Editor.Tests.LevelUp
{
    public sealed class LevelUpUiConfigurationTests
    {
        private const string LEVEL_UP_PREFAB_PATH = "Assets/Prefabs/Ui/LevelUp/LevelUpUi.prefab";
        private const string GRACE_UI_PREFAB_PATH = "Assets/Prefabs/Ui/Grace/GraceUi.prefab";
        private const string ASSET_MAPPING_PATH = "Assets/Settings/Data/AssetMappingData.asset";

        [Test]
        public void LevelUpUiPrefab_HasRequiredBindings()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LEVEL_UP_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, LEVEL_UP_PREFAB_PATH);

            LevelUpUi levelUpUi = prefab.GetComponent<LevelUpUi>();
            Assert.That(levelUpUi, Is.Not.Null, LEVEL_UP_PREFAB_PATH);

            AssertReferences(
                levelUpUi,
                "canvasGroup",
                "contentRoot",
                "currentLevelText",
                "nextLevelText",
                "currentRunesText",
                "projectedRunesText",
                "runesNeededText",
                "confirmButton",
                "backButton",
                "promptText",
                "hpCurrentText",
                "hpNextText",
                "fpCurrentText",
                "fpNextText",
                "staminaCurrentText",
                "staminaNextText",
                "equipLoadCurrentText",
                "equipLoadNextText",
                "poiseCurrentText",
                "poiseNextText",
                "discoveryCurrentText",
                "discoveryNextText");

            using var serialized = new SerializedObject(levelUpUi);
            SerializedProperty attrRows = serialized.FindProperty("attributeRows");
            Assert.That(attrRows.arraySize, Is.EqualTo(8), "LevelUpUi requires 8 attribute rows.");
            for (int i = 0; i < attrRows.arraySize; i++)
            {
                SerializedProperty row = attrRows.GetArrayElementAtIndex(i);
                Assert.That(row.FindPropertyRelative("labelText").objectReferenceValue, Is.Not.Null);
                Assert.That(row.FindPropertyRelative("currentValueText").objectReferenceValue, Is.Not.Null);
                Assert.That(row.FindPropertyRelative("nextValueText").objectReferenceValue, Is.Not.Null);
                Assert.That(row.FindPropertyRelative("decrementButton").objectReferenceValue, Is.Not.Null);
                Assert.That(row.FindPropertyRelative("incrementButton").objectReferenceValue, Is.Not.Null);
            }

            AssertReferenceArray(levelUpUi, "armamentCurrentTexts", 6);
            AssertReferenceArray(levelUpUi, "armamentNextTexts", 6);
            AssertReferenceArray(levelUpUi, "defenseCurrentTexts", 8);
            AssertReferenceArray(levelUpUi, "defenseNextTexts", 8);
            AssertReferenceArray(levelUpUi, "bodyCurrentTexts", 4);
            AssertReferenceArray(levelUpUi, "bodyNextTexts", 4);
        }

        [Test]
        public void GraceUiPrefab_HasLevelUpButton()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GRACE_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, GRACE_UI_PREFAB_PATH);

            GraceUi graceUi = prefab.GetComponent<GraceUi>();
            Assert.That(graceUi, Is.Not.Null, GRACE_UI_PREFAB_PATH);

            AssertReferences(graceUi, "levelUpButton");
        }

        [Test]
        public void LevelUpUi_IsRegisteredInAddressablesAndAssetMappingData()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.That(settings, Is.Not.Null);

            var group = settings.FindGroup("Ui");
            Assert.That(group, Is.Not.Null);

            bool foundAddressable = false;
            foreach (var entry in group.entries)
            {
                if (entry.address == "LevelUpUi")
                {
                    foundAddressable = true;
                    break;
                }
            }

            Assert.That(foundAddressable, Is.True, "LevelUpUi should be registered in Addressables group 'Ui'");

            var mappingData = AssetDatabase.LoadAssetAtPath<AssetMappingData>(ASSET_MAPPING_PATH);
            Assert.That(mappingData, Is.Not.Null);

            using var serialized = new SerializedObject(mappingData);
            SerializedProperty mappings = serialized.FindProperty("uiMappings.keyValue");
            bool foundMapping = false;
            for (int i = 0; i < mappings.arraySize; i++)
            {
                var elem = mappings.GetArrayElementAtIndex(i);
                if (elem.FindPropertyRelative("key").stringValue == "LevelUpUi")
                {
                    string guid = elem.FindPropertyRelative("value.m_AssetGUID").stringValue;
                    Assert.That(guid, Is.Not.Empty);
                    foundMapping = true;
                    break;
                }
            }

            Assert.That(foundMapping, Is.True, "LevelUpUi should be registered in AssetMappingData.uiMappings");
        }

        private static void AssertReferences(Component component, params string[] propertyNames)
        {
            using var serialized = new SerializedObject(component);
            foreach (string propertyName in propertyNames)
            {
                SerializedProperty prop = serialized.FindProperty(propertyName);
                Assert.That(
                    prop != null && prop.objectReferenceValue != null,
                    Is.True,
                    $"{component.GetType().Name} requires '{propertyName}'.");
            }
        }

        private static void AssertReferenceArray(
            Component component,
            string propertyName,
            int expectedSize)
        {
            using var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property.arraySize, Is.EqualTo(expectedSize));
            for (int index = 0; index < property.arraySize; index++)
            {
                Assert.That(property.GetArrayElementAtIndex(index).objectReferenceValue, Is.Not.Null);
            }
        }
    }
}
#endif
