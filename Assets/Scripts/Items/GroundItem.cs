using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Items
{
    public enum GroundItemRewardType
    {
        Item = 0,
        Currency = 1
    }

    public enum GroundItemState
    {
        Available = 0,
        Busy = 1,
        Collected = 2
    }

    public sealed class GroundItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private GroundItemRewardType rewardType;
        [SerializeField] private ItemId itemId;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField, Min(1)] private int currencyAmount = 1;
        [SerializeField] private string saveIdentifier;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Transform interactionAnchor;
        [SerializeField] private GroundItemVfx pickupVfx;
        [SerializeField] private int priority = 100;

        public GroundItemRewardType RewardType => rewardType;
        public GroundItemState State { get; private set; }
        public ItemId ItemId => itemId;
        public int Quantity => quantity;
        public int CurrencyAmount => currencyAmount;
        public string SaveIdentifier => saveIdentifier;
        public Transform InteractionAnchor => interactionAnchor;
        public int Priority => priority;

        public bool CanInteract(IEntity actor) => State == GroundItemState.Available;

        public InteractionPrompt GetPrompt(IEntity actor) => State switch
        {
            GroundItemState.Available => new InteractionPrompt(
                rewardType == GroundItemRewardType.Currency
                    ? "Recover runes"
                    : "Pick up item"),
            GroundItemState.Busy => new InteractionPrompt("Busy"),
            _ => default
        };

        public InteractionPrompt GetFailurePrompt(IEntity actor) => State switch
        {
            GroundItemState.Busy => new InteractionPrompt("Item is busy"),
            GroundItemState.Collected => new InteractionPrompt("Item already collected"),
            _ => new InteractionPrompt("Cannot collect item")
        };

        public async UniTask InteractAsync(IEntity actor, CancellationToken token)
        {
            if (!CanInteract(actor))
            {
                return;
            }

            State = GroundItemState.Busy;
            interactionCollider.enabled = false;

            if (!actor.TryGetComponent(out GroundItemCollectionCommand collectionCommand))
            {
                throw new System.InvalidOperationException(
                    $"{nameof(GroundItemCollectionCommand)} is not registered on entity {actor.Id}.");
            }

            collectionCommand.Collect(this);
            State = GroundItemState.Collected;

            await pickupVfx.PlayPickupAsync(CancellationToken.None);
            Destroy(gameObject);
        }
    }
}
