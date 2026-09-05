#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Enemy;
using SoulsLike.Interactions;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Layer.Data;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor
{
    public static class LayerConfigurationValidator
    {
        private const string MENU_PATH = "Tools/SoulsLike/Validate Layer Configuration";

        [MenuItem(MENU_PATH)]
        public static bool Validate()
        {
            var report = new ValidationReport();

            ValidateTagManagerLayers(report);
            ValidateLayerDataAsset(report);
            ValidateCharacterPrefabs(report);
            ValidateEnemyPrefabs(report);
            ValidateInteractablePrefabs(report);

            report.LogSummary();
            return report.ErrorCount == 0;
        }

        private static void ValidateTagManagerLayers(ValidationReport report)
        {
            int previewLayer = LayerMask.NameToLayer("Preview");
            if (previewLayer != 10)
            {
                report.Error($"Unity layer 'Preview' is expected at index 10, but got index {previewLayer}.");
            }

            int interactionLayer = LayerMask.NameToLayer("Interaction");
            if (interactionLayer != 11)
            {
                report.Error($"Unity layer 'Interaction' is expected at index 11, but got index {interactionLayer}.");
            }
        }

        private static void ValidateLayerDataAsset(ValidationReport report)
        {
            LayerData layerData;
            try
            {
                layerData = LayerDataEditorProvider.LoadLayerData();
            }
            catch (Exception ex)
            {
                report.Error($"Failed to load LayerData asset: {ex.Message}");
                return;
            }

            IReadOnlyDictionary<LayerName, LayerMask> singleLayers = layerData.SingleLayers;
            if (singleLayers == null)
            {
                report.Error("LayerData.singleLayers is null.", layerData);
                return;
            }

            foreach (LayerName layerName in Enum.GetValues(typeof(LayerName)))
            {
                if (!singleLayers.TryGetValue(layerName, out LayerMask mask))
                {
                    report.Error($"LayerData is missing single-layer entry for '{layerName}'.", layerData);
                    continue;
                }

                uint bits = unchecked((uint)mask.value);
                if (bits == 0)
                {
                    report.Error($"Single-layer entry '{layerName}' has mask 0 (empty).", layerData);
                    continue;
                }

                if ((bits & (bits - 1)) != 0)
                {
                    report.Error($"Single-layer entry '{layerName}' has multiple bits set (0x{bits:X8}). Expected exactly one bit.", layerData);
                    continue;
                }

                int layerIndex = -1;
                for (int i = 0; i < 32; i++)
                {
                    if ((bits & (1u << i)) != 0)
                    {
                        layerIndex = i;
                        break;
                    }
                }

                string actualUnityLayerName = LayerMask.LayerToName(layerIndex);
                string expectedName = layerName.ToString();
                if (!string.Equals(actualUnityLayerName, expectedName, StringComparison.OrdinalIgnoreCase))
                {
                    report.Error($"Single-layer '{layerName}' maps to Unity layer {layerIndex} ('{actualUnityLayerName}'), expected '{expectedName}'.", layerData);
                }
            }

            IReadOnlyDictionary<LayerMaskName, LayerMask> sharedMasks = layerData.SharedMasks;
            if (sharedMasks == null)
            {
                report.Error("LayerData.sharedMasks is null.", layerData);
                return;
            }

            foreach (LayerMaskName maskName in Enum.GetValues(typeof(LayerMaskName)))
            {
                if (!sharedMasks.TryGetValue(maskName, out LayerMask mask))
                {
                    report.Error($"LayerData is missing shared mask entry for '{maskName}'.", layerData);
                    continue;
                }

                if (mask.value == 0)
                {
                    report.Error($"Shared mask entry '{maskName}' is zero.", layerData);
                }
            }
        }

        private static void ValidateCharacterPrefabs(ValidationReport report)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                if (prefab.TryGetComponent<Character>(out _))
                {
                    report.CharacterPrefabCount++;
                    if (prefab.layer != playerLayer)
                    {
                        report.Error($"Character prefab '{path}' root is on layer {prefab.layer} ('{LayerMask.LayerToName(prefab.layer)}'), expected 'Player' ({playerLayer}).", prefab);
                    }
                }
            }
        }

        private static void ValidateEnemyPrefabs(ValidationReport report)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                if (prefab.TryGetComponent<EnemyActor>(out _))
                {
                    report.EnemyPrefabCount++;
                    if (prefab.layer != enemyLayer)
                    {
                        report.Error($"Enemy prefab '{path}' root is on layer {prefab.layer} ('{LayerMask.LayerToName(prefab.layer)}'), expected 'Enemy' ({enemyLayer}).", prefab);
                    }
                }
            }
        }

        private static void ValidateInteractablePrefabs(ValidationReport report)
        {
            LayerMask probeMask;
            try
            {
                probeMask = LayerDataEditorProvider.GetMask(LayerMaskName.InteractionProbe);
            }
            catch
            {
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var interactable = prefab.GetComponentInChildren<IInteractable>(true);
                if (interactable == null) continue;

                report.InteractablePrefabCount++;
                Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
                foreach (Collider collider in colliders)
                {
                    int colliderLayer = collider.gameObject.layer;
                    int colliderBit = 1 << colliderLayer;
                    if ((probeMask.value & colliderBit) == 0)
                    {
                        report.Warning($"Interactable prefab '{path}' has collider '{collider.name}' on layer {colliderLayer} ('{LayerMask.LayerToName(colliderLayer)}') which is outside InteractionProbe mask (0x{probeMask.value:X8}).", collider);
                    }
                }
            }
        }

        private sealed class ValidationReport
        {
            public int CharacterPrefabCount { get; set; }
            public int EnemyPrefabCount { get; set; }
            public int InteractablePrefabCount { get; set; }
            public int ErrorCount { get; private set; }
            public int WarningCount { get; private set; }

            public void Error(string message, UnityEngine.Object context = null)
            {
                ErrorCount++;
                Debug.LogError($"[Layer Configuration] {message}", context);
            }

            public void Warning(string message, UnityEngine.Object context = null)
            {
                WarningCount++;
                Debug.LogWarning($"[Layer Configuration] {message}", context);
            }

            public void LogSummary()
            {
                string summary = $"[Layer Configuration] Validation complete: {CharacterPrefabCount} character prefabs, "
                    + $"{EnemyPrefabCount} enemy prefabs, {InteractablePrefabCount} interactable prefabs inspected. "
                    + $"{ErrorCount} errors, {WarningCount} warnings.";

                if (ErrorCount == 0)
                {
                    Debug.Log(summary);
                }
                else
                {
                    Debug.LogError(summary);
                }
            }
        }
    }
}
#endif
