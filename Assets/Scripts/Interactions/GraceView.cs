using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Services.Travel.Data;
using UnityEngine;

namespace SoulsLike.Interactions
{
    public sealed class GraceView : MonoBehaviour, IInteractable
    {
        [SerializeField] private GraceId graceId;

        private IGracePresenter _presenter;

        public GraceId GraceId => graceId;
        public Transform InteractionAnchor => transform;

        public void AssignPresenter(IGracePresenter presenter) => _presenter = presenter;

        public bool CanInteract(IEntity actor) => _presenter.CanInteract();

        public InteractionPrompt GetPrompt(IEntity actor) => _presenter.GetPrompt(this);

        public InteractionPrompt GetFailurePrompt(IEntity actor) => _presenter.GetFailurePrompt();

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _presenter.InteractAsync(this, token);
    }
}
