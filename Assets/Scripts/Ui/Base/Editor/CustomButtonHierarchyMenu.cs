
using System.Ui.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Base.Editor
{
    public static class CustomButtonHierarchyMenu
    {
        // Finds the first mapping asset in the project.
        public static CustomButtonMapping TryGetMappingAsset()
        {
            CustomButtonMapping mapping = null;
            string[] guids = AssetDatabase.FindAssets("t:CustomButtonMapping");
            if (guids.Length > 0)
            {
                mapping = AssetDatabase.LoadAssetAtPath<CustomButtonMapping>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return mapping;
        }

        public static void InstantiatePrefabInContext(GameObject prefab, GameObject context = null)
        {
            // Attempt to instantiate the prefab preserving its exact structure
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            
            if (context == null || context.GetComponentInParent<Canvas>() == null)
            {
                // Fallback: If clicked outside a Canvas, find a Canvas in the scene
                Canvas primaryCanvas = Object.FindAnyObjectByType<Canvas>();
                if (primaryCanvas == null)
                {
                    // Create new Canvas if absolutely none exists in scene
                    GameObject canvasObj = new GameObject("Canvas");
                    primaryCanvas = canvasObj.AddComponent<Canvas>();
                    primaryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                    Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

                    if (Object.FindAnyObjectByType<EventSystem>() == null)
                    {
                        GameObject esObj = new GameObject("EventSystem");
                        esObj.AddComponent<EventSystem>();
                        esObj.AddComponent<StandaloneInputModule>();
                        Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
                    }
                }
                context = primaryCanvas.gameObject;
            }

            GameObjectUtility.SetParentAndAlign(instance, context);
            
            // Give unique name and register undo
            instance.name = prefab.name;
            GameObjectUtility.EnsureUniqueNameForSibling(instance);
            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }

        private static void InstantiatePrefabInContext(GameObject prefab, MenuCommand menuCommand)
        {
            GameObject context = menuCommand.context as GameObject;
            InstantiatePrefabInContext(prefab, context);
        }

        public static void CreateButton(InputTypes buttonType, GameObject context = null)
        {
            CustomButtonMapping mapping = TryGetMappingAsset();
            if (mapping == null)
            {
                Debug.LogError("No CustomButtonMapping asset found in the project! Please create one via right-click 'Create -> UI -> Custom Button Mapping' in your Assets folder and configure your prefabs.");
                return;
            }

            CustomButton prefab = mapping.GetPrefabForType(buttonType);
            if (prefab == null)
            {
                Debug.LogError($"No prefab assigned for button type '{buttonType}' in the CustomButtonMapping asset!");
                return;
            }

            InstantiatePrefabInContext(prefab.gameObject, context);
        }

        private static void CreateButton(MenuCommand menuCommand, InputTypes buttonType)
        {
            GameObject context = menuCommand.context as GameObject;
            CreateButton(buttonType, context);
        }

        public static void CreateToggleButton(GameObject context = null)
        {
            CustomButtonMapping mapping = TryGetMappingAsset();
            if (mapping == null)
            {
                Debug.LogError("No CustomButtonMapping asset found in the project!");
                return;
            }

            CustomButtonToggle prefab = mapping.TogglePrefab;
            if (prefab == null)
            {
                Debug.LogError("No toggle prefab assigned in the CustomButtonMapping asset!");
                return;
            }

            InstantiatePrefabInContext(prefab.gameObject, context);
        }

        [MenuItem("GameObject/UI(CustomCanvas)/Buttons/Toggle", false, 12)]
        private static void CreateToggleButton(MenuCommand menuCommand)
        {
            CreateToggleButton(menuCommand.context as GameObject);
        }

        [MenuItem("GameObject/UI(CustomCanvas)/Buttons/Primary", false, 8)]
        private static void CreatePrimaryButton(MenuCommand menuCommand)
        {
            CreateButton(menuCommand, InputTypes.Primary);
        }

        [MenuItem("GameObject/UI(CustomCanvas)/Buttons/Secondary", false, 9)]
        private static void CreateSecondaryButton(MenuCommand menuCommand)
        {
            CreateButton(menuCommand, InputTypes.Secondary);
        }

        [MenuItem("GameObject/UI(CustomCanvas)/Buttons/Destructive", false, 10)]
        private static void CreateDestructiveButton(MenuCommand menuCommand)
        {
            CreateButton(menuCommand, InputTypes.Destructive);
        }

        [MenuItem("GameObject/UI(CustomCanvas)/Buttons/Dismiss", false, 11)]
        private static void CreateDismissButton(MenuCommand menuCommand)
        {
            CreateButton(menuCommand, InputTypes.Dismiss);
        }
    }
}
