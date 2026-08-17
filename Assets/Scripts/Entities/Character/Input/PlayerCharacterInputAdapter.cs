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
        private readonly CharacterCommandFactory _commandFactory;
        private readonly SprintRollGestureResolver _sprintRollResolver;
        private readonly HeavyAttackGestureResolver _heavyAttackResolver;

        public PlayerCharacterInputAdapter(
            IInputService inputService,
            ICameraService cameraService,
            CharacterCommandFactory commandFactory,
            SprintRollGestureResolver sprintRollResolver,
            HeavyAttackGestureResolver heavyAttackResolver)
        {
            _inputService = inputService;
            _cameraService = cameraService;
            _commandFactory = commandFactory;
            _sprintRollResolver = sprintRollResolver;
            _heavyAttackResolver = heavyAttackResolver;
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

            ICharacterCommand first = null;
            ICharacterCommand second = null;
            int commandCount = 0;

            // Equipment and hand-mode are the only same-frame pair retained by the legacy path.
            if (actions.SwitchWeapon.WasPressedThisFrame())
            {
                first = _commandFactory.CreateEquipmentAction(0);
                commandCount = 1;
            }
            else if (actions.SwitchShield.WasPressedThisFrame())
            {
                first = _commandFactory.CreateEquipmentAction(1);
                commandCount = 1;
            }
            else if (actions.SwitchFlask.WasPressedThisFrame())
            {
                first = _commandFactory.CreateEquipmentAction(2);
                commandCount = 1;
            }
            else if (actions.UseItem.WasPressedThisFrame())
            {
                first = _commandFactory.CreateEquipmentAction(3);
                commandCount = 1;
            }

            if (actions.TwoHanded.WasPressedThisFrame())
            {
                ICharacterCommand handMode = _commandFactory.CreateEquipmentAction(4);
                if (commandCount == 0) first = handMode;
                else second = handMode;
                commandCount++;
            }

            if (commandCount == 0)
            {
                if (strongAttackPressed)
                {
                    first = _commandFactory.CreateAttack(
                        AttackIntent.Heavy,
                        false,
                        sprinting);
                }
                else if (!rollActive && actions.SpecialAbility.WasPressedThisFrame())
                {
                    first = _commandFactory.CreateAttack(
                        AttackIntent.Special,
                        false,
                        false);
                }
                else if (!_heavyAttackResolver.ShouldSuppressLightAttack(
                    actions.Attack.WasPressedThisFrame()))
                {
                    first = _commandFactory.CreateAttack(
                        AttackIntent.Light,
                        false,
                        sprinting);
                }
                else if (actions.Guard.WasPressedThisFrame())
                {
                    first = _commandFactory.CreateAttack(
                        AttackIntent.Light,
                        true,
                        false);
                }
                else if (_sprintRollResolver.ShouldRoll(
                    actions.Roll.WasReleasedThisFrame()))
                {
                    first = _commandFactory.CreateRoll(moveInput, cameraYaw, true);
                }
                else if (actions.Jump.WasPressedThisFrame())
                {
                    first = _commandFactory.CreateJump(sprinting);
                }
                commandCount = first == null ? 0 : 1;
            }

            CharacterControlFrame frame = new CharacterControlFrame(
                moveInput, cameraYaw, sprinting, actions.Crouch.IsPressed(),
                actions.Guard.IsPressed(), actions.StrongAttack.IsPressed() && !rollActive);
            return new CharacterInputBatch(frame, first, second, commandCount);
        }
    }
}
