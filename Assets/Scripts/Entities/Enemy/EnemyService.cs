using System.Collections;
using System.Collections.Generic;
using SoulsLike.Services;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyService : MonoBehaviour, IGameStateObserver
    {
        [SerializeField] private EnemyCatalog catalog;

        private readonly Dictionary<EnemySpawnGroup, GroupState> _groups = new();
        private IGameStateNotifier _gameStateNotifier;
        private EnemyFactory _enemyFactory;
        private bool _isConstructed;
        private bool _isObserverRegistered;
        private bool _hasStarted;
        private bool _groupsDiscovered;
        private bool _isDestroyed;

        public EnemyCatalog Catalog => catalog;

        [Inject]
        public void Construct(IGameStateNotifier gameStateNotifier, EnemyFactory enemyFactory)
        {
            _gameStateNotifier = gameStateNotifier;
            _enemyFactory = enemyFactory;
            _isConstructed = true;
            DiscoverGroups();
            TryRegisterObserver();
            TryInitialize();
        }

        private void OnEnable()
        {
            TryRegisterObserver();
            TryInitialize();
        }

        private void Start()
        {
            _hasStarted = true;
            TryInitialize();
        }

        private void OnDisable()
        {
            UnregisterObserver();
            foreach (GroupState state in _groups.Values)
            {
                DisableGroup(state);
            }
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            UnregisterObserver();
            foreach (GroupState state in _groups.Values)
            {
                state.Group.Enabled -= OnGroupEnabled;
                state.Group.Disabled -= OnGroupDisabled;
                DisableGroup(state);
                state.Coordinator.Clear();
            }

            _groups.Clear();
        }

        private void Update()
        {
            foreach (GroupState state in _groups.Values)
            {
                state.Coordinator.Tick(Time.time);
            }
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            foreach (GroupState state in _groups.Values)
            {
                if (!state.Group.isActiveAndEnabled)
                {
                    continue;
                }

                if (newState == GameState.OnGraceSit && state.Group.RespawnOnGrace)
                {
                    RespawnGroup(state);
                    continue;
                }

                if (newState == GameState.Ended && state.Group.RespawnOnGameEnded)
                {
                    RespawnGroup(state);
                }
            }
        }

        public void RespawnEnemies()
        {
            foreach (GroupState state in _groups.Values)
            {
                if (state.Group.isActiveAndEnabled)
                {
                    RespawnGroup(state);
                }
            }
        }

        private void TryInitialize()
        {
            if (!_isConstructed || !_hasStarted || _isDestroyed || !isActiveAndEnabled)
            {
                return;
            }

            DiscoverGroups();
            foreach (GroupState state in _groups.Values)
            {
                if (!state.Group.isActiveAndEnabled)
                {
                    continue;
                }

                StartGroup(state);
            }
        }

        private void DiscoverGroups()
        {
            if (_groupsDiscovered)
            {
                return;
            }

            _groupsDiscovered = true;
            EnemySpawnGroup[] groups = GetComponentsInChildren<EnemySpawnGroup>(true);
            foreach (EnemySpawnGroup group in groups)
            {
                GroupState state = new(
                    group,
                    new EnemyGroupCoordinator(
                        group.MaxPressureSlots,
                        group.PressureSlotTimeoutSeconds));
                _groups.Add(group, state);
                group.Enabled += OnGroupEnabled;
                group.Disabled += OnGroupDisabled;
            }
        }

        private void StartGroup(GroupState state)
        {
            if (state.RestoreAfterEnable)
            {
                state.RestoreAfterEnable = false;
                ScheduleRespawn(state);
                return;
            }

            if (state.HasStarted)
            {
                return;
            }

            state.HasStarted = true;
            if (state.Group.SpawnOnStart)
            {
                SpawnGroup(state);
            }
        }

        private void OnGroupEnabled(EnemySpawnGroup group)
        {
            if (_isDestroyed
                || !isActiveAndEnabled
                || !_groups.TryGetValue(group, out GroupState state))
            {
                return;
            }

            StartGroup(state);
        }

        private void OnGroupDisabled(EnemySpawnGroup group)
        {
            if (_isDestroyed || !_groups.TryGetValue(group, out GroupState state))
            {
                return;
            }

            DisableGroup(state);
        }

        private void RespawnGroup(GroupState state)
        {
            DespawnGroup(state);
            ScheduleRespawn(state);
        }

        private void SpawnGroup(GroupState state)
        {
            if (state.SpawnedEnemies.Count > 0 || !state.Group.isActiveAndEnabled)
            {
                return;
            }

            EnemySpawner[] spawners = state.Group.GetComponentsInChildren<EnemySpawner>(true);
            foreach (EnemySpawner spawner in spawners)
            {
                if (!spawner.isActiveAndEnabled || GetOwningGroup(spawner) != state.Group)
                {
                    continue;
                }

                EnemyCatalog.Definition definition = catalog.GetDefinition(spawner.EnemyId);
                EnemyActor enemy = _enemyFactory.CreateEnemy(spawner, definition, state.Coordinator);
                enemy.Despawned += OnEnemyDespawned;
                state.SpawnedEnemies.Add(enemy);
            }
        }

        private static EnemySpawnGroup GetOwningGroup(EnemySpawner spawner)
        {
            for (Transform current = spawner.transform; current != null; current = current.parent)
            {
                if (current.TryGetComponent(out EnemySpawnGroup group))
                {
                    return group;
                }
            }

            return null;
        }

        private void DespawnGroup(GroupState state)
        {
            EnemyActor[] enemies = state.SpawnedEnemies.ToArray();
            state.SpawnedEnemies.Clear();
            foreach (EnemyActor enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.Despawned -= OnEnemyDespawned;
                enemy.Despawn();
            }

            state.Coordinator.ReleaseAllPressureSlots();
        }

        private void DisableGroup(GroupState state)
        {
            state.RestoreAfterEnable |= state.SpawnedEnemies.Count > 0 || state.IsRespawnAuthorized;
            CancelRespawn(state);
            state.IsRespawnAuthorized = false;
            DespawnGroup(state);
        }

        private void ScheduleRespawn(GroupState state)
        {
            CancelRespawn(state);
            state.IsRespawnAuthorized = true;
            state.RespawnCoroutine = StartCoroutine(SpawnGroupNextFrame(state));
        }

        private IEnumerator SpawnGroupNextFrame(GroupState state)
        {
            yield return null;
            state.RespawnCoroutine = null;
            state.IsRespawnAuthorized = false;
            if (isActiveAndEnabled && state.Group.isActiveAndEnabled)
            {
                SpawnGroup(state);
            }
        }

        private void OnEnemyDespawned(EnemyActor enemy)
        {
            enemy.Despawned -= OnEnemyDespawned;
            foreach (GroupState state in _groups.Values)
            {
                if (state.SpawnedEnemies.Remove(enemy))
                {
                    return;
                }
            }
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

        private void CancelRespawn(GroupState state)
        {
            if (state.RespawnCoroutine == null)
            {
                return;
            }

            StopCoroutine(state.RespawnCoroutine);
            state.RespawnCoroutine = null;
        }

        private sealed class GroupState
        {
            public GroupState(EnemySpawnGroup group, EnemyGroupCoordinator coordinator)
            {
                Group = group;
                Coordinator = coordinator;
            }

            public EnemySpawnGroup Group { get; }
            public EnemyGroupCoordinator Coordinator { get; }
            public List<EnemyActor> SpawnedEnemies { get; } = new();
            public Coroutine RespawnCoroutine { get; set; }
            public bool HasStarted { get; set; }
            public bool IsRespawnAuthorized { get; set; }
            public bool RestoreAfterEnable { get; set; }
        }
    }
}
