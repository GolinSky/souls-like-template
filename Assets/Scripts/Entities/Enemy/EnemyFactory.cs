using System;
using SoulsLike.Services.Navigation;
using SoulsLike.Services.VContainer;
using UnityEngine.AI;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly EnemyScopeInstaller _enemyScopePrefab;
        private readonly INavMeshService _navMeshService;

        public EnemyFactory(
            LifetimeScope parentScope,
            EnemyScopeInstaller enemyScopePrefab,
            INavMeshService navMeshService)
        {
            _parentScope = parentScope;
            _enemyScopePrefab = enemyScopePrefab;
            _navMeshService = navMeshService;
        }

        public EnemyActor CreateEnemy(
            EnemySpawner spawn,
            EnemyCatalog.Definition definition,
            EnemyGroupCoordinator groupCoordinator)
        {
            EnemyActor prefab = definition.EnemyPrefab;
            NavMeshAgent prefabAgent = prefab.NavMeshAgent;
            NavMeshQueryFilter queryFilter = new()
            {
                agentTypeID = prefabAgent.agentTypeID,
                areaMask = prefabAgent.areaMask
            };
            bool hasSpawnPosition = _navMeshService.TrySamplePosition(spawn.transform.position, prefabAgent.radius, queryFilter, out NavMeshHit spawnHit);
            if (!hasSpawnPosition)
            {
                hasSpawnPosition = _navMeshService.TrySampleNearestPosition(spawn.transform.position, queryFilter, out spawnHit);
            }

            if (!hasSpawnPosition)
            {
                throw new InvalidOperationException($"No compatible baked NavMesh could be found for enemy spawn point '{spawn.name}'.");
            }

            EnemySpawnData spawnData = new(spawnHit.position, spawn.transform.rotation, spawn.BuildPatrolPositions(), spawn.RandomSeedOffset);
            EnemyScopeInstaller scope = _parentScope.CreateChildFromPrefab(_enemyScopePrefab);

            EnemyActor actor = UnityEngine.Object.Instantiate(prefab, scope.transform, true);
            actor.name = $"{prefab.name}_Instance";
            actor.StageSpawn(spawnData);
            scope.ConfigureEnemy(definition, groupCoordinator);
            scope.gameObject.SetActive(true);
            scope.BuildOnce();
            return actor;
        }
    }
}
