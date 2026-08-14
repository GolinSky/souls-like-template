using System;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace SoulsLike.Services
{
    public interface IInputService
    {
        ProjectInputActions.CharacterActions CharacterActions { get; }
        ProjectInputActions.UIActions UIActions { get; }
        InputAction OpenInventoryAction { get; }
        InputAction OpenEquipmentAction { get; }
        InputAction ToggleLoreAction { get; }
        InputAction ToggleSimpleViewAction { get; }
        InputAction UnequipAction { get; }
    }

    public sealed class InputService : IInputService, IInitializable, IDisposable
    {
        private readonly ProjectInputActions _projectInputActions;

        public ProjectInputActions.CharacterActions CharacterActions => _projectInputActions.Character;
        public ProjectInputActions.UIActions UIActions => _projectInputActions.UI;
        public InputAction OpenInventoryAction { get; }
        public InputAction OpenEquipmentAction { get; }
        public InputAction ToggleLoreAction { get; }
        public InputAction ToggleSimpleViewAction { get; }
        public InputAction UnequipAction { get; }

        public InputService()
        {
            _projectInputActions = new ProjectInputActions();
            // todo: instead of hardcoded action - add them in ProjectInputActions asset
            OpenInventoryAction = CreateMenuAction(
                "OpenInventory",
                "<Keyboard>/i",
                "<Gamepad>/select");
            OpenEquipmentAction = CreateMenuAction(
                "OpenEquipment",
                "<Keyboard>/o",
                "<Gamepad>/start");
            ToggleLoreAction = CreateMenuAction(
                "ToggleLore",
                "<Keyboard>/r",
                "<Gamepad>/buttonNorth");
            ToggleSimpleViewAction = CreateMenuAction(
                "ToggleSimpleView",
                "<Keyboard>/f",
                "<Gamepad>/rightStickPress");
            UnequipAction = CreateMenuAction(
                "Unequip",
                "<Keyboard>/delete",
                "<Gamepad>/buttonWest");
        }

        public void Initialize()
        {
            _projectInputActions.Enable();
            OpenInventoryAction.Enable();
            OpenEquipmentAction.Enable();
            ToggleLoreAction.Enable();
            ToggleSimpleViewAction.Enable();
            UnequipAction.Enable();
        }

        public void Dispose()
        {
            OpenInventoryAction.Dispose();
            OpenEquipmentAction.Dispose();
            ToggleLoreAction.Dispose();
            ToggleSimpleViewAction.Dispose();
            UnequipAction.Dispose();
            _projectInputActions.Dispose();
        }

        private static InputAction CreateMenuAction(
            string name,
            string keyboardBinding,
            string gamepadBinding)
        {
            var action = new InputAction(name, InputActionType.Button);
            action.AddBinding(keyboardBinding);
            action.AddBinding(gamepadBinding);
            return action;
        }
    }
}
