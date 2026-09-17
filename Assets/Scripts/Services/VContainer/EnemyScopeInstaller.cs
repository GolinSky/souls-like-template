using SoulsLike.Components.Visibility;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Targeting;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using SoulsLike.Entities.Ladder;
using SoulsLike.Extensions;
using SoulsLike.Items;
using SoulsLike.Ui.EnemyHealth;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Services.VContainer
{
    public sealed class EnemyScopeInstaller : EntityLifetimeScope
    {
        private EnemySpawnPoint _spawn;
        private EnemyGroupCoordinator _groupCoordinator;

        public void ConfigureEnemy(
            EnemySpawnPoint spawn,
            EnemyGroupCoordinator groupCoordinator)
        {
            _spawn = spawn;
            _groupCoordinator = groupCoordinator;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPointExceptionHandler(exception => throw exception);

            // Entity identity and actor components.
            builder.RegisterEntitySystemExt(EntityType.Enemy);
            builder.RegisterInstance(_spawn.HealthData).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(_spawn.BehaviourProfile);
            builder.RegisterInstance(_spawn.Moveset);
            builder.RegisterInstance(_groupCoordinator);
            builder.RegisterInstance(new EnemyDespawnHandler(this)).As<IEnemyDespawnHandler>();
            builder.RegisterComponentInHierarchy<EnemyActor>().UnderTransform(transform).AsSelf();
            builder.RegisterComponentInHierarchy<ViewEntity>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<TargetLockComponent>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();

            // Health and presentation.
            builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<HealthComponent>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<CombatDefenseComponent>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<VisibilityComponent>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<EnemyHealthUiComponent>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();

            // Navigation, combat, and authored data.
            builder.RegisterScriptableObject<WeaponDatabase>();
            builder.RegisterComponentInHierarchy<EnemyNavigationMotor>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<LadderClimber>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<EnemyActionExecutor>().UnderTransform(transform).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<MeleeHitboxController>().UnderTransform(transform).AsSelf();
            if (GetComponentsInChildren<EnemyActivationTrigger>(true).Length == 1)
            {
                builder.RegisterComponentInHierarchy<EnemyActivationTrigger>().UnderTransform(transform).AsSelf();
            }

            // Entity commands and AI orchestration.
            builder.Register<ApplyDamageCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ResolveMeleeHitCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<CriticalTargetCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<TargetingCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlatformRideCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<EnemyPerception>(Lifetime.Singleton).AsSelf();
            builder.Register<EnemyRandomStreams>(Lifetime.Singleton).AsSelf();
            builder.Register<EnemyActionSelector>(Lifetime.Singleton).AsSelf();
            builder.Register<EnemyController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
