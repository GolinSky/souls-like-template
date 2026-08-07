using UnityEngine;
using System.Collections.Generic;
using System.Ui.Base;

namespace UI.Base
{
    [System.Serializable]
    public class ButtonTypeMap
    {
        public InputTypes inputType;
        public CustomButton buttonPrefab;
    }

    [CreateAssetMenu(fileName = "CustomButtonMapping", menuName = "UI/Custom Button Mapping")]
    public class CustomButtonMapping : ScriptableObject
    {
        [Tooltip("Map each input type to its corresponding CustomButton prefab.")]
        [SerializeField] private List<ButtonTypeMap> buttonMappings = new List<ButtonTypeMap>();

        [Tooltip("The single CustomButtonToggle prefab for the project.")]
        [SerializeField] private CustomButtonToggle togglePrefab;
        
        public CustomButtonToggle TogglePrefab => togglePrefab;

        /// <summary>
        /// Retrieves the mapped prefab for the specified input type.
        /// </summary>
        public CustomButton GetPrefabForType(InputTypes type)
        {
            var match = buttonMappings.Find(x => x.inputType == type);
            return match?.buttonPrefab;
        }

#if UNITY_EDITOR
        public IReadOnlyList<ButtonTypeMap> Editor_GetMappings() => buttonMappings;
#endif
    }
}
