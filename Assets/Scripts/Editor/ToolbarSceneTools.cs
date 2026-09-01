using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SoulsLike.Services.Scenes.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoulsLike.EditorTools
{
    /// <summary>
    /// Unity 6 Main Toolbar extension providing:
    /// 1. Play Game Button: Starts Play mode with scene index 0 (MainMenu).
    /// 2. Fast Play Game Button: Starts Play mode with scene index 0 with Domain & Scene Reload disabled.
    /// 3. Scenes Dropdown: Dynamic selector for scenes in Assets/Scenes that loads configured dependencies for primary scenes.
    /// </summary>
    [InitializeOnLoad]
    public static class ToolbarSceneTools
    {
        private const string PLAY_BUTTON_PATH = "SoulsLike/Play Game";
        private const string FAST_PLAY_BUTTON_PATH = "SoulsLike/Fast Play Game";
        private const string SCENE_DROPDOWN_PATH = "SoulsLike/Scene Selector";
        private const string SCENES_ROOT = "Assets/Scenes";
        private const string SCENE_DATA_PATH = "Assets/Settings/Data/SceneData.asset";

        private const string KEY_FAST_ACTIVE = "SoulsLike_IsFastPlayActive";
        private const string KEY_SAVED_ENABLED = "SoulsLike_SavedOptionsEnabled";
        private const string KEY_SAVED_OPTIONS = "SoulsLike_SavedOptionsFlags";

        static ToolbarSceneTools()
        {
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Ensure elements are shown on main toolbar when domain reloads
            EditorApplication.delayCall += EnsureToolbarElementsVisible;
        }

        [MainToolbarElement(
            PLAY_BUTTON_PATH,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 0)]
        private static MainToolbarButton CreatePlayButton()
        {
            bool isPlaying = EditorApplication.isPlaying;
            string label = isPlaying ? "⏹ Stop Game" : "▶ Play MainMenu";
            string tooltip = isPlaying ? "Stop Play Mode" : "Save current scene and start Game from scene index 0 (MainMenu)";

            return new MainToolbarButton(
                new MainToolbarContent(label, null, tooltip),
                OnPlayButtonClicked);
        }

        [MainToolbarElement(
            FAST_PLAY_BUTTON_PATH,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 1)]
        private static MainToolbarButton CreateFastPlayButton()
        {
            bool isPlaying = EditorApplication.isPlaying;
            bool isFastActive = SessionState.GetBool(KEY_FAST_ACTIVE, false);

            string label = (isPlaying && isFastActive) ? "⏹ Stop Fast" : "⚡ Fast MainMenu";
            string tooltip = "Start Game from scene index 0 (MainMenu) with Domain & Scene Reload disabled (Fast Mode)";

            return new MainToolbarButton(
                new MainToolbarContent(label, null, tooltip),
                OnFastPlayButtonClicked);
        }

        [MainToolbarElement(
            SCENE_DROPDOWN_PATH,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 2)]
        private static MainToolbarDropdown CreateSceneDropdown()
        {
            var activeSceneName = SceneManager.GetActiveScene().name;
            var label = string.IsNullOrEmpty(activeSceneName) ? "Select Scene" : $"🎬 {activeSceneName}";
            var tooltip = "Open a scene from Assets/Scenes and its configured dependencies";

            return new MainToolbarDropdown(
                new MainToolbarContent(label, null, tooltip),
                ShowSceneMenu);
        }

        [MenuItem("Tools/SoulsLike/Add Toolbar Controls to Main Toolbar", false, 1)]
        public static void EnsureToolbarElementsVisible()
        {
            var showAllMethod = typeof(MainToolbar).GetMethod(
                "ShowAll",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null);

            if (showAllMethod != null)
            {
                try
                {
                    showAllMethod.Invoke(null, new object[] { PLAY_BUTTON_PATH });
                    showAllMethod.Invoke(null, new object[] { FAST_PLAY_BUTTON_PATH });
                    showAllMethod.Invoke(null, new object[] { SCENE_DROPDOWN_PATH });
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ToolbarSceneTools] Failed to auto-enable toolbar elements: {ex.Message}");
                }
            }
        }

        private static void OnPlayButtonClicked()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                StartGameFromFirstScene(fastMode: false);
            }
        }

        private static void OnFastPlayButtonClicked()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                StartGameFromFirstScene(fastMode: true);
            }
        }

        [MenuItem("SoulsLike/Play Game (MainMenu - Index 0) _F5", false, 100)]
        public static void StartNormalGameFromMenu()
        {
            StartGameFromFirstScene(fastMode: false);
        }

        [MenuItem("SoulsLike/Fast Play Game (MainMenu - Index 0) %#F5", false, 101)]
        public static void StartFastGameFromMenu()
        {
            StartGameFromFirstScene(fastMode: true);
        }

        public static void StartGameFromFirstScene(bool fastMode)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string firstScenePath = GetFirstScenePath();

            if (string.IsNullOrEmpty(firstScenePath))
            {
                Debug.LogError("[ToolbarSceneTools] Could not locate scene at index 0 or MainMenu scene.");
                return;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(firstScenePath);
            if (sceneAsset == null)
            {
                Debug.LogError($"[ToolbarSceneTools] Could not load SceneAsset at path: {firstScenePath}");
                return;
            }

            if (fastMode)
            {
                // Save current Enter Play Mode options in SessionState to restore on exit
                SessionState.SetBool(KEY_FAST_ACTIVE, true);
                SessionState.SetBool(KEY_SAVED_ENABLED, EditorSettings.enterPlayModeOptionsEnabled);
                SessionState.SetInt(KEY_SAVED_OPTIONS, (int)EditorSettings.enterPlayModeOptions);

                // Configure Fast Play Mode (disable domain reload and scene reload)
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;

                Debug.Log($"[ToolbarSceneTools] Starting FAST Play Mode (Domain & Scene Reload Disabled) with scene index 0: {firstScenePath}");
            }
            else
            {
                SessionState.SetBool(KEY_FAST_ACTIVE, false);
                Debug.Log($"[ToolbarSceneTools] Starting Normal Play Mode with scene index 0: {firstScenePath}");
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Revert Fast Play Mode settings if fast play was used
                if (SessionState.GetBool(KEY_FAST_ACTIVE, false))
                {
                    EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(KEY_SAVED_ENABLED, false);
                    EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(KEY_SAVED_OPTIONS, 0);
                    SessionState.SetBool(KEY_FAST_ACTIVE, false);
                    Debug.Log("[ToolbarSceneTools] Exited Fast Play Mode: Original Enter Play Mode options restored.");
                }

                EditorSceneManager.playModeStartScene = null;
            }

            RefreshAllToolbarElements();
        }

        private static void OnActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            RefreshAllToolbarElements();
        }

        private static void RefreshAllToolbarElements()
        {
            MainToolbar.Refresh(PLAY_BUTTON_PATH);
            MainToolbar.Refresh(FAST_PLAY_BUTTON_PATH);
            MainToolbar.Refresh(SCENE_DROPDOWN_PATH);
        }

        private static void ShowSceneMenu(Rect dropdownRect)
        {
            var menu = new GenericMenu();

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                menu.AddDisabledItem(new GUIContent("Exit Play Mode to switch scenes"));
                menu.DropDown(dropdownRect);
                return;
            }

            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_ROOT })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .OrderBy(GetSceneLabel, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (scenePaths.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes found in Assets/Scenes"));
            }
            else
            {
                var activeScenePath = SceneManager.GetActiveScene().path;

                foreach (var scenePath in scenePaths)
                {
                    menu.AddItem(
                        new GUIContent(GetSceneLabel(scenePath)),
                        string.Equals(scenePath, activeScenePath, StringComparison.OrdinalIgnoreCase),
                        OpenScene,
                        scenePath);
                }
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Ping Current Scene in Project"), false, PingCurrentScene);

            menu.DropDown(dropdownRect);
        }

        private static string GetSceneLabel(string scenePath)
        {
            if (scenePath.StartsWith(SCENES_ROOT + "/"))
            {
                var relativePath = scenePath.Substring(SCENES_ROOT.Length + 1);
                return Path.ChangeExtension(relativePath, null).Replace("/", " > ");
            }
            return Path.GetFileNameWithoutExtension(scenePath);
        }

        private static void OpenScene(object scenePathValue)
        {
            var scenePath = scenePathValue as string;
            if (string.IsNullOrEmpty(scenePath)) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            SceneData sceneData = AssetDatabase.LoadAssetAtPath<SceneData>(SCENE_DATA_PATH);
            if (sceneData == null)
            {
                Debug.LogError($"[ToolbarSceneTools] Required SceneData asset was not found at '{SCENE_DATA_PATH}'.");
                return;
            }

            SceneType sceneType = sceneData.GetSceneByPath(scenePath);
            var dependencyPaths = new List<string>();
            if (sceneType != SceneType.Undefined && sceneData.TryGetDependencies(sceneType, out SceneReference[] dependencies))
            {
                var loadedScenePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    scenePath,
                };

                foreach (SceneReference dependency in dependencies)
                {
                    if (dependency == null)
                    {
                        Debug.LogError($"[ToolbarSceneTools] SceneData dependency for '{sceneType}' is null.");
                        return;
                    }

                    string dependencyPath = dependency.ScenePath;
                    if (string.IsNullOrEmpty(dependencyPath))
                    {
                        Debug.LogError($"[ToolbarSceneTools] SceneData dependency for '{sceneType}' has an empty scene path.");
                        return;
                    }

                    if (!loadedScenePaths.Add(dependencyPath))
                    {
                        continue;
                    }

                    SceneAsset dependencySceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(dependencyPath);
                    if (dependencySceneAsset == null)
                    {
                        Debug.LogError($"[ToolbarSceneTools] SceneData dependency '{dependencyPath}' for '{sceneType}' could not be loaded as a SceneAsset.");
                        return;
                    }

                    dependencyPaths.Add(dependencyPath);
                }
            }

            Scene targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (string dependencyPath in dependencyPaths)
            {
                EditorSceneManager.OpenScene(dependencyPath, OpenSceneMode.Additive);
            }

            if (!EditorSceneManager.SetActiveScene(targetScene))
            {
                Debug.LogError($"[ToolbarSceneTools] Failed to set target scene '{scenePath}' as active.");
            }
        }

        private static void PingCurrentScene()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path)) return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene.path);
            if (sceneAsset != null)
            {
                EditorGUIUtility.PingObject(sceneAsset);
                Selection.activeObject = sceneAsset;
            }
        }

        private static string GetFirstScenePath()
        {
            if (EditorBuildSettings.scenes != null && EditorBuildSettings.scenes.Length > 0)
            {
                string buildScenePath = EditorBuildSettings.scenes[0].path;
                if (!string.IsNullOrEmpty(buildScenePath) && File.Exists(buildScenePath))
                {
                    return buildScenePath;
                }
            }

            string defaultMainMenuPath = "Assets/Scenes/MainMenu/MainMenu.unity";
            if (File.Exists(defaultMainMenuPath))
            {
                return defaultMainMenuPath;
            }

            string[] guids = AssetDatabase.FindAssets("MainMenu t:Scene");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return null;
        }
    }
}
