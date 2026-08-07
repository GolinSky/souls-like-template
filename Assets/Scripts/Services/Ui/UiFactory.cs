using MultiPlayerTemplate.Factory;
using MultiPlayerTemplate.Ui.Base;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Exception = System.Exception;

namespace MultiPlayerTemplate.Services
{
    public class UiFactory: BaseFactory
    {
        public UiFactory(IObjectResolver resolver): base(resolver)
        {
        }
        
       
        public TUi CreateUi<TUi>(Transform parent)
            where TUi : IBaseUi
        {
            var uiInstance = CreateUiInstance<TUi>(parent);

            var childScope = RootScope.CreateChild(builder =>
            {
                builder.RegisterComponentInHierarchy<TUi>().AsImplementedInterfaces(); 
            });

            return uiInstance;
        }
        

        private AssetMappingData _mappingData;
        private AssetMappingData MappingData
        {
            get
            {
                if (_mappingData == null)
                {
                    _mappingData = AssetService.Load<AssetMappingData>("AssetMappingData");
                    if (_mappingData == null)
                    {
                        Debug.LogError("[UiFactory] AssetMappingData is null! Failed to load mapping data asset.");
                    }
                }
                return _mappingData;
            }
        }

        private TUi CreateUiInstance<TUi>(Transform parent)
        {
            var className = typeof(TUi).Name;
            var mapping = MappingData;
            var addressableKey = className;
            if (mapping != null)
            {
                addressableKey = mapping.GetUiKey(className);
            }
            else
            {
                Debug.LogError($"[UiFactory] MappingData is missing while creating UI instance for {className}!");
            }

            var prefab = AssetService.Load<GameObject>(addressableKey);
            if (prefab == null)
                throw new Exception($"UI prefab for Addressables key '{addressableKey}' not found.");

            var instance = Object.Instantiate(prefab, parent);
            instance.name = $"{className}_Instance";

            var ui = instance.GetComponent<TUi>();
            if (ui == null)
                throw new Exception($"Prefab for '{className}' does not contain a component of type {typeof(TUi).Name}.");

            return ui;
        }
    }
}