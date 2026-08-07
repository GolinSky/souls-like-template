using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SoulsLike
{
    [CreateAssetMenu(fileName = "AssetMappingData", menuName = "Data/AssetMappingData")]
    public class AssetMappingData : ScriptableObject
    {
        [Tooltip("Maps C# class name (e.g. 'SceneData') to its Addressable AssetReference. If missing, defaults to class name.")]
        [SerializeField] private SerializedDictionary<string, AssetReference> scriptableObjectMappings = new();
        
        [Tooltip("Maps C# UI component name (e.g. 'InventoryUi') to its Addressable prefab AssetReference. If missing, defaults to class name.")]
        [SerializeField] private SerializedDictionary<string, AssetReferenceGameObject> uiMappings = new();

        public string GetScriptableObjectKey(string className)
        {
            if (scriptableObjectMappings != null && scriptableObjectMappings.Dictionary.TryGetValue(className, out var mappedReference) && mappedReference != null)
            {
                if (!string.IsNullOrEmpty(mappedReference.AssetGUID))
                    return mappedReference.AssetGUID;
            }
                
            return className; // Fallback to class name
        }

        public string GetUiKey(string className)
        {
            if (uiMappings != null && uiMappings.Dictionary.TryGetValue(className, out var mappedReference) && mappedReference != null)
            {
                if (!string.IsNullOrEmpty(mappedReference.AssetGUID))
                    return mappedReference.AssetGUID;
            }
                
            return className; // Fallback to class name
        }
    }
}
