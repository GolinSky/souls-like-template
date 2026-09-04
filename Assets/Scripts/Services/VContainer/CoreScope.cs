using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Navigation;
using SoulsLike.Services.Spawn;
using SoulsLike.Services.Targeting;
using SoulsLike.Services.Travel;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Enemy;
using SoulsLike.Interactions;
using SoulsLike.Ui.EnemyHealth;
using SoulsLike.Ui.Cheats;
using SoulsLike.Ui.Grace;
using SoulsLike.Ui.Travel;
using SoulsLike.Ui.Settings;
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
        [SerializeField] private PlayerSpawnPositionProvider playerSpawnPositionProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(cameraService).AsSelf().As<ICameraService>();
            builder.RegisterComponent(graceSystem).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponent(enemyEncounterSystem).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponent(playerSpawnPositionProvider).AsSelf();
            builder.Register<TargetingService>(Lifetime.Singleton).As<ITargetingService>();
            builder.Register<EnemyHealthUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<CheatsUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<TravelService>(Lifetime.Singleton);
            builder.Register<TravelUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GraceUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SettingsUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<CharacterFactory>(Lifetime.Singleton);
            builder.Register<NavMeshService>(Lifetime.Singleton).As<INavMeshService>();
            builder.Register<EnemyFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CoreGameOrchestrator>();
        }
    }
}
