using SoulsLike.Services.Repository;
using SoulsLike.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public class SharedSceneScope : LifetimeScope
    {
        [Header("Can be null")]
        [SerializeField] private UiService uiService;
        [SerializeField] private PreviewRenderService previewRenderService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UiFactory>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            if (uiService == null)
            {
                Debug.LogWarning($"uiService field is null. Trying to load prefab from addressables");
                IAssetService assetService = Parent.Container.Resolve<IAssetService>();
                UiService uiServicePrefab = assetService.LoadComponent<UiService>(nameof(UiService));
                builder.RegisterComponentInNewPrefab(uiServicePrefab, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterComponent(uiService).As<IUiService>();
            }

            // if (previewRenderService == null)
            // {
            //     Debug.LogWarning($"previewRenderService field is null. Trying to load prefab from addressables");
            //     IAssetService assetService = Parent.Container.Resolve<IAssetService>();
            //     PreviewRenderService previewRenderServicePrefab = assetService.LoadComponent<PreviewRenderService>(nameof(PreviewRenderService));
            //     builder.RegisterComponentInNewPrefab(previewRenderServicePrefab, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            // }
            // else
            // {
            //     builder.RegisterComponent(previewRenderService).As<IPreviewRenderService>();
            // }
        }
    }
}