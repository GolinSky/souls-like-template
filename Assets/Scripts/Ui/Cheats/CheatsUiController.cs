using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Enemy;
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

        private readonly IInputService _inputService;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly IEntityLocator _entityLocator;
        private readonly EnemyEncounterSystem _enemyEncounterSystem;
        private readonly List<IEntity> _entities = new();

        private CheatsUi _view;
        private CursorLockMode _cursorLockState;
        private bool _cursorVisible;
        private bool _isOpen;

        public CheatsUiController(
            IUiService uiService,
            IInputService inputService,
            IGameStateNotifier gameStateNotifier,
            IEntityLocator entityLocator,
            EnemyEncounterSystem enemyEncounterSystem)
            : base(uiService)
        {
            _inputService = inputService;
            _gameStateNotifier = gameStateNotifier;
            _entityLocator = entityLocator;
            _enemyEncounterSystem = enemyEncounterSystem;
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
            _enemyEncounterSystem.RespawnEnemies();
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

        private bool TryGetPlayer(out IEntity player)
        {
            _entityLocator.GetEntities(EntityType.Player, _entities);
            if (_entities.Count == 0)
            {
                Debug.LogWarning("Cheats require an active player entity.");
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
