using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.EditorTools
{
    public static class LocationBakeTool
    {
        public static readonly string LogFilePath = "Assets/Scenes/DefaultLocation/bake_progress.txt";

        public static readonly string[] AllScenes = new string[]
        {
            "Assets/Scenes/DefaultLocation/Blueprints.unity",
            "Assets/Scenes/DefaultLocation/CandleHolder.unity",
            "Assets/Scenes/DefaultLocation/Candles.unity",
            "Assets/Scenes/DefaultLocation/CastleSideBridges.unity",
            "Assets/Scenes/DefaultLocation/Chandeliers.unity",
            "Assets/Scenes/DefaultLocation/Grasses.unity",
            "Assets/Scenes/DefaultLocation/GroundTorches.unity",
            "Assets/Scenes/DefaultLocation/InteriorFloorBoxes.unity",
            "Assets/Scenes/DefaultLocation/MergedCylinders.unity",
            "Assets/Scenes/DefaultLocation/Other.unity",
            "Assets/Scenes/DefaultLocation/PillarSet2.unity",
            "Assets/Scenes/DefaultLocation/Railings.unity",
            "Assets/Scenes/DefaultLocation/Rocks.unity",
            "Assets/Scenes/DefaultLocation/Stairs.unity",
            "Assets/Scenes/DefaultLocation/StaticMeshActors.unity",
            "Assets/Scenes/DefaultLocation/Stones.unity",
            "Assets/Scenes/DefaultLocation/SupportPillars.unity",
            "Assets/Scenes/DefaultLocation/SupportTowers.unity",
            "Assets/Scenes/DefaultLocation/Torches.unity",
            "Assets/Scenes/DefaultLocation/WallGargoyles.unity",
            "Assets/Scenes/DefaultLocation/Walls.unity",
            "Assets/Scenes/DefaultLocation/WoodDeco.unity",
            "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity"
        };

        public static readonly string[] StructuralScenes = new string[]
        {
            "Assets/Scenes/DefaultLocation/Blueprints.unity",
            "Assets/Scenes/DefaultLocation/CastleSideBridges.unity",
            "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity",
            "Assets/Scenes/DefaultLocation/InteriorFloorBoxes.unity",
            "Assets/Scenes/DefaultLocation/MergedCylinders.unity",
            "Assets/Scenes/DefaultLocation/Other.unity",
            "Assets/Scenes/DefaultLocation/PillarSet2.unity",
            "Assets/Scenes/DefaultLocation/Rocks.unity",
            "Assets/Scenes/DefaultLocation/Stairs.unity",
            "Assets/Scenes/DefaultLocation/StaticMeshActors.unity",
            "Assets/Scenes/DefaultLocation/Stones.unity",
            "Assets/Scenes/DefaultLocation/SupportPillars.unity",
            "Assets/Scenes/DefaultLocation/SupportTowers.unity",
            "Assets/Scenes/DefaultLocation/Walls.unity",
            "Assets/Scenes/DefaultLocation/WoodDeco.unity"
        };

        public static LightingSettings CreatePCLightingSettings()
        {
            var settings = new LightingSettings();
            settings.name = "PCLightingSettings";
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            settings.lightmapResolution = 10; // 10 texels/unit for High Quality PC Location
            settings.directSampleCount = 32;
            settings.indirectSampleCount = 128;
            settings.environmentSampleCount = 128;
            settings.maxBounces = 2;
            settings.lightmapMaxSize = 1024;
            settings.directionalityMode = LightmapsMode.CombinedDirectional;
            settings.lightmapCompression = LightmapCompression.NormalQuality;
            settings.ao = true;
            settings.aoMaxDistance = 2.0f;
            return settings;
        }

        public static void ApplyPCBakeSettings()
        {
            LightingSettings pcSettings = CreatePCLightingSettings();
            Lightmapping.lightingSettings = pcSettings;

            StaticOcclusionCulling.smallestOccluder = 2.0f;
            StaticOcclusionCulling.smallestHole = 0.25f;
            StaticOcclusionCulling.backfaceThreshold = 100.0f;
        }

        public static void ConfigureAllSceneFlags()
        {
            foreach (string scenePath in AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool isStructural = Array.Exists(StructuralScenes, path => path.Equals(scenePath, StringComparison.OrdinalIgnoreCase));

                int modifiedCount = 0;
                Action<GameObject> processGO = null;
                processGO = (go) =>
                {
                    var flags = GameObjectUtility.GetStaticEditorFlags(go);
                    var renderer = go.GetComponent<Renderer>();

                    if (renderer != null || go.transform.childCount > 0)
                    {
                        StaticEditorFlags newFlags = flags;
                        newFlags |= StaticEditorFlags.ContributeGI;
                        newFlags |= StaticEditorFlags.OccludeeStatic;

                        if (isStructural)
                        {
                            if (renderer != null)
                            {
                                Vector3 size = renderer.bounds.size;
                                float maxDim = Mathf.Max(size.x, size.y, size.z);
                                if (maxDim >= 1.5f)
                                {
                                    newFlags |= StaticEditorFlags.OccluderStatic;
                                }
                                else
                                {
                                    newFlags &= ~StaticEditorFlags.OccluderStatic;
                                }
                            }
                            else
                            {
                                newFlags |= StaticEditorFlags.OccluderStatic;
                            }
                        }
                        else
                        {
                            newFlags &= ~StaticEditorFlags.OccluderStatic;
                        }

                        if (newFlags != flags)
                        {
                            GameObjectUtility.SetStaticEditorFlags(go, newFlags);
                            EditorUtility.SetDirty(go);
                            modifiedCount++;
                        }
                    }

                    foreach (Transform child in go.transform)
                    {
                        processGO(child.gameObject);
                    }
                };

                foreach (var root in scene.GetRootGameObjects())
                {
                    processGO(root);
                }

                if (modifiedCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            AssetDatabase.SaveAssets();
            WriteLog("Configured static flags across all 23 scenes.");
        }

        [MenuItem("Tools/Bake/Verify Occlusion Culling Data")]
        public static void VerifyOcclusionData()
        {
            string reportPath = "Assets/Scenes/DefaultLocation/occlusion_report.txt";
            var report = new System.Text.StringBuilder();
            report.AppendLine($"=== OCCLUSION CULLING VERIFICATION REPORT ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===");

            int bakedCount = 0;
            int missingCount = 0;

            for (int i = 0; i < AllScenes.Length; i++)
            {
                string scenePath = AllScenes[i];
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                string sceneFolder = Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath));
                string occDataPath = Path.Combine(sceneFolder, "OcclusionCullingData.asset");
                bool hasAsset = File.Exists(occDataPath);
                long fileSize = hasAsset ? new FileInfo(occDataPath).Length : 0;
                int umbraSize = StaticOcclusionCulling.umbraDataSize;
                bool hasData = hasAsset || umbraSize > 0;

                if (hasData)
                {
                    bakedCount++;
                    string msg = $"[{i + 1}/{AllScenes.Length}] {Path.GetFileName(scenePath)}: BAKED (Asset Size: {fileSize:N0} bytes, Umbra: {umbraSize} B)";
                    Debug.Log("[LocationBakeTool] " + msg);
                    report.AppendLine(msg);
                }
                else
                {
                    missingCount++;
                    string msg = $"[{i + 1}/{AllScenes.Length}] {Path.GetFileName(scenePath)}: MISSING OCCLUSION DATA";
                    Debug.LogWarning("[LocationBakeTool] " + msg);
                    report.AppendLine(msg);
                }
            }

            string summary = $"=== SUMMARY: {bakedCount} Baked, {missingCount} Missing out of {AllScenes.Length} scenes ===";
            Debug.Log("[LocationBakeTool] " + summary);
            report.AppendLine(summary);

            File.WriteAllText(reportPath, report.ToString());
        }

        [MenuItem("Tools/Bake/Bake Occlusion Data for All Scenes")]
        public static void BakeAllOcclusionData()
        {
            WriteLog($"=== STARTING OCCLUSION BAKE FOR ALL 23 SCENES ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===");
            ConfigureAllSceneFlags();

            for (int i = 0; i < AllScenes.Length; i++)
            {
                string scenePath = AllScenes[i];
                WriteLog($"[{i + 1}/{AllScenes.Length}] Baking occlusion for {Path.GetFileName(scenePath)}...");
                DateTime startTime = DateTime.Now;

                Scene scene;
                if (scenePath.Equals("Assets/Scenes/DefaultLocation/DefaultLocaiton.unity", StringComparison.OrdinalIgnoreCase))
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    foreach (string subPath in AllScenes)
                    {
                        if (!subPath.Equals(scenePath, StringComparison.OrdinalIgnoreCase))
                        {
                            EditorSceneManager.OpenScene(subPath, OpenSceneMode.Additive);
                        }
                    }
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }

                StaticOcclusionCulling.smallestOccluder = 2.0f;
                StaticOcclusionCulling.smallestHole = 0.25f;
                StaticOcclusionCulling.backfaceThreshold = 100.0f;

                bool occlResult = StaticOcclusionCulling.Compute();
                int umbraSize = StaticOcclusionCulling.umbraDataSize;

                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();

                double durationSec = (DateTime.Now - startTime).TotalSeconds;
                WriteLog($"[{i + 1}/{AllScenes.Length}] Completed {Path.GetFileName(scenePath)} -> Occlusion: {occlResult} (Umbra Data Size: {umbraSize} B, Duration: {durationSec:F1}s)");
            }

            WriteLog($"=== OCCLUSION BAKE COMPLETED FOR ALL 23 SCENES ===");
        }

        [MenuItem("Tools/Bake/Bake Missing Occlusion Data Only")]
        public static void BakeMissingOcclusionData()
        {
            WriteLog($"=== STARTING MISSING OCCLUSION BAKE ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===");
            ConfigureAllSceneFlags();

            int bakedCount = 0;
            for (int i = 0; i < AllScenes.Length; i++)
            {
                string scenePath = AllScenes[i];
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int umbraSize = StaticOcclusionCulling.umbraDataSize;

                if (umbraSize > 0)
                {
                    WriteLog($"[{i + 1}/{AllScenes.Length}] {Path.GetFileName(scenePath)} already has occlusion data ({umbraSize} B). Skipping.");
                    continue;
                }

                WriteLog($"[{i + 1}/{AllScenes.Length}] {Path.GetFileName(scenePath)} is MISSING occlusion data. Baking now...");
                DateTime startTime = DateTime.Now;

                if (scenePath.Equals("Assets/Scenes/DefaultLocation/DefaultLocaiton.unity", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string subPath in AllScenes)
                    {
                        if (!subPath.Equals(scenePath, StringComparison.OrdinalIgnoreCase))
                        {
                            EditorSceneManager.OpenScene(subPath, OpenSceneMode.Additive);
                        }
                    }
                }

                StaticOcclusionCulling.smallestOccluder = 2.0f;
                StaticOcclusionCulling.smallestHole = 0.25f;
                StaticOcclusionCulling.backfaceThreshold = 100.0f;

                bool occlResult = StaticOcclusionCulling.Compute();
                int newUmbraSize = StaticOcclusionCulling.umbraDataSize;

                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();

                double durationSec = (DateTime.Now - startTime).TotalSeconds;
                WriteLog($"[{i + 1}/{AllScenes.Length}] Completed {Path.GetFileName(scenePath)} -> Occlusion: {occlResult} (Umbra Data Size: {newUmbraSize} B, Duration: {durationSec:F1}s)");
                bakedCount++;
            }

            WriteLog($"=== MISSING OCCLUSION BAKE COMPLETED ({bakedCount} scenes baked) ===");
        }

        [MenuItem("Tools/Bake/Bake All Scenes Sync (Occlusion + Lighting)")]
        public static void BakeAllScenesSync()
        {
            File.WriteAllText(LogFilePath, $"=== STARTING ALL 23 SCENES BAKE ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===" + Environment.NewLine);
            
            ConfigureAllSceneFlags();

            for (int i = 0; i < AllScenes.Length; i++)
            {
                string scenePath = AllScenes[i];
                WriteLog($"[{i + 1}/{AllScenes.Length}] Starting bake for {Path.GetFileName(scenePath)}...");
                
                DateTime startTime = DateTime.Now;
                
                Scene scene;
                if (scenePath.Equals("Assets/Scenes/DefaultLocation/DefaultLocaiton.unity", StringComparison.OrdinalIgnoreCase))
                {
                    // Open main location scene and load all subscenes additively for multi-scene seamless bake
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    foreach (string subPath in AllScenes)
                    {
                        if (!subPath.Equals(scenePath, StringComparison.OrdinalIgnoreCase))
                        {
                            EditorSceneManager.OpenScene(subPath, OpenSceneMode.Additive);
                        }
                    }
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }

                ApplyPCBakeSettings();

                bool occlResult = StaticOcclusionCulling.Compute();
                int umbraSize = StaticOcclusionCulling.umbraDataSize;

                bool lightResult = Lightmapping.Bake();

                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();

                double durationSec = (DateTime.Now - startTime).TotalSeconds;
                WriteLog($"[{i + 1}/{AllScenes.Length}] Completed {Path.GetFileName(scenePath)} -> Occlusion: {occlResult} (Umbra: {umbraSize} B), Lightmaps: {lightResult} (Duration: {durationSec:F1}s)");
            }

            WriteLog($"=== ALL 23 SCENES SUCCESSFULLY BAKED AND SAVED ===");
        }

        private const string BAKE_TEMP_LIGHTS_NAME = "_BakeCopiedLightsContainer";
        private const int EXPECTED_COPIED_LIGHT_COUNT = 60;
        private static readonly float[] _lodScaleInLightmapByLevel = { 1.0f, 0.5f, 0.25f, 0.125f, 0.0625f };
        private static bool _bakeQueued;

        private sealed class GameObjectState
        {
            public GameObject GameObject;
            public bool ActiveSelf;
        }

        private sealed class LightState
        {
            public Light Light;
            public bool Enabled;
            public LightmapBakeType BakeType;
        }

        [MenuItem("Tools/Bake/Inspect DefaultLocation Lights")]
        public static void InspectDefaultLocationLights()
        {
            string mainScenePath = "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity";
            var scene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

            GameObject dirLightGO = FindGameObjectByNameOrComponent("Directional Light", LightType.Directional);
            GameObject pointLightsGO = GameObject.Find("PointLights");
            GameObject spotLightsGO = GameObject.Find("SpotLights");

            Debug.Log($"[LocationBakeTool] Directional Light GO: {(dirLightGO != null ? dirLightGO.name : "NOT FOUND")}");
            Debug.Log($"[LocationBakeTool] PointLights Container GO: {(pointLightsGO != null ? pointLightsGO.name + $" ({pointLightsGO.GetComponentsInChildren<Light>(true).Length} lights)" : "NOT FOUND")}");
            Debug.Log($"[LocationBakeTool] SpotLights Container GO: {(spotLightsGO != null ? spotLightsGO.name + $" ({spotLightsGO.GetComponentsInChildren<Light>(true).Length} lights)" : "NOT FOUND")}");
        }

        [MenuItem("Tools/Bake/Bake Subscenes With Copied Baked Lights")]
        public static void BakeSubscenesWithCopiedLights()
        {
            if (_bakeQueued)
            {
                WriteLog("Bake request ignored because the 23-scene bake is already queued.");
                return;
            }

            _bakeQueued = true;
            EditorApplication.delayCall += () =>
            {
                _bakeQueued = false;
                try
                {
                    RunSubsceneBake();
                }
                catch (Exception exception)
                {
                    WriteLog($"ERROR: Copy-light bake stopped: {exception}");
                    Debug.LogException(exception);
                }
            };
        }

        private static void RunSubsceneBake()
        {
            const string mainScenePath = "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity";
            const string pointLightsRootName = "PointLights";
            const string spotLightsRootName = "SpotLights";
            File.WriteAllText(LogFilePath, $"=== STARTING ALL 23 SCENES BAKE ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===" + Environment.NewLine);

            Scene mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
            GameObject pointLightsRoot = FindSceneGameObjectByName(mainScene, pointLightsRootName);
            GameObject spotLightsRoot = FindSceneGameObjectByName(mainScene, spotLightsRootName);

            if (pointLightsRoot == null || spotLightsRoot == null)
            {
                throw new InvalidOperationException("DefaultLocaiton.unity must contain both PointLights and SpotLights source roots.");
            }

            int sourceLightCount = pointLightsRoot.GetComponentsInChildren<Light>(true).Length +
                spotLightsRoot.GetComponentsInChildren<Light>(true).Length;
            if (sourceLightCount != EXPECTED_COPIED_LIGHT_COUNT)
            {
                throw new InvalidOperationException($"Expected {EXPECTED_COPIED_LIGHT_COUNT} PointLights/SpotLights, found {sourceLightCount}.");
            }

            if (RemoveExistingBakeContainers(mainScene))
            {
                SaveSceneAndAssets(mainScene);
            }

            List<GameObject> tempPrototypes = new List<GameObject>
            {
                CreateTemporaryPrototype(pointLightsRoot),
                CreateTemporaryPrototype(spotLightsRoot)
            };

            try
            {
                for (int i = 0; i < AllScenes.Length; i++)
                {
                    string scenePath = AllScenes[i];
                    string sceneName = Path.GetFileName(scenePath);
                    DateTime startTime = DateTime.Now;
                    Scene scene = default;

                    WriteLog($"[{i + 1}/{AllScenes.Length}] Starting bake for {sceneName}...");

                    try
                    {
                        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        bool isSourceScene = scenePath.Equals(mainScenePath, StringComparison.OrdinalIgnoreCase);

                        if (RemoveExistingBakeContainers(scene))
                        {
                            SaveSceneAndAssets(scene);
                        }

                        int[] lodCounts = ApplyLodScaleInLightmap(scene);
                        if (isSourceScene)
                        {
                            BakeSourceScene(scene, pointLightsRootName, spotLightsRootName, lodCounts, i, startTime);
                        }
                        else
                        {
                            BakeCopiedLightScene(scene, tempPrototypes, lodCounts, i, startTime);
                        }
                    }
                    catch (Exception exception)
                    {
                        DisableAndSaveCopiedLightsAfterFailure(scene);
                        WriteLog($"[{i + 1}/{AllScenes.Length}] FAILED {sceneName}: {exception}");
                        throw;
                    }
                }

                ReopenAllScenesAdditively(mainScenePath);
                WriteLog("=== ALL 23 SCENES SUCCESSFULLY BAKED AND SAVED ===");
            }
            finally
            {
                foreach (GameObject prototype in tempPrototypes)
                {
                    if (prototype != null)
                    {
                        UnityEngine.Object.DestroyImmediate(prototype);
                    }
                }
            }
        }

        private static void BakeSourceScene(Scene scene, string pointLightsRootName, string spotLightsRootName, int[] lodCounts, int sceneIndex, DateTime startTime)
        {
            GameObject pointLightsRoot = FindSceneGameObjectByName(scene, pointLightsRootName);
            GameObject spotLightsRoot = FindSceneGameObjectByName(scene, spotLightsRootName);
            if (pointLightsRoot == null || spotLightsRoot == null)
            {
                throw new InvalidOperationException("The source light roots were not found when the source scene was reopened.");
            }

            List<GameObjectState> gameObjectStates = CaptureGameObjectStates(pointLightsRoot, spotLightsRoot);
            List<LightState> lightStates = CaptureLightStates(pointLightsRoot, spotLightsRoot);

            try
            {
                SetHierarchyActiveAndBaked(pointLightsRoot, spotLightsRoot);
                ApplyPCBakeSettings();

                WriteLog($"[{sceneIndex + 1}/{AllScenes.Length}] Configured {lightStates.Count} original PointLights/SpotLights (Enabled & Baked mode); LODs {FormatLodCounts(lodCounts)}. Starting Lightmap Bake for {Path.GetFileName(scene.path)}...");
                bool bakeSuccess = Lightmapping.Bake();
                if (!bakeSuccess)
                {
                    throw new InvalidOperationException("Lightmapping.Bake() returned false.");
                }
            }
            finally
            {
                RestoreOriginalLightStates(gameObjectStates, lightStates);
                SaveSceneAndAssets(scene);
            }

            WriteLog($"[{sceneIndex + 1}/{AllScenes.Length}] Completed {Path.GetFileName(scene.path)} Lightmap Bake -> Success: True (LOD assignments: {FormatLodCounts(lodCounts)}, Duration: {(DateTime.Now - startTime).TotalSeconds:F1}s). Original lights restored; Directional Light untouched.");
        }

        private static void BakeCopiedLightScene(Scene scene, List<GameObject> tempPrototypes, int[] lodCounts, int sceneIndex, DateTime startTime)
        {
            GameObject container = new GameObject(BAKE_TEMP_LIGHTS_NAME);
            try
            {
                foreach (GameObject prototype in tempPrototypes)
                {
                    GameObject copy = UnityEngine.Object.Instantiate(prototype, container.transform, true);
                    copy.name = prototype.name;
                    SetHierarchyHideFlags(copy, HideFlags.None);
                }

                SetHierarchyActiveAndBaked(container);
                int copiedLightCount = container.GetComponentsInChildren<Light>(true).Length;
                if (copiedLightCount != EXPECTED_COPIED_LIGHT_COUNT)
                {
                    throw new InvalidOperationException($"Expected {EXPECTED_COPIED_LIGHT_COUNT} copied lights in {scene.path}, found {copiedLightCount}.");
                }

                ApplyPCBakeSettings();
                WriteLog($"[{sceneIndex + 1}/{AllScenes.Length}] Configured {copiedLightCount} copied lights (Enabled & Baked mode); LODs {FormatLodCounts(lodCounts)}. Starting Lightmap Bake for {Path.GetFileName(scene.path)}...");
                bool bakeSuccess = Lightmapping.Bake();
                if (!bakeSuccess)
                {
                    throw new InvalidOperationException("Lightmapping.Bake() returned false.");
                }
            }
            finally
            {
                DisableCopiedHierarchy(container);
                SaveSceneAndAssets(scene);
            }

            WriteLog($"[{sceneIndex + 1}/{AllScenes.Length}] Completed {Path.GetFileName(scene.path)} Lightmap Bake -> Success: True (LOD assignments: {FormatLodCounts(lodCounts)}, Duration: {(DateTime.Now - startTime).TotalSeconds:F1}s). Copied lights DISABLED.");
        }

        private static GameObject CreateTemporaryPrototype(GameObject sourceRoot)
        {
            GameObject prototype = UnityEngine.Object.Instantiate(sourceRoot);
            prototype.name = sourceRoot.name;
            SetHierarchyHideFlags(prototype, HideFlags.HideAndDontSave);
            return prototype;
        }

        private static int[] ApplyLodScaleInLightmap(Scene scene)
        {
            int[] counts = new int[_lodScaleInLightmapByLevel.Length];
            int nullRendererCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (LODGroup lodGroup in root.GetComponentsInChildren<LODGroup>(true))
                {
                    LOD[] lods = lodGroup.GetLODs();
                    if (lods.Length > _lodScaleInLightmapByLevel.Length)
                    {
                        throw new InvalidOperationException($"LODGroup '{lodGroup.name}' in {scene.path} has {lods.Length} levels; only LOD0-LOD4 are defined by the bake plan.");
                    }

                    for (int level = 0; level < lods.Length; level++)
                    {
                        foreach (Renderer renderer in lods[level].renderers)
                        {
                            if (renderer == null)
                            {
                                nullRendererCount++;
                                continue;
                            }

                            MeshRenderer meshRenderer = renderer as MeshRenderer;
                            if (meshRenderer == null)
                            {
                                throw new InvalidOperationException($"LOD renderer '{renderer.name}' in {scene.path} is not a MeshRenderer and has no Scale in Lightmap property.");
                            }

                            meshRenderer.scaleInLightmap = _lodScaleInLightmapByLevel[level];
                            EditorUtility.SetDirty(meshRenderer);
                            counts[level]++;
                        }
                    }
                }
            }

            if (nullRendererCount > 0)
            {
                WriteLog($"Skipped {nullRendererCount} null LOD renderer slot(s) in {Path.GetFileName(scene.path)}; counted only assigned renderers.");
            }

            return counts;
        }

        private static string FormatLodCounts(int[] counts)
        {
            return $"LOD0={counts[0]}, LOD1={counts[1]}, LOD2={counts[2]}, LOD3={counts[3]}, LOD4={counts[4]}";
        }

        private static List<GameObjectState> CaptureGameObjectStates(GameObject firstRoot, GameObject secondRoot)
        {
            List<GameObjectState> states = new List<GameObjectState>();
            CaptureGameObjectStates(firstRoot, states);
            CaptureGameObjectStates(secondRoot, states);
            return states;
        }

        private static void CaptureGameObjectStates(GameObject root, List<GameObjectState> states)
        {
            states.Add(new GameObjectState { GameObject = root, ActiveSelf = root.activeSelf });
            foreach (Transform child in root.transform)
            {
                CaptureGameObjectStates(child.gameObject, states);
            }
        }

        private static List<LightState> CaptureLightStates(GameObject firstRoot, GameObject secondRoot)
        {
            List<LightState> states = new List<LightState>();
            CaptureLightStates(firstRoot, states);
            CaptureLightStates(secondRoot, states);
            return states;
        }

        private static void CaptureLightStates(GameObject root, List<LightState> states)
        {
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                states.Add(new LightState { Light = light, Enabled = light.enabled, BakeType = light.lightmapBakeType });
            }
        }

        private static void SetHierarchyActiveAndBaked(params GameObject[] roots)
        {
            foreach (GameObject root in roots)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    transform.gameObject.SetActive(true);
                }

                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                {
                    light.gameObject.SetActive(true);
                    light.enabled = true;
                    light.lightmapBakeType = LightmapBakeType.Baked;
                }
            }
        }

        private static void RestoreOriginalLightStates(List<GameObjectState> gameObjectStates, List<LightState> lightStates)
        {
            foreach (LightState state in lightStates)
            {
                state.Light.enabled = state.Enabled;
                state.Light.lightmapBakeType = state.BakeType;
            }

            for (int i = gameObjectStates.Count - 1; i >= 0; i--)
            {
                gameObjectStates[i].GameObject.SetActive(gameObjectStates[i].ActiveSelf);
            }
        }

        private static void DisableCopiedHierarchy(GameObject container)
        {
            if (container == null)
            {
                return;
            }

            foreach (Light light in container.GetComponentsInChildren<Light>(true))
            {
                light.enabled = false;
            }

            foreach (Transform transform in container.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.SetActive(false);
            }
        }

        private static void DisableAndSaveCopiedLightsAfterFailure(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            GameObject container = FindSceneGameObjectByName(scene, BAKE_TEMP_LIGHTS_NAME);
            if (container != null)
            {
                DisableCopiedHierarchy(container);
                SaveSceneAndAssets(scene);
                WriteLog($"Disabled and saved copied lights after failure in {Path.GetFileName(scene.path)}.");
            }
        }

        private static bool RemoveExistingBakeContainers(Scene scene)
        {
            List<GameObject> containers = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectBakeContainerRoots(root, false, containers);
            }

            foreach (GameObject container in containers)
            {
                UnityEngine.Object.DestroyImmediate(container);
            }

            if (containers.Count > 0)
            {
                WriteLog($"Removed {containers.Count} existing {BAKE_TEMP_LIGHTS_NAME} container(s) from {Path.GetFileName(scene.path)}.");
            }

            return containers.Count > 0;
        }

        private static void CollectBakeContainerRoots(GameObject current, bool parentIsContainer, List<GameObject> containers)
        {
            bool isContainer = current.name.Equals(BAKE_TEMP_LIGHTS_NAME, StringComparison.Ordinal);
            if (isContainer && !parentIsContainer)
            {
                containers.Add(current);
                return;
            }

            foreach (Transform child in current.transform)
            {
                CollectBakeContainerRoots(child.gameObject, isContainer, containers);
            }
        }

        private static GameObject FindSceneGameObjectByName(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                GameObject result = FindGameObjectByName(root, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static GameObject FindGameObjectByName(GameObject current, string name)
        {
            if (current.name.Equals(name, StringComparison.Ordinal))
            {
                return current;
            }

            foreach (Transform child in current.transform)
            {
                GameObject result = FindGameObjectByName(child.gameObject, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void SetHierarchyHideFlags(GameObject root, HideFlags hideFlags)
        {
            root.hideFlags = hideFlags;
            foreach (Transform child in root.transform)
            {
                SetHierarchyHideFlags(child.gameObject, hideFlags);
            }
        }

        private static void SaveSceneAndAssets(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void ReopenAllScenesAdditively(string mainScenePath)
        {
            Scene mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
            for (int i = 0; i < AllScenes.Length; i++)
            {
                string scenePath = AllScenes[i];
                if (!scenePath.Equals(mainScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }
            }

            SceneManager.SetActiveScene(mainScene);
            WriteLog("Reopened all 23 scenes additively with DefaultLocaiton.unity active.");
        }

        private static GameObject FindGameObjectByNameOrComponent(string name, LightType type)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) return go;

            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == type) return light.gameObject;
            }
            return null;
        }

        private static void WriteLog(string message)
        {
            string formatted = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Debug.Log("[LocationBakeTool] " + formatted);
            try
            {
                File.AppendAllText(LogFilePath, formatted + Environment.NewLine);
            }
            catch { }
        }
    }
}
