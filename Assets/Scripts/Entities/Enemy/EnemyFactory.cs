using System;
using SoulsLike.Components.Visibility;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Targeting;
using SoulsLike.Entities.Combat;
using SoulsLike.Extensions;
using SoulsLike.Factory;
using SoulsLike.Items;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Navigation;
using SoulsLike.Entities.Ladder;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyFactory : BaseFactory
    {
        private readonly INavMeshService _navMeshService;

        public EnemyFactory(IObjectResolver resolver, INavMeshService navMeshService)
            : base(resolver)
        {
            _navMeshService = navMeshService;
        }

        public EnemyActor CreateEnemy(EnemySpawnPoint spawn, EnemyGroupCoordinator groupCoordinator)
        {
            EnemyActor prefab = spawn.EnemyPrefab;
            NavMeshAgent prefabAgent = prefab.NavMeshAgent;
            NavMeshQueryFilter queryFilter = new()
            {
                agentTypeID = prefabAgent.agentTypeID,
                areaMask = prefabAgent.areaMask
            };
            bool hasSpawnPosition = _navMeshService.TrySamplePosition(
                spawn.transform.position,
                prefabAgent.radius,
                queryFilter,
                out NavMeshHit spawnHit);
            if (!hasSpawnPosition)
            {
                hasSpawnPosition = _navMeshService.TrySampleNearestPosition(
                    spawn.transform.position,
                    queryFilter,
                    out spawnHit);
            }

            if (!hasSpawnPosition)
            {
                throw new InvalidOperationException(
                    $"No compatible baked NavMesh could be found for enemy spawn point "
                    + $"'{spawn.name}'.");
            }

            EnemyActivationTrigger[] activationTriggers =
                prefab.GetComponentsInChildren<EnemyActivationTrigger>(true);
            bool hasActivationTrigger = activationTriggers.Length == 1;
            long entityId = RootScope.Container
                .Resolve<IUniqueIdGenerator>()
                .GenerateUniqueId();

            LifetimeScope scope = RootScope.CreateChild(builder =>
            {
                Func<IObjectResolver, Transform> actorTransform =
                    resolver => resolver.Resolve<EnemyActor>().Transform;

                builder.RegisterEntitySystemExt(EntityType.Enemy, entityId);
                builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped)
                    .UnderTransform(resolver => resolver.Resolve<LifetimeScope>().transform)
                    .WithParameter(spawnHit.position)
                    .WithParameter(spawn.transform.rotation)
                    .WithParameter(spawn.BuildPatrolPositions())
                    .WithParameter(spawn.RandomSeedOffset)
                    .AsSelf();
                builder.RegisterComponentInHierarchy<ViewEntity>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<TargetLockComponent>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();

                builder.RegisterInstance(spawn.HealthData).AsImplementedInterfaces().AsSelf();
                builder.RegisterInstance(spawn.BehaviourProfile);
                builder.RegisterInstance(spawn.Moveset);
                builder.RegisterInstance(groupCoordinator);
                    
                builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponentInHierarchy<HealthComponent>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<CombatDefenseComponent>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<VisibilityComponent>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<EnemyHealthUiComponent>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();

                builder.RegisterScriptableObject<WeaponDatabase>();

                builder.RegisterComponentInHierarchy<EnemyNavigationMotor>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<LadderClimber>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<EnemyActionExecutor>()
                    .UnderTransform(actorTransform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<MeleeHitboxController>()
                    .UnderTransform(actorTransform)
                    .AsSelf();
                if (hasActivationTrigger)
                {
                    builder.RegisterComponentInHierarchy<EnemyActivationTrigger>()
                        .UnderTransform(actorTransform)
                        .AsSelf();
                }

                builder.Register<ApplyDamageCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<ResolveMeleeHitCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<CriticalTargetCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<TargetingCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<PlatformRideCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<EnemyPerception>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyRandomStreams>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyActionSelector>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyController>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
            }, $"{prefab.name}_LifetimeRoot");

            EnemyActor actor = scope.Container.Resolve<EnemyActor>();
            actor.name = $"{prefab.name}_Instance";
            actor.AttachLifetimeRoot(scope.gameObject);
            return actor;
        }
    }
}
