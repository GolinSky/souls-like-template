using System;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
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

        private GameState _currentGameState;

        public PlayerController(
            IInputService inputService,
            Character character,
            ICameraService cameraService,
            ITargetingService targetingService,
            IGameStateNotifier gameStateNotifier)
        {
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
            _gameStateNotifier = gameStateNotifier ?? throw new ArgumentNullException(nameof(gameStateNotifier));
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
            if (_currentGameState != GameState.Idle)
            {
                return;
            }

            HandleLockOnInput();
            _character.UpdateBehaviour(_inputService.CharacterActions);
        }

        public void LateTick()
        {
            if (_currentGameState == GameState.Idle)
            {
                _cameraService.UpdateRotation(_inputService.CharacterActions.Look.ReadValue<Vector2>());
            }
        }

        private void HandleLockOnInput()
        {
            if (_targetingService.IsLockedOn && !_targetingService.IsCurrentTargetValid(_character.transform))
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

            if (_targetingService.TryAcquireTarget(_character.transform))
            {
                TargetLockNode target = _targetingService.CurrentTarget;
                Transform targetTransform = target.TargetTransform;

                _character.SetLockOnTarget(true, targetTransform);
                _cameraService.SetLockOnTarget(targetTransform);
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
