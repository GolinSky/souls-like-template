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

            EnemyActor actor = null;
            GameObject lifetimeRoot = null;
            try
            {
                actor = UnityEngine.Object.Instantiate(
                    prefab,
                    spawnHit.position,
                    spawn.transform.rotation);
                actor.name = $"{prefab.name}_Instance";
                actor.ConfigureSpawn(
                    spawnHit.position,
                    spawn.BuildPatrolPositions(),
                    spawn.RandomSeedOffset);

                ViewEntity viewEntity = GetRequiredComponent<ViewEntity>(actor.gameObject);
                TargetLockNode targetLockNode = GetRequiredComponentInChildren<TargetLockNode>(
                    actor.gameObject);
                HealthComponent healthComponent = GetRequiredComponent<HealthComponent>(
                    actor.gameObject);
                CombatDefenseComponent combatDefense = GetRequiredComponent<CombatDefenseComponent>(
                    actor.gameObject);
                VisibilityComponent visibilityComponent =
                    GetRequiredComponent<VisibilityComponent>(actor.gameObject);
                EnemyHealthUiComponent healthUiComponent =
                    GetRequiredComponent<EnemyHealthUiComponent>(actor.gameObject);
                EnemyNavigationMotor motor = GetRequiredComponent<EnemyNavigationMotor>(
                    actor.gameObject);
                LadderClimber ladderClimber = GetRequiredComponent<LadderClimber>(actor.gameObject);
                EnemyActionExecutor actionExecutor =
                    GetRequiredComponentInChildren<EnemyActionExecutor>(actor.gameObject);
                MeleeHitboxController meleeHitbox =
                    GetRequiredComponentInChildren<MeleeHitboxController>(actor.gameObject);
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

                lifetimeRoot = new GameObject($"{prefab.name}_LifetimeRoot");
                lifetimeRoot.SetActive(false);
                lifetimeRoot.transform.SetParent(RootScope.transform, false);
                using (LifetimeScope.EnqueueParent(RootScope))
                using (LifetimeScope.Enqueue(builder =>
                {
                    builder.RegisterEntitySystemExt(EntityType.Enemy, entityId);
                    builder.RegisterComponent(viewEntity).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(targetLockNode).AsSelf();

                    builder.RegisterInstance(spawn.HealthData).AsImplementedInterfaces().AsSelf();
                    builder.RegisterInstance(spawn.BehaviourProfile);
                    builder.RegisterInstance(spawn.Moveset);
                    builder.RegisterInstance(groupCoordinator);
                    builder.RegisterComponent(actor).AsSelf();

                    builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                    builder.RegisterComponent(healthComponent).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(combatDefense).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(visibilityComponent).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(healthUiComponent).AsSelf().AsImplementedInterfaces();

                    builder.RegisterScriptableObject<WeaponDatabase>();

                    builder.RegisterComponent(motor).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(ladderClimber).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(actionExecutor).AsSelf().AsImplementedInterfaces();
                    builder.RegisterComponent(meleeHitbox).AsSelf();
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
                }))
                {
                    lifetimeRoot.AddComponent<LifetimeScope>();
                    lifetimeRoot.SetActive(true);
                    //todo: remove this shit
                }

                actor.transform.SetParent(lifetimeRoot.transform, true);
                actor.AttachLifetimeRoot(lifetimeRoot);
                return actor;
            }
            catch
            {
                if (lifetimeRoot != null
                    && actor != null
                    && actor.transform.IsChildOf(lifetimeRoot.transform))
                {
                    UnityEngine.Object.Destroy(lifetimeRoot);
                }
                else
                {
                    if (actor != null)
                    {
                        UnityEngine.Object.Destroy(actor.gameObject);
                    }

                    if (lifetimeRoot != null)
                    {
                        UnityEngine.Object.Destroy(lifetimeRoot);
                    }
                }

                throw;
            }
        }

        private static TComponent GetRequiredComponent<TComponent>(GameObject instance)
            where TComponent : Component
        {
            TComponent component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Enemy prefab requires a {typeof(TComponent).Name} component.");
            }

            return component;
        }

        private static TComponent GetRequiredComponentInChildren<TComponent>(
            GameObject instance)
            where TComponent : Component
        {
            TComponent component = instance.GetComponentInChildren<TComponent>(true);
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Enemy prefab requires a {typeof(TComponent).Name} component.");
            }

            return component;
        }
    }
}
