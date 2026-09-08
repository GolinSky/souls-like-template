namespace SoulsLike.Ui.Interaction
{
    public interface IInteractionPresenter
    {
        bool IsInteractionAvailable { get; }
        string CurrentPrompt { get; }
    }
}
