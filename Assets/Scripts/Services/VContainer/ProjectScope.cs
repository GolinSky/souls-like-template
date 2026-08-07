using MultiPlayerTemplate.Extensions;
using MultiPlayerTemplate.Services;
using MultiPlayerTemplate.Services.Repository;
using MultiPlayerTemplate.Services.Scenes;
using MultiPlayerTemplate.Services.Scenes.Data;
using MultiPlayerTemplate.Services.Storage;
using MultiPlayerTemplate.Services.Layer;
using MultiPlayerTemplate.Services.Layer.Data;
using VContainer;
using VContainer.Unity;

namespace MultiPlayerTemplate
{
    public class ProjectScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameOrchestrator>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<InputService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            // scene system
            builder.RegisterScriptableObject<SceneData>();
            builder.Register<SceneModel>(Lifetime.Singleton).AsSelf();
            builder.Register<SceneService>(Lifetime.Singleton).As<ISceneService>();
            
            // layer system
            builder.RegisterScriptableObject<LayerData>();
            builder.Register<LayerService>(Lifetime.Singleton).As<ILayerService>();
            
            builder.Register<AddressableAssetService>(Lifetime.Singleton).As<IAssetService>();
            builder.Register<StorageRegistry>(Lifetime.Singleton).As<IStorageRegistry>();
            
        }
    }
}