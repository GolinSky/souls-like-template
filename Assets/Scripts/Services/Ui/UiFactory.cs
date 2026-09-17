using SoulsLike.Factory;
using SoulsLike.Services.Repository;
using SoulsLike.Ui.Base;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Exception = System.Exception;

namespace SoulsLike.Services
{
    public class UiFactory: BaseFactory
    {
        public UiFactory(IObjectResolver resolver, IAssetService assetService): base(resolver, assetService)
        {
        }
        
       
        public TUi CreateUi<TUi>(Transform parent)
            where TUi : IBaseUi
        {
            var uiInstance = CreateUiInstance<TUi>(parent);

            RootScope.CreateChild(builder =>
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
                }
                return _mappingData;
            }
        }

        private TUi CreateUiInstance<TUi>(Transform parent)
        {
            var className = typeof(TUi).Name;
            var addressableKey = MappingData.GetUiKey(className);

            var prefab = AssetService.Load<GameObject>(addressableKey);
            var instance = Object.Instantiate(prefab, parent);
            instance.name = $"{className}_Instance";

            var ui = instance.GetComponent<TUi>();
            if (ui == null)
                throw new Exception($"Prefab for '{className}' does not contain a component of type {typeof(TUi).Name}.");

            return ui;
        }
    }
}
