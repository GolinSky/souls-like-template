using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;

namespace SoulsLike.Ui.Interaction
{
    public sealed class InteractionUi : BaseUi
    {
        private const string INTERACTION_PROMPT_FORMAT = "Press E to {0}";

        [SerializeField] private TMP_Text interactionText;

        private IInteractionPresenter Presenter { get; set; }

        public void AssignPresenter(IInteractionPresenter presenter)
        {
            Presenter = presenter;
        }

        public void Refresh()
        {
            if (Presenter.IsInteractionAvailable)
            {
                interactionText.text = string.Format(
                    INTERACTION_PROMPT_FORMAT,
                    Presenter.CurrentPrompt);
                Show();
                return;
            }

            Hide();
        }
    }
}
