using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Interactions;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class GroundItemInteractCommand : EntityCommand, IInteractableCommand
    {
        private readonly GroundItem _groundItem;

        public int Priority => _groundItem.Priority;
        public Transform InteractionAnchor => _groundItem.InteractionAnchor;

        public GroundItemInteractCommand(Entity entity, GroundItem groundItem)
            : base(entity)
        {
            _groundItem = groundItem;
        }

        public Transform GetInteractionAnchor(IEntity actor) => _groundItem.InteractionAnchor;

        public bool CanInteract(IEntity actor) => _groundItem.CanInteract(actor);

        public InteractionPrompt GetPrompt(IEntity actor) => _groundItem.GetPrompt(actor);

        public InteractionPrompt GetFailurePrompt(IEntity actor) => _groundItem.GetFailurePrompt(actor);

        public async UniTask InteractAsync(IEntity actor, CancellationToken token)
        {
            if (!_groundItem.CanInteract(actor))
            {
                return;
            }

            _groundItem.SetState(GroundItemState.Busy);
            _groundItem.DisableInteractionCollider();

            if (!actor.TryGetComponent(out GroundItemCollectionCommand collectionCommand))
            {
                throw new InvalidOperationException(
                    $"{nameof(GroundItemCollectionCommand)} is not registered on entity {actor.Id}.");
            }

            collectionCommand.Collect(_groundItem);
            _groundItem.SetState(GroundItemState.Collected);

            await _groundItem.PlayPickupVfxAsync(CancellationToken.None);
            _groundItem.DestroyItem();
        }
    }
}
