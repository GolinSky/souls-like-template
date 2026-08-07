using SoulsLike.Services;
using SoulsLike.Services.CameraService;
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
            builder.Register<PauseMenuUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterEntryPoint<CoreGameOrchestrator>();
        }
    }
}
