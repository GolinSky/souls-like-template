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
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyFactory : BaseFactory
    {
        public EnemyFactory(IObjectResolver resolver)
            : base(resolver)
        {
        }

        public EnemyActor CreateEnemy(EnemySpawnPoint spawn)
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
            if (!NavMesh.SamplePosition(
                    spawn.transform.position,
                    out NavMeshHit spawnHit,
                    prefabAgent.radius,
                    queryFilter))
            {
                throw new InvalidOperationException(
                    $"Enemy spawn point '{spawn.name}' must be within "
                    + $"{prefabAgent.radius} units of a baked NavMesh.");
            }

            EnemyActor actor = UnityEngine.Object.Instantiate(
                prefab,
                spawnHit.position,
                spawn.transform.rotation);
            actor.name = $"{prefab.name}_Instance";
            actor.ConfigureSpawn(spawnHit.position, spawn.BuildPatrolPositions());

            ViewEntity viewEntity = GetRequiredComponent<ViewEntity>(actor.gameObject);
            TargetLockNode targetLockNode = GetRequiredComponentInChildren<TargetLockNode>(
                actor.gameObject);
            HealthComponent healthComponent = GetRequiredComponent<HealthComponent>(
                actor.gameObject);
            VisibilityComponent visibilityComponent =
                GetRequiredComponent<VisibilityComponent>(actor.gameObject);
            EnemyHealthUiComponent healthUiComponent =
                GetRequiredComponent<EnemyHealthUiComponent>(actor.gameObject);
            EnemyNavigationMotor motor = GetRequiredComponent<EnemyNavigationMotor>(
                actor.gameObject);
            EnemyAnimationController animationController =
                GetRequiredComponentInChildren<EnemyAnimationController>(actor.gameObject);
            MeleeHitboxController meleeHitbox =
                GetRequiredComponentInChildren<MeleeHitboxController>(actor.gameObject);
            long entityId = RootScope.Container
                .Resolve<IUniqueIdGenerator>()
                .GenerateUniqueId();

            LifetimeScope enemyScope = RootScope.CreateChild(builder =>
            {
                builder.RegisterEntitySystemExt(EntityType.Enemy, entityId);
                builder.RegisterComponent(viewEntity).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(targetLockNode).AsSelf();

                builder.RegisterInstance(spawn.HealthData).AsImplementedInterfaces().AsSelf();
                builder.RegisterInstance(spawn.BehaviourProfile);
                builder.RegisterInstance(spawn.Moveset);
                builder.RegisterComponent(actor).AsSelf();

                builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(healthComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(visibilityComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(healthUiComponent).AsSelf().AsImplementedInterfaces();

                builder.RegisterScriptableObject<WeaponDatabase>();

                builder.RegisterComponent(motor).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(animationController).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(meleeHitbox).AsSelf();

                builder.Register<ApplyDamageCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<TargetingCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<EnemyPerception>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyActionSelector>(Lifetime.Singleton).AsSelf();
                builder.Register<EnemyBrain>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
            });

            actor.transform.SetParent(enemyScope.transform, true);
            actor.AttachLifetime(enemyScope);
            return actor;
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
