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

        public bool CanInteract(IInteractableCommand command) =>
            _actor.EntityType == EntityType.Player
            && command.CanInteract(_actor);

        public InteractionPrompt GetPrompt(IInteractableCommand command) =>
            command.GetPrompt(_actor);

        public InteractionPrompt GetFailurePrompt(IInteractableCommand command) =>
            command.GetFailurePrompt(_actor);

        public UniTask InteractAsync(IInteractableCommand command, CancellationToken token) =>
            command.InteractAsync(_actor, token);
    }
}
