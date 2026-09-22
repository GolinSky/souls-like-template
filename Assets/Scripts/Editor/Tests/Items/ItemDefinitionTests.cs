#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Items;
using UnityEditor;

namespace SoulsLike.Editor.Tests.Items
{
    public sealed class ItemDefinitionTests
    {
        [Test]
        public void ItemDatabaseAssets_DefinitionsHaveRequiredFields()
        {
            string[] databaseGuids = AssetDatabase.FindAssets("t:ItemDatabase", new[] { "Assets" });
            Assert.That(databaseGuids, Is.Not.Empty, "No ItemDatabase assets were found.");

            foreach (string guid in databaseGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
                Assert.That(database, Is.Not.Null, $"Could not load ItemDatabase at '{path}'.");
                Assert.That(database.Items, Is.Not.Empty, $"Item database '{path}' contains no definitions.");

                foreach (ItemDefinition definition in database.Items)
                {
                    Assert.That(definition, Is.Not.Null, $"Item database '{path}' contains a null definition.");
                    Assert.That(definition.ItemId, Is.Not.EqualTo(ItemId.None),
                        $"Item definition in '{path}' requires a non-None ItemId.");
                    Assert.That(string.IsNullOrWhiteSpace(definition.DisplayName), Is.False,
                        $"Item definition '{definition.ItemId}' in '{path}' requires a display name.");
                    Assert.That(definition.MaxStack, Is.GreaterThanOrEqualTo(1),
                        $"Item definition '{definition.ItemId}' in '{path}' requires MaxStack >= 1.");
                }
            }
        }
    }
}
#endif
