#if UNITY_EDITOR
using System.Reflection;
using System.Ui.Base;
using NUnit.Framework;
using SoulsLike.Ui.PauseNavigation;
using SoulsLike.Ui.Status;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoulsLike.Editor.Tests.Status
{
    public sealed class StatusUiConfigurationTests
    {
        private const string STATUS_UI_PREFAB_PATH = "Assets/Prefabs/Ui/Status/StatusUi.prefab";
        private const string PAUSE_UI_PREFAB_PATH = "Assets/Prefabs/Ui/PauseNavigation/PauseNavigationUi.prefab";

        [Test]
        public void StatusUiPrefab_HasRequiredBindings()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(STATUS_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, STATUS_UI_PREFAB_PATH);

            StatusUi statusUi = prefab.GetComponent<StatusUi>();
            Assert.That(statusUi, Is.Not.Null, STATUS_UI_PREFAB_PATH);
            AssertReferences(
                statusUi,
                "canvasGroup",
                "contentRoot",
                "characterNameText",
                "levelText",
                "heldRunesText",
                "nextLevelRunesText",
                "vigorText",
                "mindText",
                "enduranceText",
                "strengthText",
                "dexterityText",
                "intelligenceText",
                "faithText",
                "arcaneText",
                "healthText",
                "focusText",
                "staminaText",
                "equipLoadText",
                "equipLoadFill",
                "loadClassText",
                "poiseText",
                "discoveryText",
                "memorySlotsText",
                "memoryEmptyStateText",
                "spellBlockRoot",
                "combatDetailsRoot",
                "helpPanel",
                "helpText",
                "backButton",
                "simpleViewButton",
                "helpButton");
            AssertReferenceArray(statusUi, "armamentAttackTexts", 6);
            AssertReferenceArray(statusUi, "defenseTexts", 8);
            AssertReferenceArray(statusUi, "negationTexts", 8);
            AssertReferenceArray(statusUi, "resistanceTexts", 4);
        }

        [Test]
        public void PauseNavigationUiPrefab_HasTypedStatusButton()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PAUSE_UI_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, PAUSE_UI_PREFAB_PATH);

            PauseNavigationUi pauseUi = prefab.GetComponent<PauseNavigationUi>();
            Assert.That(pauseUi, Is.Not.Null, PAUSE_UI_PREFAB_PATH);
            AssertReferences(pauseUi, "openStatusButton");
        }

        [Test]
        public void PauseNavigationUiShow_RestoresFocusFromStatusToPauseRoot()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            EventSystem previousEventSystem = EventSystem.current;
            GameObject previousSelection = previousEventSystem == null
                ? null
                : previousEventSystem.currentSelectedGameObject;
            EventSystem eventSystem = null;
            MethodInfo onEnable = typeof(EventSystem).GetMethod(
                "OnEnable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo onDisable = typeof(EventSystem).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                var eventSystemObject = new GameObject("PauseNavigationEventSystem");
                eventSystemObject.SetActive(false);
                SceneManager.MoveGameObjectToScene(eventSystemObject, previewScene);
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
                onEnable.Invoke(eventSystem, null);
                EventSystem.current = eventSystem;

                var statusButtonObject = new GameObject("StatusFooterButton");
                SceneManager.MoveGameObjectToScene(statusButtonObject, previewScene);
                CustomButton statusButton = statusButtonObject.AddComponent<CustomButton>();

                var pauseRoot = new GameObject("PauseNavigationRoot");
                SceneManager.MoveGameObjectToScene(pauseRoot, previewScene);
                pauseRoot.SetActive(false);
                CanvasGroup canvasGroup = pauseRoot.AddComponent<CanvasGroup>();
                PauseNavigationUi pauseUi = pauseRoot.AddComponent<PauseNavigationUi>();
                var pauseButtonObject = new GameObject("PauseStatusButton");
                SceneManager.MoveGameObjectToScene(pauseButtonObject, previewScene);
                pauseButtonObject.transform.SetParent(pauseRoot.transform);
                CustomButton pauseButton = pauseButtonObject.AddComponent<CustomButton>();
                AssignReference(pauseUi, "canvasGroup", canvasGroup);
                AssignReference(pauseUi, "openStatusButton", pauseButton);
                pauseRoot.SetActive(true);

                statusButton.Select();
                Assert.That(eventSystem.currentSelectedGameObject, Is.EqualTo(statusButtonObject));

                pauseUi.Show();

                Assert.That(eventSystem.currentSelectedGameObject, Is.EqualTo(pauseButtonObject));
            }
            finally
            {
                if (eventSystem != null)
                {
                    onDisable.Invoke(eventSystem, null);
                }

                if (previousEventSystem != null)
                {
                    EventSystem.current = previousEventSystem;
                    previousEventSystem.SetSelectedGameObject(previousSelection);
                }

                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        [Test]
        public void CustomButton_DoesNotSubmitWhenParentCanvasGroupIsNotInteractable()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                var rootObject = new GameObject("CustomButtonRoot");
                SceneManager.MoveGameObjectToScene(rootObject, previewScene);
                CanvasGroup canvasGroup = rootObject.AddComponent<CanvasGroup>();
                var buttonObject = new GameObject("CustomButton");
                SceneManager.MoveGameObjectToScene(buttonObject, previewScene);
                buttonObject.transform.SetParent(rootObject.transform);
                CustomButton button = buttonObject.AddComponent<CustomButton>();
                MethodInfo onCanvasGroupChanged = typeof(Selectable).GetMethod(
                    "OnCanvasGroupChanged",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                onCanvasGroupChanged.Invoke(button, null);
                int submitCount = 0;
                button.onClick.AddListener(() => submitCount++);

                button.OnSubmit(new BaseEventData(null));

                Assert.That(submitCount, Is.EqualTo(1));
                canvasGroup.interactable = false;
                onCanvasGroupChanged.Invoke(button, null);

                button.OnSubmit(new BaseEventData(null));

                Assert.That(submitCount, Is.EqualTo(1));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void AssertReferences(Component component, params string[] propertyNames)
        {
            using var serialized = new SerializedObject(component);
            foreach (string propertyName in propertyNames)
            {
                Assert.That(
                    serialized.FindProperty(propertyName).objectReferenceValue,
                    Is.Not.Null,
                    $"{component.GetType().Name} requires '{propertyName}'.");
            }
        }

        private static void AssignReference(
            Component component,
            string propertyName,
            Object reference)
        {
            using var serialized = new SerializedObject(component);
            serialized.FindProperty(propertyName).objectReferenceValue = reference;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssertReferenceArray(
            Component component,
            string propertyName,
            int expectedSize)
        {
            using var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property.arraySize, Is.EqualTo(expectedSize));
            for (int index = 0; index < property.arraySize; index++)
            {
                Assert.That(property.GetArrayElementAtIndex(index).objectReferenceValue, Is.Not.Null);
            }
        }
    }
}
#endif
