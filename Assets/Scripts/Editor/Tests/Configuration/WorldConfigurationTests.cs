#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Enemy;
using SoulsLike.Entities.Ladder;
using SoulsLike.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.Editor.Tests.Configuration
{
    public sealed class WorldConfigurationTests
    {
        [TestCase(typeof(LadderView))]
        [TestCase(typeof(GroundItem))]
        [TestCase(typeof(EnemySpawner))]
        public void WorldPrefabs_HaveRequiredConfiguration(Type componentType)
        {
            int checkedCount = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Models" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var saveIdentifiers = new HashSet<string>();
                foreach (Component component in prefab.GetComponentsInChildren(componentType, true))
                {
                    AssertConfiguration(component, path, saveIdentifiers);
                    checkedCount++;
                }
            }
            Assert.That(checkedCount, Is.GreaterThan(0), $"No {componentType.Name} prefab components were checked.");
        }

        [TestCase(typeof(LadderView), "Assets/Scripts/Entities/Ladder/LadderView.cs")]
        [TestCase(typeof(GroundItem), "Assets/Scripts/Items/GroundItem.cs")]
        [TestCase(typeof(EnemySpawner), "Assets/Scripts/Entities/Enemy/EnemySpawner.cs")]
        public void AuthoredScenes_HaveRequiredConfiguration(Type componentType, string scriptPath)
        {
            var saveIdentifiers = new HashSet<string>();
            int checkedCount = 0;
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes", "Assets/Sandbox/Scenes" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.GetDependencies(path).Contains(scriptPath))
                {
                    continue;
                }

                Scene preview = EditorSceneManager.OpenPreviewScene(path);
                try
                {
                    foreach (GameObject root in preview.GetRootGameObjects())
                    {
                        foreach (Component component in root.GetComponentsInChildren(componentType, true))
                        {
                            AssertConfiguration(component, path, saveIdentifiers);
                            checkedCount++;
                        }
                    }
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(preview);
                }
            }
            Assert.That(checkedCount, Is.GreaterThan(0), $"No authored {componentType.Name} scene components were checked.");
        }

        [Test]
        public void DefaultLocation_ExcludesFourthMainEnemySpawner()
        {
            const string SCENE_PATH = "Assets/Scenes/DefaultLocation/DefaultLocation.unity";
            Scene preview = EditorSceneManager.OpenPreviewScene(SCENE_PATH);
            try
            {
                EnemySpawner[] mainSpawners = preview.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<EnemySpawner>(true))
                    .Where(spawner => spawner.EnemyId == EnemyId.ErikaMelee)
                    .ToArray();

                Assert.That(mainSpawners.Length, Is.EqualTo(4));
                Assert.That(mainSpawners.Count(spawner => spawner.isActiveAndEnabled), Is.EqualTo(3));
                Assert.That(mainSpawners.Single(spawner => spawner.RandomSeedOffset == 3).isActiveAndEnabled, Is.False);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        [Test]
        public void ElevatorDemo_KeepsMainEncounterEmpty()
        {
            const string SCENE_PATH = "Assets/Sandbox/Scenes/ElevatorDemo/ElevatorDemo.unity";
            Scene preview = EditorSceneManager.OpenPreviewScene(SCENE_PATH);
            try
            {
                EnemySpawnGroup group = preview.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<EnemySpawnGroup>(true))
                    .Single();
                Assert.That(group.SpawnOnStart, Is.False);
                Assert.That(group.GetComponentsInChildren<EnemySpawner>(true).Length, Is.EqualTo(4));
                Assert.That(group.GetComponentsInChildren<EnemySpawner>(true)
                    .Any(spawner => spawner.isActiveAndEnabled), Is.False);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void AssertConfiguration(Component component, string path, HashSet<string> saveIdentifiers)
        {
            string context = $"{path}: '{component.name}'";
            if (component is EnemySpawner spawn)
            {
                Assert.That((int)spawn.EnemyId, Is.Not.Zero, $"{context} requires an enemy ID.");
                return;
            }

            Assert.That(component.GetComponent<ViewEntity>(), Is.Not.Null, $"{context} requires a root ViewEntity.");
            if (component is LadderView ladder && ladder.StartsLocked)
            {
                Assert.That(string.IsNullOrWhiteSpace(ladder.SaveIdentifier), Is.False,
                    $"{context} requires a stable save identifier.");
                Assert.That(saveIdentifiers.Add(ladder.SaveIdentifier), Is.True,
                    $"{context} duplicates locked ladder save identifier '{ladder.SaveIdentifier}'.");
            }
        }
    }
}
#endif
