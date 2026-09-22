namespace SoulsLike.Entities.Character.Runtime
{
    /// <summary>Reports the physical result and state started by one action execution.</summary>
    public readonly struct CharacterActionExecution
    {
        public CharacterAction.Result Result { get; }
        public CharacterAction.State StartedState { get; }

        public CharacterActionExecution(
            CharacterAction.Result result,
            CharacterAction.State startedState)
        {
            Result = result;
            StartedState = startedState;
        }
    }
}
