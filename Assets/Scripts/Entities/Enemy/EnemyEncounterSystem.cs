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
        [SerializeField] private bool respawnOnGrace = true;
        [SerializeField] private bool respawnOnGameEnded = true;
        [SerializeField, Min(1)] private int maxPressureSlots = 1;
        [SerializeField, Min(0.01f)] private float pressureSlotTimeoutSeconds = 3f;

        private readonly List<EnemyActor> _spawnedEnemies = new();
        private IGameStateNotifier _gameStateNotifier;
        private EnemyFactory _enemyFactory;
        private EnemyGroupCoordinator _groupCoordinator;
        private Coroutine _respawnCoroutine;
        private bool _isConstructed;
        private bool _isObserverRegistered;
        private bool _hasStarted;
        private bool _isRespawnAuthorized;
        private bool _restoreAfterEnable;
        private bool _isDestroyed;

        [Inject]
        public void Construct(IGameStateNotifier gameStateNotifier, EnemyFactory enemyFactory)
        {
            _gameStateNotifier = gameStateNotifier;
            _enemyFactory = enemyFactory;
            _isConstructed = true;
            TryRegisterObserver();
            TryRestoreAfterEnable();
        }

        private void OnEnable()
        {
            TryRegisterObserver();
            TryRestoreAfterEnable();
        }

        private void OnDisable()
        {
            UnregisterObserver();
            _restoreAfterEnable |= _spawnedEnemies.Count > 0 || _isRespawnAuthorized;
            CancelRespawn();
            _isRespawnAuthorized = false;
            DespawnEnemies(clearCoordinator: true);
        }

        private void Start()
        {
            _hasStarted = true;
            if (_restoreAfterEnable)
            {
                TryRestoreAfterEnable();
            }
            else if (spawnOnStart)
            {
                SpawnEnemies();
            }
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (newState == GameState.OnGraceSit && respawnOnGrace)
            {
                RespawnEnemies();
                return;
            }

            if (newState == GameState.Ended && respawnOnGameEnded)
            {
                RespawnEnemies();
            }
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            UnregisterObserver();
            CancelRespawn();
            _isRespawnAuthorized = false;
            _restoreAfterEnable = false;
            DespawnEnemies(clearCoordinator: true);
            _groupCoordinator?.Clear();
        }

        public void RespawnEnemies()
        {
            DespawnEnemies(clearCoordinator: true);
            ScheduleRespawn();
        }

        private IEnumerator SpawnEnemiesNextFrame()
        {
            yield return null;
            _respawnCoroutine = null;
            _isRespawnAuthorized = false;
            if (isActiveAndEnabled)
            {
                SpawnEnemies();
            }
        }

        private void SpawnEnemies()
        {
            if (_spawnedEnemies.Count > 0)
            {
                return;
            }

            _groupCoordinator ??= new EnemyGroupCoordinator(
                maxPressureSlots,
                pressureSlotTimeoutSeconds);
            foreach (EnemySpawnPoint spawnPoint in spawnPoints)
            {
                EnemyActor enemy = _enemyFactory.CreateEnemy(spawnPoint, _groupCoordinator);
                enemy.Despawned += OnEnemyDespawned;
                _spawnedEnemies.Add(enemy);
            }
        }

        private void DespawnEnemies(bool clearCoordinator = false)
        {
            EnemyActor[] enemies = _spawnedEnemies.ToArray();
            _spawnedEnemies.Clear();
            foreach (EnemyActor enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.Despawned -= OnEnemyDespawned;
                enemy.Despawn();
            }

            _groupCoordinator?.ReleaseAllPressureSlots();
            if (clearCoordinator)
            {
                _groupCoordinator?.Clear();
            }
        }

        private void Update()
        {
            _groupCoordinator?.Tick(Time.time);
        }

        private void OnEnemyDespawned(EnemyActor enemy)
        {
            enemy.Despawned -= OnEnemyDespawned;
            _spawnedEnemies.Remove(enemy);
        }

        private void TryRegisterObserver()
        {
            if (!_isConstructed || !isActiveAndEnabled || _isObserverRegistered)
            {
                return;
            }

            _gameStateNotifier.RegisterObserver(this);
            _isObserverRegistered = true;
        }

        private void UnregisterObserver()
        {
            if (!_isObserverRegistered)
            {
                return;
            }

            _gameStateNotifier.UnregisterObserver(this);
            _isObserverRegistered = false;
        }

        private void CancelRespawn()
        {
            if (_respawnCoroutine == null)
            {
                return;
            }

            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }

        private void TryRestoreAfterEnable()
        {
            if (!_isConstructed
                || !_hasStarted
                || _isDestroyed
                || !_restoreAfterEnable
                || !isActiveAndEnabled)
            {
                return;
            }

            _restoreAfterEnable = false;
            ScheduleRespawn();
        }

        private void ScheduleRespawn()
        {
            CancelRespawn();
            _isRespawnAuthorized = true;
            _respawnCoroutine = StartCoroutine(SpawnEnemiesNextFrame());
        }
    }
}
