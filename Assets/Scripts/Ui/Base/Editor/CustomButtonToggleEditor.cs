using UnityEditor;
using UnityEditor.UI;

namespace UI.Base.Editor
{
    [CustomEditor(typeof(CustomButtonToggle))]
    [CanEditMultipleObjects]
    public class CustomButtonToggleEditor : ToggleEditor
    {
        private SerializedProperty toggleTextProperty;
        private SerializedProperty additionalIconProperty;
        private SerializedProperty tintTextAndIconProperty;
        private SerializedProperty textColorBlockProperty;
        private SerializedProperty enableWhenOnProperty;
        private SerializedProperty enableWhenOffProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            toggleTextProperty = serializedObject.FindProperty("toggleText");
            additionalIconProperty = serializedObject.FindProperty("additionalIcon");
            tintTextAndIconProperty = serializedObject.FindProperty("tintTextAndIcon");
            textColorBlockProperty = serializedObject.FindProperty("textColorBlock");
            enableWhenOnProperty = serializedObject.FindProperty("enableWhenOn");
            enableWhenOffProperty = serializedObject.FindProperty("enableWhenOff");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Toggle Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(toggleTextProperty);
            EditorGUILayout.PropertyField(additionalIconProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text & Icon Tint Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tintTextAndIconProperty);
            if (tintTextAndIconProperty.boolValue)
            {
                EditorGUILayout.PropertyField(textColorBlockProperty);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("State Graphics Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableWhenOnProperty, true);
            EditorGUILayout.PropertyField(enableWhenOffProperty, true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            // This draws the standard toggle properties (interactable, transition, graphic, group, etc.)
            base.OnInspectorGUI(); 
        }
    }
}
