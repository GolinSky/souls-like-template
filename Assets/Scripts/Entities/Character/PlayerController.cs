using System;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Interactions;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using SoulsLike.Entities.BaseEntity.EntityCommands;
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
        private readonly PlayerCharacterInputAdapter _inputAdapter;
        private readonly InteractionController _interactionController;

        private GameState _currentGameState;

        public PlayerController(
            IInputService inputService,
            Character character,
            ICameraService cameraService,
            ITargetingService targetingService,
            IGameStateNotifier gameStateNotifier,
            PlayerCharacterInputAdapter inputAdapter,
            InteractionController interactionController)
        {
            _inputService = inputService;
            _character = character;
            _cameraService = cameraService;
            _targetingService = targetingService;
            _gameStateNotifier = gameStateNotifier;
            _inputAdapter = inputAdapter;
            _interactionController = interactionController;
        }

        public void Initialize()
        {
            _cameraService.SetTarget(_character.CameraTarget);
            _gameStateNotifier.RegisterObserver(this);
            _currentGameState = _gameStateNotifier.CurrentGameState;
        }

        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
        }

        public void OnGameStateChanged(GameState newState)
        {
            _currentGameState = newState;
        }

        public void Tick()
        {
            if (_currentGameState != GameState.Idle || _character.IsInputBlocked)
            {
                _interactionController.ClearTarget();
                return;
            }

            HandleLockOnInput();
            _interactionController.Tick();
            _character.Tick(_inputAdapter.Read(_character.CurrentActionState));
        }

        public void LateTick()
        {
            if (_currentGameState == GameState.Idle && !_character.IsInputBlocked)
            {
                _cameraService.UpdateRotation(_inputService.CharacterActions.Look.ReadValue<Vector2>());
            }
        }

        private void HandleLockOnInput()
        {
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
    }
}
