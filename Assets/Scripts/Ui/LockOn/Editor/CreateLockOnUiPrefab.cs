#if UNITY_EDITOR
using MPUIKIT;
using SoulsLike.Ui.LockOn;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SoulsLike.Ui.LockOn.Editor
{
    public static class CreateLockOnUiPrefab
    {
        private const string PREFAB_FOLDER = "Assets/Prefabs/Ui/LockOn";
        private const string PREFAB_PATH = PREFAB_FOLDER + "/LockOnUi.prefab";
        private const string UI_GROUP_NAME = "Ui";

        [MenuItem("Tools/SoulsLike/Generate Lock On UI Prefab")]
        public static void GeneratePrefab()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs/Ui", "LockOn");
            }

            GameObject root = new GameObject("LockOnUi");
            root.layer = 5;

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(16f, 16f);

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            LockOnUi lockOnUi = root.AddComponent<LockOnUi>();

            GameObject reticleObject = new GameObject("Reticle");
            reticleObject.layer = 5;
            reticleObject.transform.SetParent(root.transform, false);

            RectTransform reticleRect = reticleObject.AddComponent<RectTransform>();
            reticleRect.anchorMin = Vector2.zero;
            reticleRect.anchorMax = Vector2.one;
            reticleRect.offsetMin = Vector2.zero;
            reticleRect.offsetMax = Vector2.zero;

            MPImage reticle = reticleObject.AddComponent<MPImage>();
            reticle.color = new Color(1f, 1f, 1f, 0.8f);
            reticle.DrawShape = DrawShape.Circle;
            reticle.Circle = new Circle { FitToRect = true };
            reticle.FalloffDistance = 0.5f;
            reticle.raycastTarget = false;

            SerializedObject serializedUi = new SerializedObject(lockOnUi);
            serializedUi.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedUi.FindProperty("reticle").objectReferenceValue = reticle;
            serializedUi.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            RegisterAddressablePrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Successfully generated LockOnUi prefab at {PREFAB_PATH}");
        }

        private static void RegisterAddressablePrefab()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings could not be found.");
                return;
            }

            AddressableAssetGroup uiGroup = settings.FindGroup(UI_GROUP_NAME);
            if (uiGroup == null)
            {
                Debug.LogError($"Addressable group '{UI_GROUP_NAME}' could not be found.");
                return;
            }

            string prefabGuid = AssetDatabase.AssetPathToGUID(PREFAB_PATH);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(prefabGuid, uiGroup);
            entry.address = nameof(LockOnUi);
            EditorUtility.SetDirty(settings);
        }
    }
}
#endif
