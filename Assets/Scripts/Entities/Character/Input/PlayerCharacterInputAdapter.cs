using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using UnityEngine;

namespace SoulsLike.Entities.Character.Input
{
    public sealed class PlayerCharacterInputAdapter
    {
        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly SprintRollGestureResolver _sprintRollResolver;
        private readonly HeavyAttackGestureResolver _heavyAttackResolver;

        public PlayerCharacterInputAdapter(
            IInputService inputService,
            ICameraService cameraService,
            SprintRollGestureResolver sprintRollResolver,
            HeavyAttackGestureResolver heavyAttackResolver)
        {
            _inputService = inputService;
            _cameraService = cameraService;
            _sprintRollResolver = sprintRollResolver;
            _heavyAttackResolver = heavyAttackResolver;
        }

        public CharacterInputBatch ReadMovementOnly()
        {
            ProjectInputActions.CharacterActions actions = _inputService.CharacterActions;
            CharacterControlFrame frame = new CharacterControlFrame(
                actions.Move.ReadValue<Vector2>(),
                _cameraService.GetYaw(),
                false,
                false,
                false,
                false);
            return new CharacterInputBatch(frame);
        }

        public CharacterInputBatch Read(CharacterActionStateId currentState)
        {
            ProjectInputActions.CharacterActions actions = _inputService.CharacterActions;
            _sprintRollResolver.Update(
                actions.Sprint.WasPressedThisFrame(),
                actions.Sprint.IsPressed(),
                actions.Sprint.WasReleasedThisFrame(),
                Time.deltaTime);

            Vector2 moveInput = actions.Move.ReadValue<Vector2>();
            float cameraYaw = _cameraService.GetYaw();
            bool hasMovement = moveInput.sqrMagnitude > 0.0001f;
            bool sprinting = _sprintRollResolver.IsSprinting && hasMovement;
            bool rollActive = currentState == CharacterActionStateId.Roll;
            bool strongAttackPressed = !rollActive && _heavyAttackResolver.TryResolve(
                actions.StrongAttack.WasPressedThisFrame(),
                actions.StrongAttack.WasReleasedThisFrame(),
                actions.Attack.IsPressed(),
                true);

            CharacterCommand? first = null;
            CharacterCommand? second = null;

            // Equipment and hand-mode are the only same-frame pair retained by the legacy path.
            if (actions.SwitchWeapon.WasPressedThisFrame())
            {
                first = CharacterCommand.Equipment(
                    EquipmentActionKind.SwitchRightWeapon);
            }
            else if (actions.SwitchShield.WasPressedThisFrame())
            {
                first = CharacterCommand.Equipment(
                    EquipmentActionKind.SwitchLeftWeapon);
            }
            else if (actions.SwitchFlask.WasPressedThisFrame())
            {
                first = CharacterCommand.Equipment(
                    EquipmentActionKind.SwitchQuickItem);
            }
            else if (actions.UseItem.WasPressedThisFrame())
            {
                first = CharacterCommand.Equipment(
                    EquipmentActionKind.UseQuickItem);
            }

            if (actions.TwoHanded.WasPressedThisFrame())
            {
                CharacterCommand handMode = CharacterCommand.Equipment(
                    EquipmentActionKind.ToggleHandMode);
                if (!first.HasValue) first = handMode;
                else second = handMode;
            }

            if (!first.HasValue)
            {
                if (strongAttackPressed)
                {
                    first = CharacterCommand.Attack(
                        AttackIntent.Heavy,
                        false,
                        sprinting);
                }
                else if (!rollActive && actions.SpecialAbility.WasPressedThisFrame())
                {
                    first = CharacterCommand.Attack(
                        AttackIntent.Special,
                        false,
                        false);
                }
                else if (!_heavyAttackResolver.ShouldSuppressLightAttack(
                    actions.Attack.WasPressedThisFrame()))
                {
                    first = CharacterCommand.Attack(
                        AttackIntent.Light,
                        false,
                        sprinting);
                }
                else if (actions.Guard.WasPressedThisFrame())
                {
                    first = CharacterCommand.Attack(
                        AttackIntent.Light,
                        true,
                        false);
                }
                else if (_sprintRollResolver.ShouldRoll(
                    actions.Roll.WasReleasedThisFrame()))
                {
                    first = CharacterCommand.Roll(moveInput, cameraYaw, true);
                }
                else if (actions.Jump.WasPressedThisFrame())
                {
                    first = CharacterCommand.Jump(sprinting);
                }
            }

            CharacterControlFrame frame = new CharacterControlFrame(
                moveInput, cameraYaw, sprinting, actions.Crouch.IsPressed(),
                actions.Guard.IsPressed(), actions.StrongAttack.IsPressed() && !rollActive);
            return new CharacterInputBatch(frame, first, second);
        }
    }
}
