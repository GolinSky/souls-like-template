using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Items;
using SoulsLike.Ui.PlayerHud;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Editor
{
    public static class GroundItemPrefabBuilder
    {
        private const string SHADER_PATH = "Assets/Shaders/GroundItemAdditive.shader";
        private const string MATERIAL_FOLDER = "Assets/Art/Materials/GroundItems";
        private const string STRAND_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemGold.mat";
        private const string GLOW_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemGroundGlow.mat";
        private const string PREFAB_PATH = "Assets/Prefabs/Item/GroundItem.prefab";
        private const string LEGACY_PREFAB_PATH = "Assets/Prefabs/Item/Sphere.prefab";
        private const string HUD_PREFAB_PATH = "Assets/Prefabs/Ui/PlayerHud/PlayerHudUi.prefab";

        [MenuItem("Tools/SoulsLike/Build Ground Item")]
        public static void Build()
        {
            EnsureFolder("Assets/Art", "Materials");
            EnsureFolder("Assets/Art/Materials", "GroundItems");

            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(SHADER_PATH);
            Material strandMaterial = CreateMaterial(
                STRAND_MATERIAL_PATH,
                shader,
                new Color(1.35f, 0.72f, 0.14f, 0.9f),
                5.5f,
                0f);
            Material glowMaterial = CreateMaterial(
                GLOW_MATERIAL_PATH,
                shader,
                new Color(1.1f, 0.55f, 0.08f, 0.55f),
                3.5f,
                1f);

            BuildGroundItemPrefab(strandMaterial, glowMaterial);
            BuildLegacyPrefabVariant();
            AddAcquisitionPanelToHud();
            AssetDatabase.SaveAssets();
        }

        private static void BuildLegacyPrefabVariant()
        {
            GameObject groundItemPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            GameObject instance =
                (GameObject)PrefabUtility.InstantiatePrefab(groundItemPrefab);
            instance.name = "Sphere";
            PrefabUtility.SaveAsPrefabAsset(instance, LEGACY_PREFAB_PATH);
            Object.DestroyImmediate(instance);
        }

        private static void BuildGroundItemPrefab(
            Material strandMaterial,
            Material glowMaterial)
        {
            var root = new GameObject("GroundItem");
            var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.55f, 0f);
            collider.radius = 0.7f;

            GroundItem groundItem = root.AddComponent<GroundItem>();
            root.AddComponent<ViewEntity>();
            Transform anchor = CreateChild(root.transform, "InteractionAnchor");
            anchor.localPosition = new Vector3(0f, 0.45f, 0f);
            Transform visualRoot = CreateChild(root.transform, "VFX");
            GroundItemVfx vfx = visualRoot.gameObject.AddComponent<GroundItemVfx>();

            var renderers = new List<Renderer>();
            MeshRenderer groundGlow = CreateGroundGlow(visualRoot, glowMaterial);
            renderers.Add(groundGlow);

            Vector3[][] strandPoints =
            {
                new[] { new Vector3(-0.22f, 0.02f, 0.04f), new Vector3(-0.15f, 0.27f, 0.01f), new Vector3(-0.2f, 0.58f, 0.02f), new Vector3(-0.12f, 0.92f, 0f) },
                new[] { new Vector3(-0.08f, 0.01f, -0.1f), new Vector3(-0.03f, 0.34f, -0.06f), new Vector3(-0.07f, 0.72f, -0.03f), new Vector3(0f, 1.2f, 0f) },
                new[] { new Vector3(0.05f, 0.01f, 0.08f), new Vector3(0.09f, 0.29f, 0.04f), new Vector3(0.03f, 0.64f, 0.03f), new Vector3(0.12f, 1.03f, 0f) },
                new[] { new Vector3(0.19f, 0.01f, -0.03f), new Vector3(0.16f, 0.22f, 0f), new Vector3(0.23f, 0.46f, 0.02f), new Vector3(0.18f, 0.77f, 0f) },
                new[] { new Vector3(-0.01f, 0.02f, 0.2f), new Vector3(0.02f, 0.18f, 0.14f), new Vector3(-0.04f, 0.42f, 0.09f), new Vector3(0.03f, 0.68f, 0.03f) }
            };

            for (int index = 0; index < strandPoints.Length; index++)
            {
                LineRenderer strand = CreateStrand(
                    visualRoot,
                    $"SpectralStrand_{index + 1}",
                    strandMaterial,
                    strandPoints[index],
                    0.018f + index * 0.003f);
                renderers.Add(strand);
            }

            ParticleSystem upwardMotes = CreateMotes(
                visualRoot,
                "UpwardMotes",
                strandMaterial,
                false);
            ParticleSystem orbitMotes = CreateMotes(
                visualRoot,
                "OrbitMotes",
                strandMaterial,
                true);
            ParticleSystem pickupFlash = CreatePickupFlash(
                visualRoot,
                strandMaterial);
            renderers.Add(upwardMotes.GetComponent<ParticleSystemRenderer>());
            renderers.Add(orbitMotes.GetComponent<ParticleSystemRenderer>());
            renderers.Add(pickupFlash.GetComponent<ParticleSystemRenderer>());

            ConfigureGroundItem(
                groundItem,
                collider,
                anchor,
                vfx);
            ConfigureVfx(
                vfx,
                visualRoot,
                upwardMotes,
                orbitMotes,
                pickupFlash,
                renderers);

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);
        }

        private static MeshRenderer CreateGroundGlow(
            Transform parent,
            Material material)
        {
            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "GroundGlow";
            Object.DestroyImmediate(glow.GetComponent<Collider>());
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            glow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            glow.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            MeshRenderer renderer = glow.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static LineRenderer CreateStrand(
            Transform parent,
            string objectName,
            Material material,
            Vector3[] points,
            float width)
        {
            Transform strandTransform = CreateChild(parent, objectName);
            LineRenderer strand = strandTransform.gameObject.AddComponent<LineRenderer>();
            strand.useWorldSpace = false;
            strand.alignment = LineAlignment.View;
            strand.textureMode = LineTextureMode.Stretch;
            strand.sharedMaterial = material;
            strand.positionCount = points.Length;
            strand.SetPositions(points);
            strand.startWidth = width;
            strand.endWidth = width * 0.35f;
            strand.numCapVertices = 3;
            strand.numCornerVertices = 3;
            strand.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            strand.receiveShadows = false;
            return strand;
        }

        private static ParticleSystem CreateMotes(
            Transform parent,
            string objectName,
            Material material,
            bool orbit)
        {
            Transform particleTransform = CreateChild(parent, objectName);
            var particleSystem = particleTransform.gameObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = orbit
                ? new ParticleSystem.MinMaxCurve(1f, 1.8f)
                : new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
            main.startSpeed = orbit
                ? new ParticleSystem.MinMaxCurve(0.01f, 0.04f)
                : new ParticleSystem.MinMaxCurve(0.05f, 0.22f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.065f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.55f, 0.08f, 0.45f),
                new Color(1f, 0.92f, 0.35f, 1f));
            main.maxParticles = orbit ? 24 : 50;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = orbit ? 11f : 22f;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = orbit ? 0.42f : 0.32f;
            shape.radiusThickness = orbit ? 1f : 0.5f;

            if (orbit)
            {
                ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
                velocity.enabled = true;
                velocity.orbitalY = 1.2f;
                velocity.radial = -0.08f;
            }

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particleSystem;
        }

        private static ParticleSystem CreatePickupFlash(
            Transform parent,
            Material material)
        {
            Transform flashTransform = CreateChild(parent, "PickupFlash");
            var particleSystem = flashTransform.gameObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = new Color(1f, 0.9f, 0.35f, 1f);
            main.maxParticles = 20;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.12f;

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return particleSystem;
        }

        private static void ConfigureGroundItem(
            GroundItem groundItem,
            Collider collider,
            Transform anchor,
            GroundItemVfx vfx)
        {
            var serialized = new SerializedObject(groundItem);
            serialized.FindProperty("rewardType").enumValueIndex =
                (int)GroundItemRewardType.Item;
            serialized.FindProperty("itemId").enumValueIndex =
                (int)ItemId.GoldenRuneSmall;
            serialized.FindProperty("quantity").intValue = 1;
            serialized.FindProperty("currencyAmount").intValue = 200;
            serialized.FindProperty("saveIdentifier").stringValue =
                "ground-item-golden-rune-small";
            serialized.FindProperty("interactionCollider").objectReferenceValue = collider;
            serialized.FindProperty("interactionAnchor").objectReferenceValue = anchor;
            serialized.FindProperty("pickupVfx").objectReferenceValue = vfx;
            serialized.FindProperty("priority").intValue = 100;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(groundItem);
        }

        private static void ConfigureVfx(
            GroundItemVfx vfx,
            Transform visualRoot,
            ParticleSystem upwardMotes,
            ParticleSystem orbitMotes,
            ParticleSystem pickupFlash,
            IReadOnlyList<Renderer> renderers)
        {
            var serialized = new SerializedObject(vfx);
            serialized.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            SerializedProperty particles = serialized.FindProperty("ambientParticles");
            particles.arraySize = 2;
            particles.GetArrayElementAtIndex(0).objectReferenceValue = upwardMotes;
            particles.GetArrayElementAtIndex(1).objectReferenceValue = orbitMotes;
            serialized.FindProperty("pickupFlash").objectReferenceValue = pickupFlash;
            SerializedProperty rendererProperty = serialized.FindProperty("renderers");
            rendererProperty.arraySize = renderers.Count;
            for (int index = 0; index < renderers.Count; index++)
            {
                rendererProperty.GetArrayElementAtIndex(index).objectReferenceValue =
                    renderers[index];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vfx);
        }

        private static void AddAcquisitionPanelToHud()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HUD_PREFAB_PATH);
            try
            {
                Transform existing = root.transform.Find("ItemAcquisitionPanel");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                GameObject panelObject = CreateUiObject(
                    "ItemAcquisitionPanel",
                    root.transform,
                    typeof(CanvasGroup),
                    typeof(Image),
                    typeof(ItemAcquisitionPanel));
                RectTransform panelRect = panelObject.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.anchoredPosition = new Vector2(0f, 135f);
                panelRect.sizeDelta = new Vector2(540f, 92f);
                Image background = panelObject.GetComponent<Image>();
                background.color = new Color(0.025f, 0.02f, 0.015f, 0.82f);
                background.raycastTarget = false;

                Image icon = CreateImage(panelObject.transform, "Icon");
                RectTransform iconRect = icon.rectTransform;
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(18f, 0f);
                iconRect.sizeDelta = new Vector2(60f, 60f);
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                TextMeshProUGUI itemName = CreateText(
                    panelObject.transform,
                    "ItemName",
                    28f,
                    TextAlignmentOptions.MidlineLeft,
                    new Color(0.95f, 0.88f, 0.7f, 1f));
                RectTransform nameRect = itemName.rectTransform;
                nameRect.anchorMin = new Vector2(0f, 0f);
                nameRect.anchorMax = new Vector2(1f, 1f);
                nameRect.offsetMin = new Vector2(96f, 8f);
                nameRect.offsetMax = new Vector2(-100f, -8f);

                TextMeshProUGUI quantity = CreateText(
                    panelObject.transform,
                    "Quantity",
                    26f,
                    TextAlignmentOptions.MidlineRight,
                    new Color(1f, 0.75f, 0.25f, 1f));
                RectTransform quantityRect = quantity.rectTransform;
                quantityRect.anchorMin = new Vector2(1f, 0f);
                quantityRect.anchorMax = new Vector2(1f, 1f);
                quantityRect.pivot = new Vector2(1f, 0.5f);
                quantityRect.anchoredPosition = new Vector2(-22f, 0f);
                quantityRect.sizeDelta = new Vector2(90f, 0f);

                ItemAcquisitionPanel panel =
                    panelObject.GetComponent<ItemAcquisitionPanel>();
                var panelSerialized = new SerializedObject(panel);
                panelSerialized.FindProperty("canvasGroup").objectReferenceValue =
                    panelObject.GetComponent<CanvasGroup>();
                panelSerialized.FindProperty("icon").objectReferenceValue = icon;
                panelSerialized.FindProperty("itemNameText").objectReferenceValue = itemName;
                panelSerialized.FindProperty("quantityText").objectReferenceValue = quantity;
                panelSerialized.ApplyModifiedPropertiesWithoutUndo();

                PlayerHudUi hud = root.GetComponent<PlayerHudUi>();
                var hudSerialized = new SerializedObject(hud);
                hudSerialized.FindProperty("acquisitionPanel").objectReferenceValue = panel;
                hudSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
                EditorUtility.SetDirty(panel);

                PrefabUtility.SaveAsPrefabAsset(root, HUD_PREFAB_PATH);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            params System.Type[] componentTypes)
        {
            var types = new List<System.Type> { typeof(RectTransform) };
            types.AddRange(componentTypes);
            var result = new GameObject(objectName, types.ToArray());
            result.transform.SetParent(parent, false);
            return result;
        }

        private static Image CreateImage(Transform parent, string objectName)
        {
            return CreateUiObject(
                    objectName,
                    parent,
                    typeof(CanvasRenderer),
                    typeof(Image))
                .GetComponent<Image>();
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string objectName,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color)
        {
            TextMeshProUGUI text = CreateUiObject(
                    objectName,
                    parent,
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI))
                .GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            return text;
        }

        private static Material CreateMaterial(
            string path,
            Shader shader,
            Color tint,
            float intensity,
            float radial)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_Tint", tint);
            material.SetFloat("_Intensity", intensity);
            material.SetFloat("_PulseSpeed", 6.28f);
            material.SetFloat("_Wobble", radial > 0f ? 0f : 0.018f);
            material.SetFloat("_Radial", radial);
            material.SetFloat("_Dissolve", 0f);
            material.renderQueue = 3100;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform CreateChild(Transform parent, string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
