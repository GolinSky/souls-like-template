#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Entities.Elevator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.Editor.Tests.Elevator
{
    public sealed class ElevatorConfigurationTests
    {
        private const string ELEVATOR_SCRIPT_PATH = "Assets/Scripts/Entities/Elevator/ElevatorView.cs";

        [Test]
        public void SceneElevators_HaveUniqueSaveIdentifiersAndRequiredEntities()
        {
            var saveIdentifiers = new HashSet<string>();
            int elevatorCount = 0;
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene",
                new[] { "Assets/Scenes", "Assets/Sandbox/Scenes" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.GetDependencies(path).Contains(ELEVATOR_SCRIPT_PATH))
                {
                    continue;
                }

                Scene preview = EditorSceneManager.OpenPreviewScene(path);
                try
                {
                    foreach (GameObject root in preview.GetRootGameObjects())
                    {
                        foreach (ElevatorView elevator in root.GetComponentsInChildren<ElevatorView>(true))
                        {
                            AssertConfiguration(elevator, path, saveIdentifiers);
                            elevatorCount++;
                        }
                    }
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(preview);
                }
            }

            Assert.That(elevatorCount, Is.GreaterThan(0), "No authored scene elevators were checked.");
        }

        [Test]
        public void PrefabElevators_HaveSaveIdentifiersAndRequiredEntities()
        {
            int elevatorCount = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.GetDependencies(path).Contains(ELEVATOR_SCRIPT_PATH))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var saveIdentifiers = new HashSet<string>();
                foreach (ElevatorView elevator in prefab.GetComponentsInChildren<ElevatorView>(true))
                {
                    AssertConfiguration(elevator, path, saveIdentifiers);
                    elevatorCount++;
                }
            }

            Assert.That(elevatorCount, Is.GreaterThan(0), "No elevator prefabs were checked.");
        }

        private static void AssertConfiguration(
            ElevatorView elevator, string path, HashSet<string> saveIdentifiers)
        {
            Assert.That(elevator.ViewEntity, Is.Not.Null,
                $"{path}: elevator '{elevator.name}' requires a ViewEntity.");
            foreach (ElevatorEndpoint endpoint in elevator.Endpoints)
            {
                Assert.That(endpoint, Is.Not.Null, $"{path}: elevator '{elevator.name}' has a null endpoint.");
                Assert.That(endpoint.ViewEntity, Is.Not.Null,
                    $"{path}: endpoint '{endpoint.name}' requires a ViewEntity.");
            }

            if (elevator.StartsLocked)
            {
                Assert.That(string.IsNullOrWhiteSpace(elevator.SaveIdentifier), Is.False,
                    $"{path}: locked elevator '{elevator.name}' requires a stable save identifier.");
                Assert.That(saveIdentifiers.Add(elevator.SaveIdentifier), Is.True,
                    $"{path}: locked elevator '{elevator.name}' duplicates save identifier '{elevator.SaveIdentifier}'.");
            }
        }
    }
}
#endif
