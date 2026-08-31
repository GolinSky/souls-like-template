using System;
using UnityEngine;
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
        InputAction ToggleCheatsAction { get; }
        InputAction UnequipAction { get; }
        InputAction UiBackAction { get; }
        bool WasUiBackConsumedThisFrame { get; }

        void ConsumeUiBack();
    }

    public sealed class InputService : IInputService, IInitializable, IDisposable
    {
        private readonly ProjectInputActions _projectInputActions;
        private int _uiBackConsumedFrame = -1;

        public ProjectInputActions.CharacterActions CharacterActions => _projectInputActions.Character;
        public ProjectInputActions.UIActions UIActions => _projectInputActions.UI;
        public InputAction OpenInventoryAction { get; }
        public InputAction OpenEquipmentAction { get; }
        public InputAction ToggleLoreAction { get; }
        public InputAction ToggleSimpleViewAction { get; }
        public InputAction ToggleCheatsAction { get; }
        public InputAction UnequipAction { get; }
        public InputAction UiBackAction { get; }
        public bool WasUiBackConsumedThisFrame => _uiBackConsumedFrame == Time.frameCount;

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
            ToggleCheatsAction = new InputAction(
                "ToggleCheats",
                InputActionType.Button,
                "<Keyboard>/f12");
            UnequipAction = CreateMenuAction(
                "Unequip",
                "<Keyboard>/delete",
                "<Gamepad>/buttonWest");
            UiBackAction = CreateMenuAction(
                "UiBack",
                "<Keyboard>/q",
                "<Gamepad>/buttonEast");
        }

        public void Initialize()
        {
            _projectInputActions.Enable();
            OpenInventoryAction.Enable();
            OpenEquipmentAction.Enable();
            ToggleLoreAction.Enable();
            ToggleSimpleViewAction.Enable();
            ToggleCheatsAction.Enable();
            UnequipAction.Enable();
            UiBackAction.Enable();
        }

        public void Dispose()
        {
            OpenInventoryAction.Dispose();
            OpenEquipmentAction.Dispose();
            ToggleLoreAction.Dispose();
            ToggleSimpleViewAction.Dispose();
            ToggleCheatsAction.Dispose();
            UnequipAction.Dispose();
            UiBackAction.Dispose();
            _projectInputActions.Dispose();
        }

        public void ConsumeUiBack()
        {
            _uiBackConsumedFrame = Time.frameCount;
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
