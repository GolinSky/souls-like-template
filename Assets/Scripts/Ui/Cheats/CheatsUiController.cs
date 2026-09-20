using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Enemy;
using SoulsLike.Interactions;
using SoulsLike.Services;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.Cheats
{
    public sealed class CheatsUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        ICheatsPresenter,
        IGameStateObserver
    {
        private const float HIT_DAMAGE = 20f;
        private const float ENEMY_SPAWN_DISTANCE = 3.0f;

        private readonly IInputService _inputService;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly IEntityLocator _entityLocator;
        private readonly EnemyService _enemyService;
        private readonly GraceSystem _graceSystem;
        private readonly List<IEntity> _entities = new();

        private CheatsUi _view;
        private CursorLockMode _cursorLockState;
        private bool _cursorVisible;
        private bool _isOpen;

        public bool IsPlayerInvincible => TryGetPlayer(out IEntity player, false)
            && GetApplyDamageCommand(player).IsCheatInvulnerable;

        public IReadOnlyList<EnemyId> AvailableEnemyIds
        {
            get
            {
                if (_enemyService != null && _enemyService.Catalog != null)
                {
                    List<EnemyId> ids = new();
                    foreach (var entry in _enemyService.Catalog.Definitions)
                    {
                        if (entry != null && entry.Key != EnemyId.Unassigned && !ids.Contains(entry.Key))
                        {
                            ids.Add(entry.Key);
                        }
                    }

                    if (ids.Count > 0)
                    {
                        return ids;
                    }
                }

                return new[] { EnemyId.ErikaMelee, EnemyId.BackstabDummy, EnemyId.RiposteDummy };
            }
        }

        public CheatsUiController(
            IUiService uiService,
            IInputService inputService,
            IGameStateNotifier gameStateNotifier,
            IEntityLocator entityLocator,
            EnemyService enemyService,
            GraceSystem graceSystem)
            : base(uiService)
        {
            _inputService = inputService;
            _gameStateNotifier = gameStateNotifier;
            _entityLocator = entityLocator;
            _enemyService = enemyService;
            _graceSystem = graceSystem;
        }

        public void Initialize()
        {
            _view = CreateUi<CheatsUi>();
            _view.AssignPresenter(this);
            UiService.MarkUiAsOverlay(_view);
            _view.Hide();
            _gameStateNotifier.RegisterObserver(this);
        }

        public void Tick()
        {
            if (_gameStateNotifier.CurrentGameState == GameState.Idle
                && _inputService.ToggleCheatsAction.WasPressedThisFrame())
            {
                Toggle();
            }
        }

        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
            if (_isOpen)
            {
                Close();
            }
        }

        public void HitPlayer()
        {
            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            ApplyDamage(player, player.Id, HIT_DAMAGE);
        }

        public void KillPlayer()
        {
            Close();
            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            ApplyDamage(player, player.Id, GetApplyDamageCommand(player).Stats.CurrentHealth);
        }

        public void TogglePlayerInvincibility()
        {
            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            ApplyDamageCommand command = GetApplyDamageCommand(player);
            command.SetCheatInvulnerable(!command.IsCheatInvulnerable);
        }

        public void ResetOpenGraces()
        {
            _graceSystem.ResetOpenGraces();
        }

        public void HitAllEnemies()
        {
            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            GetEnemies();
            if (_entities.Count == 0)
            {
                Debug.LogWarning("Cheats cannot hit enemies because no enemy entities are active.");
                return;
            }

            foreach (IEntity enemy in _entities)
            {
                ApplyDamage(enemy, player.Id, HIT_DAMAGE);
            }
        }

        public void KillAllEnemies()
        {
            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            GetEnemies();
            if (_entities.Count == 0)
            {
                Debug.LogWarning("Cheats cannot kill enemies because no enemy entities are active.");
                return;
            }

            foreach (IEntity enemy in _entities)
            {
                ApplyDamage(enemy, player.Id, GetApplyDamageCommand(enemy).Stats.CurrentHealth);
            }
        }

        public void RespawnEnemies()
        {
            _enemyService.RespawnEnemies();
        }

        public void SpawnEnemy(EnemyId enemyId)
        {
            if (enemyId == EnemyId.Unassigned)
            {
                Debug.LogWarning("Cheats cannot spawn enemy with Unassigned EnemyId.");
                return;
            }

            if (!TryGetPlayer(out IEntity player))
            {
                return;
            }

            if (!player.TryGetComponent(out TargetingCommand targeting))
            {
                Debug.LogWarning("Cheats cannot spawn enemy because player entity is missing TargetingCommand.");
                return;
            }

            TargetingSnapshot snapshot = targeting.Read();
            Vector3 forward = snapshot.Forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            Vector3 spawnPosition = snapshot.Position + forward * ENEMY_SPAWN_DISTANCE;
            Quaternion spawnRotation = Quaternion.LookRotation(-forward, Vector3.up);

            _enemyService.SpawnEnemy(enemyId, spawnPosition, spawnRotation);
        }

        public void SpawnEnemy(string enemyTypeOrId)
        {
            if (string.IsNullOrWhiteSpace(enemyTypeOrId))
            {
                Debug.LogWarning("Enemy type/id string cannot be null or empty.");
                return;
            }

            if (Enum.TryParse(enemyTypeOrId, true, out EnemyId parsedId))
            {
                SpawnEnemy(parsedId);
                return;
            }

            if (int.TryParse(enemyTypeOrId, out int rawId) && Enum.IsDefined(typeof(EnemyId), (EnemyId)rawId))
            {
                SpawnEnemy((EnemyId)rawId);
                return;
            }

            Debug.LogWarning($"Unknown enemy type/id '{enemyTypeOrId}'.");
        }

        public void SpawnEnemy(int enemyId)
        {
            if (Enum.IsDefined(typeof(EnemyId), (EnemyId)enemyId))
            {
                SpawnEnemy((EnemyId)enemyId);
            }
            else
            {
                Debug.LogWarning($"Unknown numeric enemy ID '{enemyId}'.");
            }
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.Idle || !_isOpen)
            {
                return;
            }

            _view.Hide();
            _isOpen = false;
        }

        private void Toggle()
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            _cursorLockState = Cursor.lockState;
            _cursorVisible = Cursor.visible;
            _view.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _isOpen = true;
        }

        private void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            _view.Hide();
            Cursor.lockState = _cursorLockState;
            Cursor.visible = _cursorVisible;
            _isOpen = false;
        }

        private bool TryGetPlayer(out IEntity player, bool logMissingPlayer = true)
        {
            _entityLocator.GetEntities(EntityType.Player, _entities);
            if (_entities.Count == 0)
            {
                if (logMissingPlayer)
                {
                    Debug.LogWarning("Cheats require an active player entity.");
                }

                player = null;
                return false;
            }

            player = _entities[0];
            return true;
        }

        private void GetEnemies()
        {
            _entityLocator.GetEntities(EntityType.Enemy, _entities);
        }

        private static void ApplyDamage(IEntity target, long sourceEntityId, float amount)
        {
            ApplyDamageCommand command = GetApplyDamageCommand(target);
            DamageRequest request = new DamageRequest
            {
                SourceEntityId = sourceEntityId,
                Amount = amount
            };
            command.ExecuteDirect(in request);
        }

        private static ApplyDamageCommand GetApplyDamageCommand(IEntity entity)
        {
            if (entity.TryGetComponent(out ApplyDamageCommand command))
            {
                return command;
            }

            throw new InvalidOperationException(
                $"Entity {entity.Id} ({entity.EntityType}) is missing {nameof(ApplyDamageCommand)}.");
        }
    }
}
