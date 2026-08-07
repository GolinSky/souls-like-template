using System;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class PlayerController : ITickable, ILateTickable, IInitializable, IGameStateObserver, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly Character _character;
        private readonly ICameraService _cameraService;
        private readonly IGameStateNotifier _gameStateNotifier;

        private GameState _currentGameState;

        public PlayerController(
            IInputService inputService,
            Character character,
            ICameraService cameraService,
            IGameStateNotifier gameStateNotifier)
        {
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
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

            _character.UpdateBehaviour(_inputService.CharacterActions);
            if (_inputService.CharacterActions.SwitchCameraAngle.WasPressedThisFrame())
            {
                _cameraService.SwitchAngle();
            }
        }

        public void LateTick()
        {
            if (_currentGameState == GameState.Idle)
            {
                _cameraService.UpdateRotation(_inputService.CharacterActions.Look.ReadValue<Vector2>());
            }
        }
    }
}
