using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;

namespace SoulsLike.Ui.Interaction
{
    public sealed class InteractionUi : BaseUi
    {
        private const string INTERACTION_PROMPT = "Press E";

        [SerializeField] private TMP_Text interactionText;

        private IInteractionPresenter Presenter { get; set; }

        public void AssignPresenter(IInteractionPresenter presenter)
        {
            Presenter = presenter;
        }

        public void Refresh()
        {
            interactionText.text = INTERACTION_PROMPT;

            if (Presenter.IsInteractionAvailable)
            {
                Show();
                return;
            }

            Hide();
        }
    }
}
