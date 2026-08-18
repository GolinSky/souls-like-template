using SoulsLike.Extensions;
using SoulsLike.Services;
using SoulsLike.Services.Audio;
using SoulsLike.Services.Audio.Data;
using SoulsLike.Services.Repository;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Storage;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Layer.Data;
using SoulsLike.Ui.FpsCounter;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public class ProjectScope: LifetimeScope
    {
        [Header("UI Components")]
        [SerializeField] private OnGuiFpsCounter fpsCounter;

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
            
            // audio system
            builder.RegisterScriptableObject<AudioData>();
            builder.RegisterScriptableObject<AmbienceData>();
            builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AmbienceService>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<AddressableAssetService>(Lifetime.Singleton).As<IAssetService>();
            builder.Register<StorageRegistry>(Lifetime.Singleton).As<IStorageRegistry>();

            // Wire OnGuiFpsCounter in ProjectScope
            if (fpsCounter != null)
            {
                builder.RegisterComponent(fpsCounter);
            }
            else
            {
                builder.RegisterComponentOnNewGameObject<OnGuiFpsCounter>(Lifetime.Singleton, "OnGUI_FPS_Counter");
            }
        }
    }
}
