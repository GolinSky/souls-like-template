using SoulsLike.Entities.Character.Input;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.PlayerSession;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    /// <summary>Feeds local input and camera updates through the player-session boundary.</summary>
    public sealed class PlayerController : ITickable, ILateTickable, IInitializable
    {
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly PlayerInputReader _inputReader;
        private readonly IPlayerSession _session;

        public PlayerController(
            IInputService inputService,
            ICameraService cameraService,
            PlayerInputReader inputReader,
            IPlayerSession session)
        {
            _inputService = inputService;
            _cameraService = cameraService;
            _inputReader = inputReader;
            _session = session;
        }

        public void Initialize()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _cameraService.SetTarget(_session.Snapshot.CameraTarget);
        }

        public void Tick()
        {
            PlayerSessionSnapshot snapshot = _session.Snapshot;
            if (snapshot.IsPaused)
            {
                _session.Submit(_inputReader.ReadMovementOnly());
                return;
            }

            if (snapshot.CanReadGameplayInput)
            {
                _session.Submit(_inputReader.Read());
                return;
            }

            _inputReader.Reset();
            _session.ClearInput();
        }

        public void LateTick()
        {
            PlayerSessionSnapshot snapshot = _session.Snapshot;
            _cameraService.UpdateFollowTarget(snapshot.IsGrounded, snapshot.VerticalVelocity);
            if (snapshot.ShouldUpdateCamera)
            {
                Vector2 look = snapshot.CanRotateCamera
                    ? _inputService.CharacterActions.Look.ReadValue<Vector2>()
                    : Vector2.zero;
                _cameraService.UpdateRotation(look);
            }
        }
    }
}
