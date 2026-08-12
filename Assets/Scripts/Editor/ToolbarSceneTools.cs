using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SoulsLike.EditorTools
{
    /// <summary>
    /// Unity Editor Toolbar extension that provides:
    /// 1. Game Entry Play Button: Starts Play Mode using scene index 0 (MainMenu).
    /// 2. Scenes Dropdown: Dynamic list of all scenes in Assets/Scenes to switch scenes quickly.
    /// </summary>
    [InitializeOnLoad]
    public static class ToolbarSceneTools
    {
        private static ScriptableObject m_CurrentToolbar;

        static ToolbarSceneTools()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Reset playModeStartScene so standard Unity play mode works normally when initiated elsewhere
                EditorSceneManager.playModeStartScene = null;
            }
        }

        private static void OnUpdate()
        {
            if (m_CurrentToolbar != null) return;

            // Locate UnityEditor.Toolbar instance via reflection
            Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return;

            UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars == null || toolbars.Length == 0) return;

            m_CurrentToolbar = toolbars[0] as ScriptableObject;
            if (m_CurrentToolbar == null) return;

            FieldInfo rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null) return;

            VisualElement mRoot = rootField.GetValue(m_CurrentToolbar) as VisualElement;
            if (mRoot == null) return;

            // Search for left alignment zone, right alignment zone, or fallback to root
            VisualElement targetZone = mRoot.Q("ToolbarZoneLeftAlign")
                                    ?? mRoot.Q(className: "unity-toolbar-zone-left")
                                    ?? mRoot.Q("ToolbarZoneRightAlign")
                                    ?? mRoot.Q(className: "unity-toolbar-zone-right")
                                    ?? mRoot;

            if (targetZone == null) return;

            // Prevent duplicate toolbar elements injection
            if (targetZone.Q("SoulsLikeToolbarTools") != null) return;

            IMGUIContainer container = new IMGUIContainer(OnGUI)
            {
                name = "SoulsLikeToolbarTools",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = 8,
                    marginRight = 8
                }
            };

            targetZone.Add(container);
        }

        private static void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // 1. Game Entry Play Button (Starts index 0 / MainMenu)
            DrawGameEntryPlayButton();

            GUILayout.Space(6);

            // 2. Scene Selector Dropdown (Assets/Scenes)
            DrawSceneSelectionDropdown();

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders the Game Entry Play button in the Unity Editor Toolbar.
        /// </summary>
        private static void DrawGameEntryPlayButton()
        {
            bool isPlaying = EditorApplication.isPlaying;

            GUIContent playContent;
            if (isPlaying)
            {
                playContent = new GUIContent(" ⏹ Stop Game", "Stop Play Mode");
            }
            else
            {
                playContent = new GUIContent(" ▶ Play MainMenu (0)", "Save current scene and start Game from scene index 0 (MainMenu)");
            }

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Color originalColor = GUI.backgroundColor;
            if (isPlaying)
            {
                GUI.backgroundColor = new Color(1.0f, 0.45f, 0.45f); // Red tint while playing
            }
            else
            {
                GUI.backgroundColor = new Color(0.4f, 0.9f, 0.55f); // Soft green tint for Play MainMenu
            }

            if (GUILayout.Button(playContent, buttonStyle, GUILayout.Height(22), GUILayout.MinWidth(140)))
            {
                if (isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
                else
                {
                    StartGameFromFirstScene();
                }
            }

            GUI.backgroundColor = originalColor;
        }

        /// <summary>
        /// Renders the Scenes Dropdown menu in the Unity Editor Toolbar.
        /// </summary>
        private static void DrawSceneSelectionDropdown()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            string currentSceneName = string.IsNullOrEmpty(activeScene.name) ? "Untitled Scene" : activeScene.name;

            GUIContent dropdownContent = new GUIContent($" 🎬 Scene: {currentSceneName} ▾", "Select a scene from Assets/Scenes");

            Rect rect = GUILayoutUtility.GetRect(dropdownContent, EditorStyles.toolbarDropDown, GUILayout.Height(22), GUILayout.MinWidth(150));

            if (EditorGUI.DropdownButton(rect, dropdownContent, FocusType.Keyboard, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();

                // Locate all scene assets in Assets/Scenes
                string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

                if (guids == null || guids.Length == 0)
                {
                    menu.AddDisabledItem(new GUIContent("No scenes found in Assets/Scenes"));
                }
                else
                {
                    var scenePaths = guids.Select(AssetDatabase.GUIDToAssetPath).Distinct().OrderBy(p => p).ToList();

                    foreach (string scenePath in scenePaths)
                    {
                        // Clean display name (e.g. "MainMenu/MainMenu" or "DefaultLocation/Blueprints")
                        string displayName = scenePath;
                        if (displayName.StartsWith("Assets/Scenes/"))
                        {
                            displayName = displayName.Substring("Assets/Scenes/".Length);
                        }
                        if (displayName.EndsWith(".unity"))
                        {
                            displayName = displayName.Substring(0, displayName.Length - ".unity".Length);
                        }

                        bool isActive = string.Equals(activeScene.path, scenePath, StringComparison.OrdinalIgnoreCase);

                        string targetPath = scenePath;
                        menu.AddItem(new GUIContent(displayName), isActive, () =>
                        {
                            SwitchToScene(targetPath);
                        });
                    }
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Ping Current Scene in Project"), false, PingCurrentScene);

                menu.DropDown(rect);
            }
        }

        /// <summary>
        /// Launches the game starting from Scene Index 0 (MainMenu).
        /// </summary>
        [MenuItem("SoulsLike/Play Game (MainMenu - Index 0) _F5", false, 100)]
        public static void StartGameFromFirstScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // Prompt user to save modified scenes
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

            Debug.Log($"[ToolbarSceneTools] Starting Play Mode with scene index 0: {firstScenePath}");
            EditorSceneManager.playModeStartScene = sceneAsset;
            EditorApplication.isPlaying = true;
        }

        /// <summary>
        /// Switches to the specified scene path after prompting to save modifications.
        /// </summary>
        public static void SwitchToScene(string scenePath)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[ToolbarSceneTools] Cannot switch scenes while in Play Mode. Exit Play Mode first.");
                return;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (string.Equals(activeScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Debug.Log($"[ToolbarSceneTools] Opening scene: {scenePath}");
            EditorSceneManager.OpenScene(scenePath);
        }

        /// <summary>
        /// Selects and pings the active scene file in the Project window.
        /// </summary>
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

        /// <summary>
        /// Retrieves scene index 0 path from EditorBuildSettings or fallbacks to MainMenu path.
        /// </summary>
        private static string GetFirstScenePath()
        {
            // 1. Try index 0 from Build Settings
            if (EditorBuildSettings.scenes != null && EditorBuildSettings.scenes.Length > 0)
            {
                string buildScenePath = EditorBuildSettings.scenes[0].path;
                if (!string.IsNullOrEmpty(buildScenePath) && File.Exists(buildScenePath))
                {
                    return buildScenePath;
                }
            }

            // 2. Default path Assets/Scenes/MainMenu/MainMenu.unity
            string defaultMainMenuPath = "Assets/Scenes/MainMenu/MainMenu.unity";
            if (File.Exists(defaultMainMenuPath))
            {
                return defaultMainMenuPath;
            }

            // 3. Find any scene named MainMenu under Assets/
            string[] guids = AssetDatabase.FindAssets("MainMenu t:Scene");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return null;
        }
    }
}
