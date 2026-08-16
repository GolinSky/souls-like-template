using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using SoulsLike.Entities.Character;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public class CoreScope : LifetimeScope
    {
        [SerializeField] private CameraService cameraService;
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("CoreScope Configure");
            builder.RegisterComponent(cameraService).AsSelf().As<ICameraService>();
            builder.Register<TargetingService>(Lifetime.Singleton).As<ITargetingService>();
            builder.Register<CharacterFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CoreGameOrchestrator>();
        }
    }
}
