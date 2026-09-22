using System;
using SoulsLike.Services.Navigation;
using SoulsLike.Services.VContainer;
using UnityEngine;
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
            return CreateEnemy(
                spawn.transform.position,
                spawn.transform.rotation,
                definition,
                groupCoordinator,
                spawn.BuildPatrolPositions(),
                spawn.RandomSeedOffset,
                spawn.name);
        }

        public EnemyActor CreateEnemy(
            Vector3 position,
            Quaternion rotation,
            EnemyCatalog.Definition definition,
            EnemyGroupCoordinator groupCoordinator,
            Vector3[] patrolPositions = null,
            int randomSeedOffset = 0,
            string spawnName = null)
        {
            EnemyActor prefab = definition.EnemyPrefab;
            NavMeshAgent prefabAgent = prefab.NavMeshAgent;
            NavMeshQueryFilter queryFilter = new()
            {
                agentTypeID = prefabAgent.agentTypeID,
                areaMask = prefabAgent.areaMask
            };
            bool hasSpawnPosition = _navMeshService.TrySamplePosition(position, prefabAgent.radius, queryFilter, out NavMeshHit spawnHit);
            if (!hasSpawnPosition)
            {
                hasSpawnPosition = _navMeshService.TrySampleNearestPosition(position, queryFilter, out spawnHit);
            }

            if (!hasSpawnPosition)
            {
                string locationLabel = string.IsNullOrEmpty(spawnName) ? position.ToString() : $"'{spawnName}'";
                throw new InvalidOperationException($"No compatible baked NavMesh could be found for enemy spawn point {locationLabel}.");
            }

            EnemySpawnData spawnData = new(spawnHit.position, rotation, patrolPositions ?? Array.Empty<Vector3>(), randomSeedOffset);
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
