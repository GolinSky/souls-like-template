using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class GraceInteractCommand : EntityCommand, IInteractableCommand
    {
        private readonly GraceView _graceView;
        private readonly IGracePresenter _presenter;

        public int Priority => _graceView.Priority;
        public Transform InteractionAnchor => _graceView.InteractionAnchor;

        public GraceInteractCommand(
            Entity entity,
            GraceView graceView,
            IGracePresenter presenter)
            : base(entity)
        {
            _graceView = graceView;
            _presenter = presenter;
        }

        public Transform GetInteractionAnchor(IEntity actor) => _graceView.InteractionAnchor;

        public bool CanInteract(IEntity actor) => _presenter.CanInteract();

        public InteractionPrompt GetPrompt(IEntity actor) => _presenter.GetPrompt(_graceView);

        public InteractionPrompt GetFailurePrompt(IEntity actor) => _presenter.GetFailurePrompt();

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _presenter.InteractAsync(_graceView, token);
    }
}
