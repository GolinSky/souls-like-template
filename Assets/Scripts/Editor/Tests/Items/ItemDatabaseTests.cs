#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Items;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Items
{
    public sealed class ItemDatabaseTests
    {
        [TestCase(nameof(ItemDatabase))]
        [TestCase(nameof(WeaponDatabase))]
        [TestCase(nameof(ShieldDatabase))]
        [TestCase(nameof(ConsumableDatabase))]
        public void DatabaseAssets_HaveUniqueNonEmptyItemIds(string databaseType)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{databaseType}", new[] { "Assets" });
            Assert.That(guids, Is.Not.Empty, $"No {databaseType} assets were found.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                Assert.That(database, Is.Not.Null, path);
                using var serialized = new SerializedObject(database);
                SerializedProperty items = serialized.FindProperty("items");
                var ids = new HashSet<ItemId>();
                for (int index = 0; index < items.arraySize; index++)
                {
                    var id = (ItemId)items.GetArrayElementAtIndex(index).FindPropertyRelative("itemId").intValue;
                    Assert.That(id, Is.Not.EqualTo(ItemId.None), $"{path}: definition {index} requires an ItemId.");
                    Assert.That(ids.Add(id), Is.True, $"{path}: duplicate ItemId '{id}'.");
                }
            }
        }

        [Test]
        public void WeaponDatabaseAssets_DefinitionsHaveAttackSfx()
        {
            string[] guids = AssetDatabase.FindAssets("t:WeaponDatabase", new[] { "Assets" });
            Assert.That(guids, Is.Not.Empty);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath<WeaponDatabase>(path);
                foreach (WeaponDefinition definition in database.Items)
                {
                    Assert.That(definition, Is.Not.Null, path);
                    Assert.That(definition.AttackSfx, Is.Not.Null,
                        $"{path}: weapon '{definition.ItemId}' requires attack SFX.");
                }
            }
        }

        [Test]
        public void RebuildIndex_RefreshesItemLookupAfterAuthoringChanges()
        {
            var database = ScriptableObject.CreateInstance<ItemDatabase>();
            try
            {
                using var serialized = new SerializedObject(database);
                SerializedProperty items = serialized.FindProperty("items");
                items.arraySize = 1;
                SerializedProperty itemId = items.GetArrayElementAtIndex(0).FindPropertyRelative("itemId");
                itemId.intValue = (int)ItemId.GreatSword;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(database.GetRequired(ItemId.GreatSword), Is.SameAs(database.Items[0]));

                itemId.intValue = (int)ItemId.WoodenShield;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                database.RebuildIndex();

                Assert.That(database.GetRequired(ItemId.WoodenShield), Is.SameAs(database.Items[0]));
                Assert.Throws<KeyNotFoundException>(() => database.GetRequired(ItemId.GreatSword));
            }
            finally
            {
                Object.DestroyImmediate(database);
            }
        }
    }
}
#endif
