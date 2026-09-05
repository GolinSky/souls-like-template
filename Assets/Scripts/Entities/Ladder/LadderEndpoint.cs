using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.Ladder
{
    public sealed class LadderEndpoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private LadderEnd end;
        [SerializeField] private Transform interactionAnchor;
        [SerializeField] private int priority = 110;

        private LadderView _ladder;

        public LadderEnd End => end;
        public Transform InteractionAnchor => interactionAnchor;
        public int Priority => priority;

        private void Awake()
        {
            _ladder = GetComponentInParent<LadderView>();
            if (_ladder == null)
            {
                throw new System.InvalidOperationException(
                    $"{nameof(LadderEndpoint)} '{name}' requires a parent {nameof(LadderView)}.");
            }
        }

        public bool CanInteract(IEntity actor) => _ladder.CanInteract(actor, end);

        public InteractionPrompt GetPrompt(IEntity actor) => _ladder.GetPrompt(actor, end);

        public InteractionPrompt GetFailurePrompt(IEntity actor) =>
            _ladder.GetFailurePrompt(actor, end);

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _ladder.InteractAsync(actor, end, token);
    }
}
