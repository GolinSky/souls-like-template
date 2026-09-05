using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Interactions;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Items;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class PlayerController : ITickable, ILateTickable, IInitializable, IGameStateObserver, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly Character _character;
        private readonly ICameraService _cameraService;
        private readonly ITargetingService _targetingService;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly HealthComponent _healthComponent;
        private readonly PlayerInputReader _inputReader;
        private readonly InteractionController _interactionController;
        private readonly ICoreGameOrchestrator _coreGameOrchestrator;

        private GameState _currentGameState;

        public PlayerController(
            IInputService inputService,
            Character character,
            ICameraService cameraService,
            ITargetingService targetingService,
            IGameStateNotifier gameStateNotifier,
            HealthComponent healthComponent,
            PlayerInputReader inputReader,
            InteractionController interactionController,
            ICoreGameOrchestrator coreGameOrchestrator)
        {
            _inputService = inputService;
            _character = character;
            _cameraService = cameraService;
            _targetingService = targetingService;
            _gameStateNotifier = gameStateNotifier;
            _healthComponent = healthComponent;
            _inputReader = inputReader;
            _interactionController = interactionController;
            _coreGameOrchestrator = coreGameOrchestrator;
        }

        public void Initialize()
        {
            _cameraService.SetTarget(_character.CameraTarget);
            _gameStateNotifier.RegisterObserver(this);
            _healthComponent.Model.OnDied += HandleDied;
            _character.OnDeathAnimationCompleted += HandleDeathAnimationCompleted;
            _currentGameState = _gameStateNotifier.CurrentGameState;
        }

        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
            _healthComponent.Model.OnDied -= HandleDied;
            _character.OnDeathAnimationCompleted -= HandleDeathAnimationCompleted;
        }

        public void OnGameStateChanged(GameState newState)
        {
            _currentGameState = newState;

            if (newState == GameState.Ended)
            {
                ClearLockOn();
            }

            if (newState == GameState.OnGraceSit || newState == GameState.Ended)
            {
                HealthStats stats = _healthComponent.Stats;
                stats.CurrentHealth = stats.MaxHealth;
                stats.CurrentFocus = stats.MaxFocus;
                stats.CurrentStamina = stats.MaxStamina;
                stats.IsAlive = true;
                _healthComponent.ApplyAuthoritativeStats(stats);
                _character.InventoryComponent.RefillFlask(ItemId.CrimsonFlask, 5);
            }
        }

        public void Tick()
        {
            if (!_healthComponent.Stats.IsAlive
                || (_character.IsInputBlocked && !_character.IsInLadderOperation)
                || (_currentGameState != GameState.Idle && _currentGameState != GameState.Paused))
            {
                _interactionController.ClearTarget();
                return;
            }

            if (_currentGameState == GameState.Paused)
            {
                _interactionController.ClearTarget();
                if (!_character.IsInLadderOperation)
                {
                    _character.Tick(_inputReader.ReadMovementOnly());
                }
                return;
            }

            if (!_character.IsInLadderOperation)
            {
                HandleLockOnInput();
                _interactionController.Tick();
            }
            else
            {
                _interactionController.ClearTarget();
            }
            _character.Tick(_inputReader.Read(_character.CurrentActionState));
        }

        public void LateTick()
        {
            _cameraService.UpdateFollowTarget(_character.IsGrounded, _character.VerticalVelocity);

            if (_currentGameState == GameState.Idle)
            {
                Vector2 look = !_healthComponent.Stats.IsAlive || _character.IsInputBlocked
                    ? Vector2.zero
                    : _inputService.CharacterActions.Look.ReadValue<Vector2>();
                _cameraService.UpdateRotation(look);
            }
        }

        private void HandleLockOnInput()
        {
            if (_inputService.WasUiBackConsumedThisFrame)
            {
                return;
            }

            if (_targetingService.IsLockedOn && !_targetingService.IsCurrentTargetValid(_character.transform.position))
            {
                ClearLockOn();
            }

            if (!_inputService.CharacterActions.LockOn.WasPressedThisFrame())
            {
                return;
            }

            if (_targetingService.IsLockedOn)
            {
                ClearLockOn();
                return;
            }

            if (_targetingService.TryAcquireTarget(_character.transform.position)
                && _targetingService.TryGetCurrentTarget(out TargetingSnapshot snapshot))
            {
                _character.SetLockOnTarget(true, snapshot.EntityId);
                _cameraService.SetLockOnTarget(snapshot.EntityId);
            }
            else
            {
                _cameraService.RecenterCamera();
            }
        }

        private void ClearLockOn()
        {
            _targetingService.ClearTarget();
            _character.SetLockOnTarget(false, null);
            _cameraService.ClearLockOnTarget();
        }

        private void HandleDied(long sourceEntityId)
        {
            _character.PlayDeath();
        }

        private void HandleDeathAnimationCompleted()
        {
            _coreGameOrchestrator.RespawnAtLastGrace().Forget();
        }
    }
}
