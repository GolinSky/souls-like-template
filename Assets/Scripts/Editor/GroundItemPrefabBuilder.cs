using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Items;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor
{
    public static class GroundItemPrefabBuilder
    {
        private const string SHADER_PATH = "Assets/Art/Shaders/GroundItemAdditive.shader";
        private const string MATERIAL_FOLDER = "Assets/Art/Materials/GroundItems";
        private const string WHITE_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemWhite.mat";
        private const string WHITE_GLOW_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemWhiteGlow.mat";
        private const string BLUE_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemBlue.mat";
        private const string BLUE_GLOW_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemBlueGlow.mat";
        private const string GOLD_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemGold.mat";
        private const string GOLD_GLOW_MATERIAL_PATH = MATERIAL_FOLDER + "/GroundItemGroundGlow.mat";
        private const string PREFAB_PATH = "Assets/Prefabs/Models/Item/GroundItem.prefab";
        private const string LEGACY_PREFAB_PATH = "Assets/Prefabs/Models/Item/Sphere.prefab";

        [MenuItem("Tools/SoulsLike/Build Ground Item")]
        public static void Build()
        {
            EnsureFolder("Assets/Art", "Materials");
            EnsureFolder("Assets/Art/Materials", "GroundItems");

            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(SHADER_PATH);
            if (shader == null)
            {
                Debug.LogError($"GroundItemAdditive shader not found at {SHADER_PATH}");
                return;
            }

            Material whiteMaterial = CreateMaterial(
                WHITE_MATERIAL_PATH,
                shader,
                new Color(1.4f, 1.5f, 1.65f, 0.95f),
                5.0f,
                0f,
                0.008f);

            Material whiteGlowMaterial = CreateMaterial(
                WHITE_GLOW_MATERIAL_PATH,
                shader,
                new Color(1.2f, 1.3f, 1.45f, 0.55f),
                2.5f,
                1f,
                0f);

            CreateMaterial(
                BLUE_MATERIAL_PATH,
                shader,
                new Color(0.4f, 0.8f, 1.8f, 0.95f),
                5.0f,
                0f,
                0.008f);

            CreateMaterial(
                BLUE_GLOW_MATERIAL_PATH,
                shader,
                new Color(0.35f, 0.65f, 1.5f, 0.55f),
                2.5f,
                1f,
                0f);

            CreateMaterial(
                GOLD_MATERIAL_PATH,
                shader,
                new Color(1.35f, 0.72f, 0.14f, 0.9f),
                5.0f,
                0f,
                0.008f);

            CreateMaterial(
                GOLD_GLOW_MATERIAL_PATH,
                shader,
                new Color(1.1f, 0.55f, 0.08f, 0.55f),
                2.5f,
                1f,
                0f);

            BuildGroundItemPrefab(whiteMaterial, whiteGlowMaterial);
            BuildLegacyPrefabVariant();
            AssetDatabase.SaveAssets();
            Debug.Log("GroundItem and Sphere prefabs built successfully.");
        }

        private static void BuildLegacyPrefabVariant()
        {
            GameObject groundItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(groundItemPrefab);
            instance.name = "Sphere";
            PrefabUtility.SaveAsPrefabAsset(instance, LEGACY_PREFAB_PATH);
            Object.DestroyImmediate(instance);
        }

        private static void BuildGroundItemPrefab(
            Material mainMaterial,
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

            // 1. Ground contact glow: subtle circular pool directly on floor
            MeshRenderer groundGlow = CreateGroundGlow(visualRoot, glowMaterial);
            renderers.Add(groundGlow);

            // 2. Core glowing point: small intense orb sitting on the ground
            MeshRenderer corePoint = CreateCorePoint(visualRoot, mainMaterial);
            renderers.Add(corePoint);

            // 3. Core soft halo: camera-facing soft billboard glow around the core
            MeshRenderer coreGlow = CreateCoreGlow(visualRoot, glowMaterial);
            renderers.Add(coreGlow);

            // 4. Vertical light beam: thin vertical beam ~0.75m rising from ground, brightest at base
            LineRenderer verticalBeam = CreateVerticalBeam(visualRoot, mainMaterial);
            renderers.Add(verticalBeam);

            // 5. Subtle vertical wisp: gentle wavy wisp rising alongside beam
            LineRenderer verticalWisp = CreateVerticalWisp(visualRoot, mainMaterial);
            renderers.Add(verticalWisp);

            // 6. Ambient sparkles: a few tiny particles shimmering around the vertical beam
            ParticleSystem ambientSparkles = CreateAmbientSparkles(visualRoot, glowMaterial);
            renderers.Add(ambientSparkles.GetComponent<ParticleSystemRenderer>());

            // 7. Pickup flash: clean burst played on collection
            ParticleSystem pickupFlash = CreatePickupFlash(visualRoot, glowMaterial);
            renderers.Add(pickupFlash.GetComponent<ParticleSystemRenderer>());

            ConfigureGroundItem(groundItem, collider, anchor, vfx);
            ConfigureVfx(vfx, ambientSparkles, pickupFlash, renderers);

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
            glow.transform.localScale = new Vector3(0.32f, 0.32f, 1f);

            MeshRenderer renderer = glow.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static MeshRenderer CreateCorePoint(
            Transform parent,
            Material material)
        {
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "CorePoint";
            Object.DestroyImmediate(core.GetComponent<Collider>());
            core.transform.SetParent(parent, false);
            core.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            core.transform.localScale = new Vector3(0.065f, 0.065f, 0.065f);

            MeshRenderer renderer = core.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static MeshRenderer CreateCoreGlow(
            Transform parent,
            Material material)
        {
            GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            halo.name = "CoreGlow";
            Object.DestroyImmediate(halo.GetComponent<Collider>());
            halo.transform.SetParent(parent, false);
            halo.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            halo.transform.localRotation = Quaternion.identity;
            halo.transform.localScale = new Vector3(0.14f, 0.14f, 1f);

            MeshRenderer renderer = halo.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static LineRenderer CreateVerticalBeam(
            Transform parent,
            Material material)
        {
            Transform beamTransform = CreateChild(parent, "VerticalBeam");
            LineRenderer beam = beamTransform.gameObject.AddComponent<LineRenderer>();
            beam.useWorldSpace = false;
            beam.alignment = LineAlignment.View;
            beam.textureMode = LineTextureMode.Stretch;
            beam.sharedMaterial = material;
            beam.positionCount = 2;
            beam.SetPositions(new[]
            {
                new Vector3(0f, 0.02f, 0f),
                new Vector3(0f, 0.75f, 0f)
            });
            beam.startWidth = 0.035f;
            beam.endWidth = 0.008f;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1.0f, 0f),
                    new GradientAlphaKey(0.75f, 0.35f),
                    new GradientAlphaKey(0.35f, 0.7f),
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            beam.colorGradient = gradient;

            beam.numCapVertices = 3;
            beam.numCornerVertices = 3;
            beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            beam.receiveShadows = false;
            return beam;
        }

        private static LineRenderer CreateVerticalWisp(
            Transform parent,
            Material material)
        {
            Transform wispTransform = CreateChild(parent, "VerticalWisp");
            LineRenderer wisp = wispTransform.gameObject.AddComponent<LineRenderer>();
            wisp.useWorldSpace = false;
            wisp.alignment = LineAlignment.View;
            wisp.textureMode = LineTextureMode.Stretch;
            wisp.sharedMaterial = material;
            wisp.positionCount = 4;
            wisp.SetPositions(new[]
            {
                new Vector3(0f, 0.02f, 0f),
                new Vector3(0.006f, 0.22f, 0.002f),
                new Vector3(-0.005f, 0.44f, -0.002f),
                new Vector3(0.002f, 0.68f, 0f)
            });
            wisp.startWidth = 0.050f;
            wisp.endWidth = 0.015f;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.85f, 0.92f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.65f, 0f),
                    new GradientAlphaKey(0.45f, 0.4f),
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            wisp.colorGradient = gradient;

            wisp.numCapVertices = 3;
            wisp.numCornerVertices = 3;
            wisp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            wisp.receiveShadows = false;
            return wisp;
        }

        private static ParticleSystem CreateAmbientSparkles(
            Transform parent,
            Material material)
        {
            Transform sparklesTransform = CreateChild(parent, "AmbientSparkles");
            var particleSystem = sparklesTransform.gameObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.050f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 1f, 1f, 0.95f),
                new Color(0.85f, 0.92f, 1f, 0.8f));
            main.maxParticles = 10;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 5f;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.12f, 0.55f, 0.12f);
            shape.position = new Vector3(0f, 0.32f, 0f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var colorGradient = new Gradient();
            colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.9f, 0.95f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1.0f, 0.2f),
                    new GradientAlphaKey(0.9f, 0.7f),
                    new GradientAlphaKey(0f, 1.0f)
                });
            colorOverLifetime.color = colorGradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.2f);
            sizeCurve.AddKey(0.45f, 1.0f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

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
            main.duration = 0.25f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = new Color(1.2f, 1.3f, 1.5f, 1f);
            main.maxParticles = 16;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;
            shape.position = new Vector3(0f, 0.1f, 0f);

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
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
            serialized.FindProperty("rewardType").enumValueIndex = (int)GroundItemRewardType.Item;
            serialized.FindProperty("itemId").enumValueIndex = (int)ItemId.GoldenRuneSmall;
            serialized.FindProperty("quantity").intValue = 1;
            serialized.FindProperty("currencyAmount").intValue = 200;
            serialized.FindProperty("saveIdentifier").stringValue = "ground-item-golden-rune-small";
            serialized.FindProperty("interactionCollider").objectReferenceValue = collider;
            serialized.FindProperty("interactionAnchor").objectReferenceValue = anchor;
            serialized.FindProperty("pickupVfx").objectReferenceValue = vfx;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(groundItem);
        }

        private static void ConfigureVfx(
            GroundItemVfx vfx,
            ParticleSystem ambientSparkles,
            ParticleSystem pickupFlash,
            IReadOnlyList<Renderer> renderers)
        {
            var serialized = new SerializedObject(vfx);
            SerializedProperty particles = serialized.FindProperty("ambientParticles");
            particles.arraySize = 1;
            particles.GetArrayElementAtIndex(0).objectReferenceValue = ambientSparkles;

            serialized.FindProperty("pickupFlash").objectReferenceValue = pickupFlash;
            serialized.FindProperty("dissolveDuration").floatValue = 0.45f;

            SerializedProperty rendererProperty = serialized.FindProperty("renderers");
            rendererProperty.arraySize = renderers.Count;
            for (int index = 0; index < renderers.Count; index++)
            {
                rendererProperty.GetArrayElementAtIndex(index).objectReferenceValue = renderers[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vfx);
        }

        private static Material CreateMaterial(
            string path,
            Shader shader,
            Color tint,
            float intensity,
            float radial,
            float wobble)
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
            material.SetFloat("_PulseSpeed", 3.14f);
            material.SetFloat("_Wobble", wobble);
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
