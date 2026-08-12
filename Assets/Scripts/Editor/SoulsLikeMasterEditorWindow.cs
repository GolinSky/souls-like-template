using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Ui.Base;
using UI.Base;
using UI.Base.Editor;
using DoubleL;

namespace SoulsLike.EditorTools
{
    public class SoulsLikeMasterEditorWindow : EditorWindow
    {
        private int selectedTab = 0;
        private readonly string[] tabTitles = new string[]
        {
            "Scene & Bake Tools",
            "UI & Canvas Tools",
            "Animation Tools"
        };

        // Tab 1: Occlusion & Bake parameters
        private float minOccluderSize = 3.5f;

        // Tab 3: Animation Retargeter parameters
        private AnimationClip retargetSourceClip;
        private GameObject retargetSourceRoot;
        private GameObject retargetTargetRoot;
        private string sourceWeaponName = "weapon_r";
        private string targetWeaponName = "weapon_r_socket";

        // Scroll positions
        private Vector2 tab1Scroll;
        private Vector2 tab2Scroll;
        private Vector2 tab3Scroll;
        private Vector2 logScroll;

        // Log messages buffer
        private string logOutput = "Master Editor Window initialized.\nSelect a tab and execute editor tools directly.";

        [MenuItem("Tools/SoulsLike Master Editor", false, 0)]
        [MenuItem("Window/SoulsLike Master Editor", false, 0)]
        public static void OpenWindow()
        {
            var window = GetWindow<SoulsLikeMasterEditorWindow>("Master Editor");
            window.minSize = new Vector2(550, 600);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();

            selectedTab = GUILayout.Toolbar(selectedTab, tabTitles, GUILayout.Height(30));
            EditorGUILayout.Space(5);

            switch (selectedTab)
            {
                case 0:
                    DrawSceneAndBakeToolsTab();
                    break;
                case 1:
                    DrawUiAndCanvasToolsTab();
                    break;
                case 2:
                    DrawAnimationToolsTab();
                    break;
            }

            DrawLogFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("SoulsLike Unified Master Editor Tool", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        #region Tab 1: Scene & Bake Tools
        private void DrawSceneAndBakeToolsTab()
        {
            tab1Scroll = EditorGUILayout.BeginScrollView(tab1Scroll);

            // Category 1: Occlusion Optimization
            DrawCategoryHeader("Occlusion Optimization");
            EditorGUILayout.HelpBox(
                "Optimize static flags on scene objects to prevent memory crashes during occlusion baking. " +
                "Strips 'Occluder Static' from small props while retaining 'Occludee Static'.",
                MessageType.Info
            );
            minOccluderSize = EditorGUILayout.FloatField("Min Occluder Size (meters)", minOccluderSize);
            if (GUILayout.Button("Optimize Scene Occlusion (Prevent Baking Crash)", GUILayout.Height(30)))
            {
                ExecuteAction("Optimizing Scene Occlusion...", () =>
                {
                    OcclusionOptimizer.OptimizeSceneOcclusion(minOccluderSize);
                    LogInfo($"Completed occlusion optimization with min size threshold: {minOccluderSize}m");
                });
            }

            EditorGUILayout.Space(15);

            // Category 2: Occlusion Baking & Verification
            DrawCategoryHeader("Occlusion Baking & Verification");
            if (GUILayout.Button("Bake Occlusion Data for All Scenes", GUILayout.Height(28)))
            {
                ExecuteAction("Baking Occlusion Data for All Scenes...", () =>
                {
                    LocationBakeTool.BakeAllOcclusionData();
                    LogInfo("BakeAllOcclusionData completed successfully.");
                });
            }

            if (GUILayout.Button("Bake Missing Occlusion Data Only", GUILayout.Height(28)))
            {
                ExecuteAction("Baking Missing Occlusion Data...", () =>
                {
                    LocationBakeTool.BakeMissingOcclusionData();
                    LogInfo("BakeMissingOcclusionData completed successfully.");
                });
            }

            if (GUILayout.Button("Verify Occlusion Culling Data", GUILayout.Height(28)))
            {
                ExecuteAction("Verifying Occlusion Data...", () =>
                {
                    LocationBakeTool.VerifyOcclusionData();
                    LogInfo("VerifyOcclusionData completed. Check 'Assets/Scenes/DefaultLocation/occlusion_report.txt'.");
                });
            }

            EditorGUILayout.Space(15);

            // Category 3: Lighting & Scene Baking
            DrawCategoryHeader("Lighting & Multi-Scene Bakes");
            if (GUILayout.Button("Bake All Scenes Sync (Occlusion + Lighting)", GUILayout.Height(28)))
            {
                ExecuteAction("Baking All Scenes Synchronously...", () =>
                {
                    LocationBakeTool.BakeAllScenesSync();
                    LogInfo("BakeAllScenesSync completed.");
                });
            }

            if (GUILayout.Button("Inspect DefaultLocation Lights", GUILayout.Height(28)))
            {
                ExecuteAction("Inspecting Default Location Lights...", () =>
                {
                    LocationBakeTool.InspectDefaultLocationLights();
                    LogInfo("InspectDefaultLocationLights finished. Check Console log.");
                });
            }

            if (GUILayout.Button("Bake Subscenes With Copied Baked Lights", GUILayout.Height(28)))
            {
                ExecuteAction("Baking Subscenes With Copied Lights...", () =>
                {
                    LocationBakeTool.BakeSubscenesWithCopiedLights();
                    LogInfo("BakeSubscenesWithCopiedLights completed.");
                });
            }

            if (GUILayout.Button("Configure Static Flags Across All Scenes", GUILayout.Height(28)))
            {
                ExecuteAction("Configuring Static Flags...", () =>
                {
                    LocationBakeTool.ConfigureAllSceneFlags();
                    LogInfo("ConfigureAllSceneFlags completed across scenes.");
                });
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Tab 2: UI & Canvas Tools
        private void DrawUiAndCanvasToolsTab()
        {
            tab2Scroll = EditorGUILayout.BeginScrollView(tab2Scroll);

            DrawCategoryHeader("Custom UI Button Instantiation");
            EditorGUILayout.HelpBox(
                "Instantiate pre-configured Custom Canvas Buttons into the active hierarchy or active Canvas.",
                MessageType.Info
            );

            GameObject contextGO = Selection.activeGameObject;
            EditorGUILayout.ObjectField("Target Hierarchy Context", contextGO, typeof(GameObject), true);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Spawn Primary Button", GUILayout.Height(28)))
            {
                CustomButtonHierarchyMenu.CreateButton(InputTypes.Primary, contextGO);
                LogInfo("Spawned Primary Button.");
            }

            if (GUILayout.Button("Spawn Secondary Button", GUILayout.Height(28)))
            {
                CustomButtonHierarchyMenu.CreateButton(InputTypes.Secondary, contextGO);
                LogInfo("Spawned Secondary Button.");
            }

            if (GUILayout.Button("Spawn Destructive Button", GUILayout.Height(28)))
            {
                CustomButtonHierarchyMenu.CreateButton(InputTypes.Destructive, contextGO);
                LogInfo("Spawned Destructive Button.");
            }

            if (GUILayout.Button("Spawn Dismiss Button", GUILayout.Height(28)))
            {
                CustomButtonHierarchyMenu.CreateButton(InputTypes.Dismiss, contextGO);
                LogInfo("Spawned Dismiss Button.");
            }

            if (GUILayout.Button("Spawn Toggle Button", GUILayout.Height(28)))
            {
                CustomButtonHierarchyMenu.CreateToggleButton(contextGO);
                LogInfo("Spawned Toggle Button.");
            }

            EditorGUILayout.Space(15);

            DrawCategoryHeader("UI Mapping Configuration");
            if (GUILayout.Button("Locate & Select CustomButtonMapping Asset", GUILayout.Height(28)))
            {
                CustomButtonMapping mapping = CustomButtonHierarchyMenu.TryGetMappingAsset();
                if (mapping != null)
                {
                    Selection.activeObject = mapping;
                    EditorGUIUtility.PingObject(mapping);
                    LogInfo($"Selected CustomButtonMapping asset at: {AssetDatabase.GetAssetPath(mapping)}");
                }
                else
                {
                    LogError("No CustomButtonMapping asset found in project!");
                }
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Tab 3: Animation Tools
        private void DrawAnimationToolsTab()
        {
            tab3Scroll = EditorGUILayout.BeginScrollView(tab3Scroll);

            DrawCategoryHeader("Weapon Bone Retargeter");
            EditorGUILayout.HelpBox(
                "Convert weapon bone keyframes between character armatures using scene references.",
                MessageType.Info
            );

            retargetSourceClip = (AnimationClip)EditorGUILayout.ObjectField("Source AnimationClip", retargetSourceClip, typeof(AnimationClip), false);
            retargetSourceRoot = (GameObject)EditorGUILayout.ObjectField("Source Character (Scene)", retargetSourceRoot, typeof(GameObject), true);
            retargetTargetRoot = (GameObject)EditorGUILayout.ObjectField("Target Character (Scene)", retargetTargetRoot, typeof(GameObject), true);

            EditorGUILayout.Space(5);
            sourceWeaponName = EditorGUILayout.TextField("Source Weapon Bone Name", sourceWeaponName);
            targetWeaponName = EditorGUILayout.TextField("Target Weapon Bone Name", targetWeaponName);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Run Retarget Conversion", GUILayout.Height(35)))
            {
                if (retargetSourceClip == null || retargetSourceRoot == null || retargetTargetRoot == null)
                {
                    LogError("Please assign Source AnimationClip, Source Character, and Target Character!");
                }
                else
                {
                    ExecuteAction("Running Weapon Bone Retargeting...", () =>
                    {
                        var window = GetWindow<WeaponBoneRetargeter>();
                        window.sourceClip = retargetSourceClip;
                        window.sourceRoot = retargetSourceRoot;
                        window.targetRoot = retargetTargetRoot;
                        window.sourceWeaponName = sourceWeaponName;
                        window.targetWeaponName = targetWeaponName;
                        window.Show();
                        LogInfo("Weapon Bone Retargeter initialized.");
                    });
                }
            }

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Utility UI Components & Logging
        private void DrawCategoryHeader(string title)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void DrawLogFooter()
        {
            EditorGUILayout.Space(5);
            DrawCategoryHeader("Operation Log");
            logScroll = EditorGUILayout.BeginScrollView(logScroll, GUILayout.Height(100));
            EditorGUILayout.TextArea(logOutput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Log", GUILayout.Height(20)))
            {
                logOutput = "";
            }
        }

        private void ExecuteAction(string startMessage, Action action)
        {
            LogInfo(startMessage);
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                LogError($"Action failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LogInfo(string msg)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] [INFO] {msg}";
            logOutput += "\n" + entry;
            Debug.Log("[MasterEditor] " + msg);
            Repaint();
        }

        private void LogError(string msg)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] [ERROR] {msg}";
            logOutput += "\n" + entry;
            Debug.LogError("[MasterEditor] " + msg);
            Repaint();
        }
        #endregion
    }
}
