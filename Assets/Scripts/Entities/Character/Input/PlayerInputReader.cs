using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using UnityEngine;

namespace SoulsLike.Entities.Character.Input
{
    public sealed class PlayerInputReader
    {
        private const float SPRINT_HOLD_THRESHOLD = 0.3f;
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private float _sprintHoldTime;
        private bool _sprintQualified;
        private bool _rollRequestedOnRelease;
        private bool _suppressLightUntilRelease;

        public PlayerInputReader(IInputService inputService, ICameraService cameraService)
        {
            _inputService = inputService;
            _cameraService = cameraService;
        }

        public CharacterInput ReadMovementOnly()
        {
            ProjectInputActions.CharacterActions actions = _inputService.CharacterActions;
            return new CharacterInput(actions.Move.ReadValue<Vector2>(), _cameraService.GetYaw(), false, false, false, false);
        }

        public CharacterInput Read(CharacterAction.State currentState)
        {
            ProjectInputActions.CharacterActions actions = _inputService.CharacterActions;
            UpdateSprintGesture(actions.Sprint.WasPressedThisFrame(), actions.Sprint.IsPressed(), actions.Sprint.WasReleasedThisFrame());

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            float cameraYaw = _cameraService.GetYaw();
            bool hasMovement = moveInput.sqrMagnitude > 0.0001f;
            bool sprinting = _sprintQualified && hasMovement;
            bool rollActive = currentState == CharacterAction.State.Roll;
            bool strongAttackPressed = !rollActive && TryResolveHeavyAttack(
                actions.StrongAttack.WasPressedThisFrame(),
                actions.Attack.IsPressed());

            CharacterAction? first = null;
            CharacterAction? second = null;

            // Equipment and hand-mode are the only same-frame pair retained by the legacy path.
            if (actions.SwitchWeapon.WasPressedThisFrame())
            {
                first = CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchRightWeapon);
            }
            else if (actions.SwitchShield.WasPressedThisFrame())
            {
                first = CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchLeftWeapon);
            }
            else if (actions.SwitchFlask.WasPressedThisFrame())
            {
                first = CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchQuickItem);
            }
            else if (actions.UseItem.WasPressedThisFrame())
            {
                first = CharacterAction.Equipment(CharacterAction.EquipmentKind.UseQuickItem);
            }

            if (actions.TwoHanded.WasPressedThisFrame())
            {
                CharacterAction handMode = CharacterAction.Equipment(CharacterAction.EquipmentKind.ToggleHandMode);
                if (!first.HasValue) first = handMode;
                else second = handMode;
            }

            if (!first.HasValue)
            {
                if (strongAttackPressed)
                {
                    first = CharacterAction.Attack(
                        CharacterAction.AttackIntent.Heavy,
                        false,
                        sprinting,
                        moveInput,
                        cameraYaw);
                }
                else if (!rollActive && actions.SpecialAbility.WasPressedThisFrame())
                {
                    first = CharacterAction.Attack(
                        CharacterAction.AttackIntent.Special,
                        false,
                        false,
                        moveInput,
                        cameraYaw);
                }
                else if (!ShouldSuppressLightAttack(
                    actions.Attack.WasPressedThisFrame()))
                {
                    first = CharacterAction.Attack(
                        CharacterAction.AttackIntent.Light,
                        false,
                        sprinting,
                        moveInput,
                        cameraYaw);
                }
                else if (actions.Guard.WasPressedThisFrame())
                {
                    first = CharacterAction.Attack(
                        CharacterAction.AttackIntent.Light,
                        true,
                        false,
                        moveInput,
                        cameraYaw);
                }
                else if (ShouldRoll(
                    actions.Roll.WasReleasedThisFrame()))
                {
                    first = CharacterAction.Roll(moveInput, cameraYaw);
                }
                else if (actions.Jump.WasPressedThisFrame())
                {
                    first = CharacterAction.Jump(sprinting);
                }
            }

            return new CharacterInput(moveInput, cameraYaw, sprinting, actions.Crouch.IsPressed(), actions.Guard.IsPressed(), actions.StrongAttack.IsPressed() && !rollActive, first, second);
        }

        private void UpdateSprintGesture(bool pressedThisFrame, bool isPressed, bool releasedThisFrame)
        {
            _rollRequestedOnRelease = false;
            if (pressedThisFrame) { _sprintHoldTime = 0f; _sprintQualified = false; }
            if (isPressed) { _sprintHoldTime += Time.deltaTime; if (_sprintHoldTime >= SPRINT_HOLD_THRESHOLD) _sprintQualified = true; }
            if (releasedThisFrame) { _rollRequestedOnRelease = !_sprintQualified; _sprintHoldTime = 0f; _sprintQualified = false; }
        }

        private bool TryResolveHeavyAttack(bool pressedThisFrame, bool lightIsPressed)
        {
            if (!lightIsPressed) _suppressLightUntilRelease = false;
            if (!pressedThisFrame) return false;
            _suppressLightUntilRelease = true;
            return true;
        }

        private bool ShouldSuppressLightAttack(bool lightPressedThisFrame) => _suppressLightUntilRelease || !lightPressedThisFrame;
        private bool ShouldRoll(bool rollReleasedThisFrame) => rollReleasedThisFrame && _rollRequestedOnRelease;
    }
}
