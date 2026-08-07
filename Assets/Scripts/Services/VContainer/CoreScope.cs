using MultiPlayerTemplate.Services;
using MultiPlayerTemplate.Services.CameraService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MultiPlayerTemplate
{
    public class CoreScope : LifetimeScope
    {
        [SerializeField] private CameraService cameraService;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(cameraService).AsSelf().As<ICameraService>();
            builder.UseEntryPoints(Lifetime.Singleton, pointsBuilder =>
            {
                pointsBuilder.Add<CoreGameOrchestrator>();
            });
        }
    }
}
