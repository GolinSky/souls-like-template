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
            string mainScenePath = "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity";
            WriteLog($"=== STARTING COPY-LIGHT BAKE FOR ALL SUBSCENES ({DateTime.Now:yyyy-MM-dd HH:mm:ss}) ===");

            // Step 1: Open main scene and collect target light GameObjects
            var mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

            GameObject dirLightGO = FindGameObjectByNameOrComponent("Directional Light", LightType.Directional);
            GameObject pointLightsGO = GameObject.Find("PointLights");
            GameObject spotLightsGO = GameObject.Find("SpotLights");

            List<GameObject> sourceLightRootGOs = new List<GameObject>();
            if (dirLightGO != null) sourceLightRootGOs.Add(dirLightGO);
            if (pointLightsGO != null) sourceLightRootGOs.Add(pointLightsGO);
            if (spotLightsGO != null) sourceLightRootGOs.Add(spotLightsGO);

            if (sourceLightRootGOs.Count == 0)
            {
                WriteLog("ERROR: No light objects (Directional Light, PointLights, SpotLights) found in DefaultLocaiton.unity!");
                return;
            }

            // Duplicate source light roots into temporary prototypes
            List<GameObject> tempPrototypes = new List<GameObject>();
            foreach (var srcGO in sourceLightRootGOs)
            {
                var dup = UnityEngine.Object.Instantiate(srcGO);
                dup.name = srcGO.name;
                dup.hideFlags = HideFlags.DontSave;
                tempPrototypes.Add(dup);
            }

            try
            {
                for (int i = 0; i < AllScenes.Length; i++)
                {
                    string scenePath = AllScenes[i];
                    string sceneName = Path.GetFileName(scenePath);

                    WriteLog($"[{i + 1}/{AllScenes.Length}] Processing scene {sceneName} for Light Baking...");
                    DateTime startTime = DateTime.Now;

                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    // Clean up any existing temp copied lights root in scene
                    GameObject existingContainer = GameObject.Find(BAKE_TEMP_LIGHTS_NAME);
                    if (existingContainer != null)
                    {
                        UnityEngine.Object.DestroyImmediate(existingContainer);
                    }

                    // Create new container for copied lights
                    GameObject container = new GameObject(BAKE_TEMP_LIGHTS_NAME);

                    // Copy each prototype hierarchy into the subscene
                    foreach (var proto in tempPrototypes)
                    {
                        var copy = UnityEngine.Object.Instantiate(proto, container.transform);
                        copy.name = proto.name;
                        copy.hideFlags = HideFlags.None;
                    }

                    // Ensure container and all lights inside are ENABLED and set to BAKED
                    container.SetActive(true);
                    int lightsConfigured = 0;
                    foreach (Transform child in container.GetComponentsInChildren<Transform>(true))
                    {
                        child.gameObject.SetActive(true);
                    }

                    foreach (var light in container.GetComponentsInChildren<Light>(true))
                    {
                        light.gameObject.SetActive(true);
                        light.enabled = true;
                        light.lightmapBakeType = LightmapBakeType.Baked;
                        lightsConfigured++;
                    }

                    // Apply PCBakeSettings
                    ApplyPCBakeSettings();

                    WriteLog($"[{i + 1}/{AllScenes.Length}] Configured {lightsConfigured} copied lights (Enabled & Baked mode). Starting Lightmap Bake for {sceneName}...");

                    // Perform Lightmap Bake synchronously
                    bool bakeSuccess = Lightmapping.Bake();

                    // Step 3: Disable copied lights in subscene so they are inactive at runtime
                    container.SetActive(false);
                    foreach (var light in container.GetComponentsInChildren<Light>(true))
                    {
                        light.enabled = false;
                        light.gameObject.SetActive(false);
                    }

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();

                    double durationSec = (DateTime.Now - startTime).TotalSeconds;
                    WriteLog($"[{i + 1}/{AllScenes.Length}] Completed {sceneName} Lightmap Bake -> Success: {bakeSuccess} (Duration: {durationSec:F1}s). Copied lights DISABLED.");
                }

                WriteLog("=== ALL SUBSCENES SUCCESSFULLY BAKED WITH COPIED LIGHTS AND SAVED ===");
            }
            finally
            {
                // Clean up in-memory prototypes
                foreach (var proto in tempPrototypes)
                {
                    if (proto != null) UnityEngine.Object.DestroyImmediate(proto);
                }
            }
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
