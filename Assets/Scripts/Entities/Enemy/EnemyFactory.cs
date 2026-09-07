using System;
using SoulsLike.Components.Visibility;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Extensions;
using SoulsLike.Factory;
using SoulsLike.Items;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Navigation;
using SoulsLike.Entities.Ladder;
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
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Enemy spawn point '{spawn.name}' requires an enemy prefab.");
            }

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

            EnemyActor actor = UnityEngine.Object.Instantiate(
                prefab,
                spawnHit.position,
                spawn.transform.rotation);
            actor.name = $"{prefab.name}_Instance";
            actor.ConfigureSpawn(
                spawnHit.position,
                spawn.BuildPatrolPositions(),
                spawn.RandomSeedOffset);

            EnemyActivationTrigger[] activationTriggers =
                actor.GetComponentsInChildren<EnemyActivationTrigger>(true);
            if (activationTriggers.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Enemy prefab '{prefab.name}' may contain only one {nameof(EnemyActivationTrigger)}.");
            }

            EnemyActivationTrigger activationTrigger = activationTriggers.Length == 1
                ? activationTriggers[0]
                : null;
            long entityId = RootScope.Container
                .Resolve<IUniqueIdGenerator>()
                .GenerateUniqueId();

            LifetimeScope scope = RootScope.CreateChild(builder =>
            {
                builder.RegisterEntitySystemExt(EntityType.Enemy, entityId);
                builder.RegisterComponentInHierarchy<ViewEntity>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<TargetLockNode>()
                    .UnderTransform(actor.transform)
                    .AsSelf();

                builder.RegisterInstance(spawn.HealthData).AsImplementedInterfaces().AsSelf();
                builder.RegisterInstance(spawn.BehaviourProfile);
                builder.RegisterInstance(spawn.Moveset);
                builder.RegisterInstance(groupCoordinator);
                builder.RegisterComponent(actor).AsSelf();
                    
                builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponentInHierarchy<HealthComponent>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<CombatDefenseComponent>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<VisibilityComponent>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<EnemyHealthUiComponent>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();

                builder.RegisterScriptableObject<WeaponDatabase>();

                builder.RegisterComponentInHierarchy<EnemyNavigationMotor>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<LadderClimber>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<EnemyActionExecutor>()
                    .UnderTransform(actor.transform)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.RegisterComponentInHierarchy<MeleeHitboxController>()
                    .UnderTransform(actor.transform)
                    .AsSelf();
                if (activationTrigger != null)
                {
                    builder.RegisterComponent(activationTrigger).AsSelf();
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
                builder.Register<EnemyPerception>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyRandomStreams>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyActionSelector>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyController>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
            }, $"{prefab.name}_LifetimeRoot");

            actor.transform.SetParent(scope.transform, true);
            actor.AttachLifetimeRoot(scope.gameObject);
            return actor;
        }
    }
}
