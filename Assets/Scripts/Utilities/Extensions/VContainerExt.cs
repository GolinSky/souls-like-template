using MultiPlayerTemplate.Services.Repository;
using UnityEngine;
using VContainer;

namespace MultiPlayerTemplate.Extensions
{
    public static class VContainerExt
    {
        private static readonly AddressableAssetService AssetService = new AddressableAssetService();
        private static AssetMappingData _mappingData;
 
        private static AssetMappingData MappingData 
        {
            get 
            {
                if (_mappingData == null)
                {
                    _mappingData = AssetService.Load<AssetMappingData>("AssetMappingData");
                    if (_mappingData == null)
                    {
                        Debug.LogWarning("[VContainerExt] AssetMappingData not found in Addressables under key 'AssetMappingData'. Falling back to exact class names.");
                    }
                }
                return _mappingData;
            }
        }

        public static RegistrationBuilder RegisterScriptableObject<TImpl>(this IContainerBuilder builder)
            where TImpl : Object
        {
            var className = typeof(TImpl).Name;
            var key = MappingData != null ? MappingData.GetScriptableObjectKey(className) : className;
            return RegisterScriptableObjectInternal<TImpl>(builder, key);
        }

        public static RegistrationBuilder RegisterScriptableObject<TImpl>(this IContainerBuilder builder, string key)
            where TImpl : Object
        {
            return RegisterScriptableObjectInternal<TImpl>(builder, key);
        }

        private static RegistrationBuilder RegisterScriptableObjectInternal<TImpl>(
            IContainerBuilder builder,
            string key
        )
            where TImpl : Object
        {
            var instance = AssetService.Load<TImpl>(key);
            return builder.RegisterInstance(instance).AsSelf();
        }
    }

}