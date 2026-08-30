using System.Collections;
using System.Collections.Generic;
using SoulsLike.Services;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyEncounterSystem : MonoBehaviour, IGameStateObserver
    {
        [SerializeField] private EnemySpawnPoint[] spawnPoints = { };
        [SerializeField] private bool spawnOnStart = true;

        private readonly List<EnemyActor> _spawnedEnemies = new();
        private IGameStateNotifier _gameStateNotifier;
        private EnemyFactory _enemyFactory;
        private Coroutine _respawnCoroutine;

        [Inject]
        public void Construct(IGameStateNotifier gameStateNotifier, EnemyFactory enemyFactory)
        {
            _gameStateNotifier = gameStateNotifier;
            _enemyFactory = enemyFactory;
            _gameStateNotifier.RegisterObserver(this);
        }

        private void Start()
        {
            SpawnEnemies();
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (newState != GameState.OnGraceSit)
            {
                return;
            }

            Restart();
        }

        private void OnDestroy()
        {
            _gameStateNotifier.UnregisterObserver(this);
        }

        private void Restart()
        {
            foreach (EnemyActor enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    enemy.Despawn();
                }
            }

            _spawnedEnemies.Clear();

            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
            }

            _respawnCoroutine = StartCoroutine(SpawnEnemiesNextFrame());
        }

        private IEnumerator SpawnEnemiesNextFrame()
        {
            yield return null;
            _respawnCoroutine = null;
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (!spawnOnStart)
            {
                return;
            }

            foreach (EnemySpawnPoint spawnPoint in spawnPoints)
            {
                _spawnedEnemies.Add(_enemyFactory.CreateEnemy(spawnPoint));
            }
        }
    }
}
