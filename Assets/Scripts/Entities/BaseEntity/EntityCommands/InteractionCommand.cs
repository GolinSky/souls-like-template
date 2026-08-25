using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Interactions;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class InteractionCommand : EntityCommand
    {
        private readonly Entity _actor;

        public InteractionCommand(Entity actor)
            : base(actor)
        {
            _actor = actor;
        }

        public bool CanInteract(IInteractable interactable) =>
            _actor.EntityType == EntityType.Player
            && interactable.CanInteract(_actor);

        public InteractionPrompt GetPrompt(IInteractable interactable) =>
            interactable.GetPrompt(_actor);

        public InteractionPrompt GetFailurePrompt(IInteractable interactable) =>
            interactable.GetFailurePrompt(_actor);

        public UniTask InteractAsync(IInteractable interactable, CancellationToken token) =>
            interactable.InteractAsync(_actor, token);
    }
}
