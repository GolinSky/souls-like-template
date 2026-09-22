namespace SoulsLike.Entities.Character.Runtime
{
    /// <summary>Executes an accepted semantic action through the actor's outer adapters.</summary>
    public interface ICharacterActionExecutor
    {
        CharacterActionExecution Execute(in CharacterAction action);
    }
}
