using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.Ladder;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class LadderInteractCommand : EntityCommand, IInteractableCommand
    {
        private const int PRIORITY = 110;

        private readonly LadderView _ladderView;

        public int Priority => PRIORITY;
        public Transform InteractionAnchor => _ladderView.transform;

        public LadderInteractCommand(Entity entity, LadderView ladderView)
            : base(entity)
        {
            _ladderView = ladderView;
        }

        public Transform GetInteractionAnchor(IEntity actor) =>
            _ladderView.GetInteractionAnchor(GetActorEnd(actor));

        public bool CanInteract(IEntity actor) =>
            _ladderView.CanInteract(actor, GetActorEnd(actor));

        public InteractionPrompt GetPrompt(IEntity actor) =>
            _ladderView.GetPrompt(actor, GetActorEnd(actor));

        public InteractionPrompt GetFailurePrompt(IEntity actor) =>
            _ladderView.GetFailurePrompt(actor, GetActorEnd(actor));

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _ladderView.InteractAsync(actor, GetActorEnd(actor), token);

        private LadderEnd GetActorEnd(IEntity actor)
        {
            if (actor.TryGetComponent(out ViewEntity viewEntity))
            {
                return _ladderView.GetClosestEnd(viewEntity.transform.position);
            }

            return LadderEnd.Bottom;
        }
    }
}
