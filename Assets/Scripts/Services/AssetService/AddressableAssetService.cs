using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SoulsLike.Services.Repository
{
    public interface IAssetService
    {
        TSource Load<TSource>(string key) where TSource : Object;
        TComponent LoadComponent<TComponent>(string key) where TComponent : Component;
        GameObject LoadPrefab(string key);
    
    }
    
    public class AddressableAssetService:IAssetService
    {
        public TSource Load<TSource>(string key) where TSource : Object
        {
            TSource asset = Addressables.LoadAssetAsync<TSource>(key).WaitForCompletion();
            if (asset == null)
            {
                throw new System.InvalidOperationException($"Addressable asset for key '{key}' was not found.");
            }

            return asset;
        }

        public TComponent LoadComponent<TComponent>(string key) where TComponent : Component
        {
            TComponent component = Load<GameObject>(key).GetComponent<TComponent>();
            if (component == null)
            {
                throw new System.InvalidOperationException($"Addressable prefab for key '{key}' does not contain a {typeof(TComponent).Name} component.");
            }

            return component;
        }
        
        public GameObject LoadPrefab(string key)
        {
            return Load<GameObject>(key);
        }
    }
}
