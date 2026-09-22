using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    /// <summary>Consumes one submitted frame and ticks the actor after local input has updated.</summary>
    public sealed class CharacterActorTick : IPostTickable
    {
        private readonly Character _character;
        private readonly HealthComponent _healthComponent;
        private readonly IGameStateNotifier _gameStateNotifier;

        private CharacterInput _pendingInput;
        private bool _hasPendingInput;

        public CharacterActorTick(
            Character character,
            HealthComponent healthComponent,
            IGameStateNotifier gameStateNotifier)
        {
            _character = character;
            _healthComponent = healthComponent;
            _gameStateNotifier = gameStateNotifier;
        }

        public void Submit(in CharacterInput input)
        {
            _pendingInput = input;
            _hasPendingInput = true;
        }

        public void Clear()
        {
            _hasPendingInput = false;
        }

        public void PostTick()
        {
            if (!_hasPendingInput) return;

            CharacterInput input = _pendingInput;
            _hasPendingInput = false;
            GameState gameState = _gameStateNotifier.CurrentGameState;
            if (!_healthComponent.Stats.IsAlive
                || (_character.IsInputBlocked && !_character.IsInLadderOperation)
                || (gameState != GameState.Idle && gameState != GameState.Paused))
            {
                return;
            }

            if (gameState == GameState.Paused)
            {
                if (_character.IsInLadderOperation) return;
                input = new CharacterInput(input.MoveInput, input.CameraYaw, false, false, false, false);
            }

            _character.Tick(input);
        }
    }
}
