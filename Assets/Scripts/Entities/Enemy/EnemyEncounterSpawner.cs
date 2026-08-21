using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyEncounterSpawner : MonoBehaviour
    {
        [SerializeField] private EnemySpawnPoint[] spawnPoints = { };
        [SerializeField] private bool spawnOnStart = true;

        public void Spawn(EnemyFactory enemyFactory)
        {
            if (!spawnOnStart)
            {
                return;
            }

            foreach (EnemySpawnPoint spawnPoint in spawnPoints)
            {
                enemyFactory.CreateEnemy(spawnPoint);
            }
        }
    }
}
