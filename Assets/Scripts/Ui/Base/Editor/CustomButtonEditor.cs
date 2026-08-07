using System.Ui.Base;
using UnityEditor;
using UnityEditor.UI;

namespace UI.Base.Editor
{
    [CustomEditor(typeof(CustomButton))]
    [CanEditMultipleObjects]
    public class CustomButtonEditor : ButtonEditor
    {
        private SerializedProperty inputTypeProperty;
        private SerializedProperty buttonTextProperty;
        private SerializedProperty innerIconProperty;
        private SerializedProperty additionalTextProperty;
        private SerializedProperty tintTextAndIconProperty;
        private SerializedProperty textColorBlockProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            inputTypeProperty = serializedObject.FindProperty("inputType");
            buttonTextProperty = serializedObject.FindProperty("buttonText");
            innerIconProperty = serializedObject.FindProperty("innerIcon");
            additionalTextProperty = serializedObject.FindProperty("additionalText");
            tintTextAndIconProperty = serializedObject.FindProperty("tintTextAndIcon");
            textColorBlockProperty = serializedObject.FindProperty("textColorBlock");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Button Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(inputTypeProperty);
            EditorGUILayout.PropertyField(buttonTextProperty);
            EditorGUILayout.PropertyField(innerIconProperty);
            EditorGUILayout.PropertyField(additionalTextProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text & Icon Tint Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tintTextAndIconProperty);
            if (tintTextAndIconProperty.boolValue)
            {
                EditorGUILayout.PropertyField(textColorBlockProperty);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            // This draws the standard button properties (interactable, image, transitions, events)
            base.OnInspectorGUI(); 
        }
    }
}
