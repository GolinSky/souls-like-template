using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.PlayerSession;
using UnityEngine;

namespace SoulsLike.Entities.Character.Input
{
    /// <summary>Recognizes local input gestures before the session applies action priority.</summary>
    public sealed class PlayerInputReader
    {
        private const float SPRINT_HOLD_THRESHOLD = 0.3f;

        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private float _sprintHoldTime;
        private bool _sprintQualified;
        private bool _rollRequestedOnRelease;

        public PlayerInputReader(IInputService inputService, ICameraService cameraService)
        {
            _inputService = inputService;
            _cameraService = cameraService;
        }

        public PlayerSessionInput Read()
        {
            ProjectInputActions.CharacterActions actions = _inputService.CharacterActions;
            UpdateSprintGesture(
                actions.Sprint.WasPressedThisFrame(),
                actions.Sprint.IsPressed(),
                actions.Sprint.WasReleasedThisFrame());
            return CreateInput(actions, _sprintQualified, _rollRequestedOnRelease);
        }

        public PlayerSessionInput ReadMovementOnly()
        {
            return CreateInput(_inputService.CharacterActions, false, false);
        }

        public void Reset()
        {
            _sprintHoldTime = 0f;
            _sprintQualified = false;
            _rollRequestedOnRelease = false;
        }

        private PlayerSessionInput CreateInput(
            ProjectInputActions.CharacterActions actions,
            bool sprintHeld,
            bool rollRequested)
        {
            UnityEngine.Vector2 move = actions.Move.ReadValue<UnityEngine.Vector2>();
            return new PlayerSessionInput(
                new System.Numerics.Vector2(move.x, move.y),
                _cameraService.GetYaw(),
                sprintHeld,
                rollRequested,
                actions.Crouch.IsPressed(),
                actions.Guard.WasPressedThisFrame(),
                actions.Guard.IsPressed(),
                actions.Attack.WasPressedThisFrame(),
                actions.Attack.IsPressed(),
                actions.StrongAttack.WasPressedThisFrame(),
                actions.StrongAttack.IsPressed(),
                actions.SpecialAbility.WasPressedThisFrame(),
                actions.SwitchWeapon.WasPressedThisFrame(),
                actions.SwitchShield.WasPressedThisFrame(),
                actions.SwitchFlask.WasPressedThisFrame(),
                actions.UseItem.WasPressedThisFrame(),
                actions.TwoHanded.WasPressedThisFrame(),
                actions.Jump.WasPressedThisFrame());
        }

        private void UpdateSprintGesture(bool pressedThisFrame, bool isPressed, bool releasedThisFrame)
        {
            _rollRequestedOnRelease = false;
            if (pressedThisFrame)
            {
                _sprintHoldTime = 0f;
                _sprintQualified = false;
            }
            if (isPressed)
            {
                _sprintHoldTime += Time.deltaTime;
                if (_sprintHoldTime >= SPRINT_HOLD_THRESHOLD) _sprintQualified = true;
            }
            if (releasedThisFrame)
            {
                _rollRequestedOnRelease = !_sprintQualified;
                _sprintHoldTime = 0f;
                _sprintQualified = false;
            }
        }
    }
}
