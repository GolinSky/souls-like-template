using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyEncounterSpawner : MonoBehaviour, IInitializable
    {
        [SerializeField] private EnemySpawnPoint[] spawnPoints = { };
        [SerializeField] private bool spawnOnStart = true;

        private EnemyFactory _enemyFactory;

        [Inject]
        public void Construct(EnemyFactory enemyFactory)
        {
            _enemyFactory = enemyFactory;
        }

        public void Initialize()
        {
            if (!spawnOnStart)
            {
                return;
            }

            foreach (EnemySpawnPoint spawnPoint in spawnPoints)
            {
                _enemyFactory.CreateEnemy(spawnPoint);
            }
        }
    }
}
