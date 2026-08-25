using System;
using SoulsLike.Interactions;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike.Ui.Interaction
{
    public sealed class InteractionUiController : UiController, IInitializable, IInteractionPresenter, IDisposable
    {
        private readonly InteractionController _interactionController;
        private InteractionUi _interactionUi;

        public bool IsInteractionAvailable => _interactionController.CurrentPrompt.IsVisible;

        public InteractionUiController(
            IUiService uiService,
            InteractionController interactionController)
            : base(uiService)
        {
            _interactionController = interactionController;
        }

        public void Initialize()
        {
            _interactionUi = CreateUi<InteractionUi>();
            _interactionUi.AssignPresenter(this);
            _interactionController.PromptChanged += OnPromptChanged;
            _interactionUi.Refresh();
        }

        public void Dispose()
        {
            _interactionController.PromptChanged -= OnPromptChanged;
        }

        private void OnPromptChanged(InteractionPrompt _)
        {
            _interactionUi.Refresh();
        }
    }
}
