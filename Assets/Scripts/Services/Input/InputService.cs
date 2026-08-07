using System;
using VContainer.Unity;

namespace MultiPlayerTemplate.Services
{
    public interface IInputService
    {
        ProjectInputActions.CharacterActions CharacterActions { get; }
    }
    public class InputService:IInputService, IInitializable, IDisposable
    {
        private readonly ProjectInputActions _projectInputActions;

        public ProjectInputActions.CharacterActions CharacterActions => _projectInputActions.Character;

        public InputService()
        {
            _projectInputActions = new ProjectInputActions();

        }
        public void Initialize()
        {
            _projectInputActions.Enable();
        }
        
        public void Dispose()
        {
            _projectInputActions?.Dispose();
        }
    }
}