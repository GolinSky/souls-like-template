using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.Elevator;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class ElevatorInteractCommand : EntityCommand, IInteractableCommand
    {
        private readonly ElevatorEndpoint _endpoint;

        public ElevatorInteractCommand(Entity entity, ElevatorEndpoint endpoint)
            : base(entity)
        {
            _endpoint = endpoint;
        }

        public Transform InteractionAnchor => _endpoint.InteractionAnchor;

        public Transform GetInteractionAnchor(IEntity actor) => _endpoint.InteractionAnchor;

        public bool CanInteract(IEntity actor) =>
            _endpoint.IsActorAllowed(actor) && _endpoint.System.CanInteract(_endpoint, actor);

        public InteractionPrompt GetPrompt(IEntity actor) => CanInteract(actor)
            ? new InteractionPrompt(_endpoint.System.GetPrompt())
            : GetFailurePrompt(actor);

        public InteractionPrompt GetFailurePrompt(IEntity actor) =>
            new(_endpoint.System.GetFailurePrompt(_endpoint, actor));

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _endpoint.System.InteractAsync(_endpoint, actor, token);
    }
}
