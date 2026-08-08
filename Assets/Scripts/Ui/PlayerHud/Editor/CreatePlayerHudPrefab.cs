#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MPUIKIT;
using SoulsLike.Ui.PlayerHud;
using System.IO;

namespace SoulsLike.Ui.PlayerHud.Editor
{
    public static class CreatePlayerHudPrefab
    {
        [MenuItem("Tools/SoulsLike/Generate Player HUD Prefab")]
        public static void GeneratePrefab()
        {
            string folderPath = "Assets/Prefabs/Ui/PlayerHud";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string prefabPath = $"{folderPath}/PlayerHudUi.prefab";

            // Root GameObject
            GameObject root = new GameObject("PlayerHudUi");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(40f, -40f);
            rootRect.sizeDelta = new Vector2(400f, 150f);

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
            PlayerHudUi playerHudUi = root.AddComponent<PlayerHudUi>();

            // Container for bars
            GameObject barsContainer = new GameObject("BarsContainer");
            barsContainer.transform.SetParent(root.transform, false);
            RectTransform barsContainerRect = barsContainer.AddComponent<RectTransform>();
            barsContainerRect.anchorMin = Vector2.zero;
            barsContainerRect.anchorMax = Vector2.one;
            barsContainerRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = barsContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create individual bars
            var hpRefs = CreateBar(barsContainer.transform, "HpBar", 200f, 18f, new Color(0.784f, 0.137f, 0.137f), new Color(1f, 0.647f, 0f));
            var fpRefs = CreateBar(barsContainer.transform, "FpBar", 150f, 14f, new Color(0.118f, 0.384f, 0.784f), new Color(0.529f, 0.808f, 0.98f));
            var staminaRefs = CreateBar(barsContainer.transform, "StaminaBar", 180f, 14f, new Color(0.18f, 0.545f, 0.341f), new Color(0.565f, 0.933f, 0.565f));

            SerializedObject so = new SerializedObject(playerHudUi);
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            BindStatBar(so.FindProperty("hpBar"), hpRefs.container, hpRefs.primary, hpRefs.buffer);
            BindStatBar(so.FindProperty("fpBar"), fpRefs.container, fpRefs.primary, fpRefs.buffer);
            BindStatBar(so.FindProperty("staminaBar"), staminaRefs.container, staminaRefs.primary, staminaRefs.buffer);
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"Successfully generated PlayerHudUi prefab at {prefabPath}");
        }

        private static void BindStatBar(SerializedProperty barProp, RectTransform container, MPImage primary, MPImage buffer)
        {
            if (barProp == null) return;
            barProp.FindPropertyRelative("container").objectReferenceValue = container;
            barProp.FindPropertyRelative("primaryBar").objectReferenceValue = primary;
            barProp.FindPropertyRelative("trailingBufferBar").objectReferenceValue = buffer;
        }

        private struct BarRefs
        {
            public RectTransform container;
            public MPImage primary;
            public MPImage buffer;
        }

        private static BarRefs CreateBar(Transform parent, string barName, float baseWidth, float height, Color primaryColor, Color bufferColor)
        {
            GameObject container = new GameObject(barName);
            container.transform.SetParent(parent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(baseWidth, height);

            // Background / Track
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(container.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            MPImage bgImage = bgObj.AddComponent<MPImage>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);
            bgImage.DrawShape = DrawShape.Rectangle;
            bgImage.Rectangle = new Rectangle { CornerRadius = new Vector4(4f, 4f, 4f, 4f) };

            // Trailing Buffer Bar
            GameObject bufferObj = new GameObject("TrailingBufferBar");
            bufferObj.transform.SetParent(container.transform, false);
            RectTransform bufferRect = bufferObj.AddComponent<RectTransform>();
            bufferRect.anchorMin = Vector2.zero;
            bufferRect.anchorMax = Vector2.one;
            bufferRect.sizeDelta = Vector2.zero;
            MPImage bufferImage = bufferObj.AddComponent<MPImage>();
            bufferImage.color = bufferColor;
            bufferImage.type = Image.Type.Filled;
            bufferImage.fillMethod = Image.FillMethod.Horizontal;
            bufferImage.fillOrigin = 0;
            bufferImage.fillAmount = 1f;
            bufferImage.DrawShape = DrawShape.Rectangle;
            bufferImage.Rectangle = new Rectangle { CornerRadius = new Vector4(4f, 4f, 4f, 4f) };

            // Primary Bar
            GameObject primaryObj = new GameObject("PrimaryBar");
            primaryObj.transform.SetParent(container.transform, false);
            RectTransform primaryRect = primaryObj.AddComponent<RectTransform>();
            primaryRect.anchorMin = Vector2.zero;
            primaryRect.anchorMax = Vector2.one;
            primaryRect.sizeDelta = Vector2.zero;
            MPImage primaryImage = primaryObj.AddComponent<MPImage>();
            primaryImage.color = primaryColor;
            primaryImage.type = Image.Type.Filled;
            primaryImage.fillMethod = Image.FillMethod.Horizontal;
            primaryImage.fillOrigin = 0;
            primaryImage.fillAmount = 1f;
            primaryImage.DrawShape = DrawShape.Rectangle;
            primaryImage.Rectangle = new Rectangle { CornerRadius = new Vector4(4f, 4f, 4f, 4f) };

            return new BarRefs
            {
                container = containerRect,
                primary = primaryImage,
                buffer = bufferImage
            };
        }
    }
}
#endif
