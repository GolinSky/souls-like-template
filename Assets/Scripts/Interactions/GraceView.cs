using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Travel.Data;
using UnityEngine;

namespace SoulsLike.Interactions
{
    public sealed class GraceView : MonoBehaviour, IInteractable
    {
        private const float GROUND_RAY_ORIGIN_HEIGHT = 1f;
        private const float GROUND_RAY_DISTANCE = 10f;

        [SerializeField] private GraceId graceId;

        private IGracePresenter _presenter;

        public GraceId GraceId => graceId;
        public Transform InteractionAnchor => transform;

        public Vector3 GetGroundSpawnPosition()
        {
            int walkableLayerMask = LayerMask.GetMask(nameof(LayerName.Walkable));
            Vector3 origin = transform.position + Vector3.up * GROUND_RAY_ORIGIN_HEIGHT;
            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    GROUND_RAY_DISTANCE,
                    walkableLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            throw new InvalidOperationException(
                $"Grace '{name}' has no Walkable ground below its interaction anchor.");
        }

        public void AssignPresenter(IGracePresenter presenter) => _presenter = presenter;

        public bool CanInteract(IEntity actor) => _presenter.CanInteract();

        public InteractionPrompt GetPrompt(IEntity actor) => _presenter.GetPrompt(this);

        public InteractionPrompt GetFailurePrompt(IEntity actor) => _presenter.GetFailurePrompt();

        public UniTask InteractAsync(IEntity actor, CancellationToken token) =>
            _presenter.InteractAsync(this, token);
    }
}
