using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiPlayerTemplate.Services.Repository
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
            return Addressables.LoadAssetAsync<TSource>(key).WaitForCompletion();
        }

        public TComponent LoadComponent<TComponent>(string key) where TComponent : Component
        {
            return Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion().GetComponent<TComponent>();
        }
        
        public GameObject LoadPrefab(string key)
        {
            return Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion();
        }
    }
}