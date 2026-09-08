using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Services;
using SoulsLike.Services.Layer;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Interactions
{
    public sealed class InteractionController : IInitializable, IDisposable
    {
        private const int MAX_PROBE_COLLIDERS = 64;
        private const float INTERACTION_RADIUS = 0.35f;
        private const float INTERACTION_REACH = 1.5f;
        private const float INTERACTION_REACH_SQR = INTERACTION_REACH * INTERACTION_REACH;
        private const float PROBE_HEIGHT = 0.5f;
        private const float PROBE_DISTANCE = INTERACTION_REACH - INTERACTION_RADIUS;

        private readonly Collider[] _overlapBuffer = new Collider[MAX_PROBE_COLLIDERS];
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[MAX_PROBE_COLLIDERS];
        private readonly IInputService _inputService;
        private readonly IEntityLocator _entityLocator;
        private readonly ViewEntity _actorView;
        private readonly Character _character;
        private readonly LayerMask _interactionMask;

        private CancellationTokenSource _lifetimeCancellation;
        private IEntity _actorEntity;
        private InteractionCommand _interactionCommand;
        private IInteractableCommand _currentCommand;
        private bool _isInteracting;

        public event Action<InteractionPrompt> PromptChanged;
        public event Action<InteractionPrompt> InteractionFailed;

        public InteractionPrompt CurrentPrompt { get; private set; }

        public InteractionController(
            IInputService inputService,
            IEntityLocator entityLocator,
            ViewEntity actorView,
            Character character,
            ILayerService layerService)
        {
            _inputService = inputService;
            _entityLocator = entityLocator;
            _actorView = actorView;
            _character = character;
            _interactionMask = layerService.GetMask(LayerMaskName.InteractionProbe);
        }

        public void Initialize()
        {
            _lifetimeCancellation = new CancellationTokenSource();

            _actorEntity = _entityLocator.GetEntity(_actorView.Id);
            if (!_actorEntity.TryGetComponent(out _interactionCommand))
            {
                throw new InvalidOperationException(
                    $"{nameof(InteractionCommand)} is not registered on entity {_actorEntity.Id}.");
            }
        }

        public void Tick()
        {
            if (_character.IsInLadderOperation)
            {
                ClearTarget();
                return;
            }

            RefreshTarget();

            if (_currentCommand != null
                && !_isInteracting
                && _inputService.CharacterActions.Interact.WasPressedThisFrame())
            {
                InteractAsync(_currentCommand).Forget();
            }
        }

        public void ClearTarget()
        {
            SetCurrentTarget(null);
        }

        public void Dispose()
        {
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        private void RefreshTarget()
        {
            Transform actorTransform = _character.transform;
            Vector3 actorPosition = actorTransform.position;
            Vector3 probeOrigin = actorPosition + Vector3.up * PROBE_HEIGHT;
            Vector3 probeDirection = actorTransform.forward;
            probeDirection.y = 0f;
            probeDirection.Normalize();
            Vector3 probeEnd = probeOrigin + probeDirection * PROBE_DISTANCE;
            IInteractableCommand selectedCommand = null;
            float selectedLateralDistanceSqr = float.PositiveInfinity;
            float selectedAnchorDistanceSqr = float.PositiveInfinity;

            int hitCount = Physics.SphereCastNonAlloc(
                probeOrigin,
                INTERACTION_RADIUS,
                probeDirection,
                _hitBuffer,
                PROBE_DISTANCE,
                _interactionMask,
                QueryTriggerInteraction.Collide);

            for (int index = 0; index < hitCount; index++)
            {
                SelectCandidate(
                    _hitBuffer[index].collider,
                    actorPosition,
                    probeOrigin,
                    probeEnd,
                    probeDirection,
                    ref selectedCommand,
                    ref selectedLateralDistanceSqr,
                    ref selectedAnchorDistanceSqr);
            }

            int overlapCount = Physics.OverlapSphereNonAlloc(
                probeOrigin,
                INTERACTION_RADIUS,
                _overlapBuffer,
                _interactionMask,
                QueryTriggerInteraction.Collide);
            for (int index = 0; index < overlapCount; index++)
            {
                SelectCandidate(
                    _overlapBuffer[index],
                    actorPosition,
                    probeOrigin,
                    probeEnd,
                    probeDirection,
                    ref selectedCommand,
                    ref selectedLateralDistanceSqr,
                    ref selectedAnchorDistanceSqr);
            }

            SetCurrentTarget(selectedCommand);
        }

        private void SelectCandidate(
            Collider collider,
            Vector3 actorPosition,
            Vector3 probeOrigin,
            Vector3 probeEnd,
            Vector3 probeDirection,
            ref IInteractableCommand selectedCommand,
            ref float selectedLateralDistanceSqr,
            ref float selectedAnchorDistanceSqr)
        {
            if (!_entityLocator.TryGetEntity(collider, out IEntity targetEntity)
                || !targetEntity.TryGetComponent(out IInteractableCommand command)
                || (command is Behaviour behaviour && !behaviour.isActiveAndEnabled))
            {
                return;
            }

            Transform anchor = command.GetInteractionAnchor(_actorEntity);
            if (anchor == null
                || (anchor.position - actorPosition).sqrMagnitude
                > INTERACTION_REACH_SQR)
            {
                return;
            }

            Vector3 closestPoint = collider.ClosestPoint(probeEnd);
            Vector3 offset = closestPoint - probeOrigin;
            offset.y = 0f;
            float forwardDistance = Vector3.Dot(offset, probeDirection);
            if (forwardDistance <= 0f)
            {
                return;
            }

            Vector3 interactionAnchorOffset = command.InteractionAnchor.position - actorPosition;
            interactionAnchorOffset.y = 0f;
            float anchorDistanceSqr = interactionAnchorOffset.sqrMagnitude;
            if (anchorDistanceSqr > INTERACTION_REACH_SQR
                || Vector3.Dot(interactionAnchorOffset, probeDirection) < 0f)
            {
                return;
            }

            float lateralDistanceSqr =
                (interactionAnchorOffset
                - probeDirection * Vector3.Dot(interactionAnchorOffset, probeDirection)).sqrMagnitude;
            if (lateralDistanceSqr < selectedLateralDistanceSqr
                || Mathf.Approximately(lateralDistanceSqr, selectedLateralDistanceSqr)
                && anchorDistanceSqr < selectedAnchorDistanceSqr)
            {
                selectedCommand = command;
                selectedLateralDistanceSqr = lateralDistanceSqr;
                selectedAnchorDistanceSqr = anchorDistanceSqr;
            }
        }

        private void SetCurrentTarget(IInteractableCommand command)
        {
            _currentCommand = command;
            InteractionPrompt prompt = command == null
                ? default
                : _interactionCommand.GetPrompt(command);
            if (prompt.Equals(CurrentPrompt))
            {
                return;
            }

            CurrentPrompt = prompt;
            PromptChanged?.Invoke(prompt);
        }

        private async UniTask InteractAsync(IInteractableCommand command)
        {
            if (!_interactionCommand.CanInteract(command))
            {
                InteractionFailed?.Invoke(
                    _interactionCommand.GetFailurePrompt(command));
                return;
            }

            _isInteracting = true;
            try
            {
                await _interactionCommand.InteractAsync(
                        command,
                        _lifetimeCancellation.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                _isInteracting = false;
            }
        }
    }
}
