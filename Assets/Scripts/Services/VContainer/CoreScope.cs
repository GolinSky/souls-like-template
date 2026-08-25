using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Enemy;
using SoulsLike.Interactions;
using SoulsLike.Ui.EnemyHealth;
using SoulsLike.Ui.Grace;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public class CoreScope : LifetimeScope
    {
        [SerializeField] private CameraService cameraService;
        [SerializeField] private GraceSystem graceSystem;
        [SerializeField] private EnemyEncounterSystem enemyEncounterSystem;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(cameraService).AsSelf().As<ICameraService>();
            builder.RegisterComponent(graceSystem).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponent(enemyEncounterSystem).AsSelf().AsImplementedInterfaces();
            builder.Register<TargetingService>(Lifetime.Singleton).As<ITargetingService>();
            builder.Register<EnemyHealthUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GraceUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<CharacterFactory>(Lifetime.Singleton);
            builder.Register<EnemyFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CoreGameOrchestrator>();
        }
    }
}
