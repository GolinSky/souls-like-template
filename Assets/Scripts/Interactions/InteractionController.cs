using System;
using System.Collections.Generic;
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
        private const int MAX_CANDIDATE_COLLIDERS = 64;
        private const float INTERACTION_RADIUS = 3f;
        private const float INTERACTION_RADIUS_SQR = INTERACTION_RADIUS * INTERACTION_RADIUS;
        private const float MIN_FACING_DOT = 0.258819f;

        private readonly Collider[] _colliderBuffer = new Collider[MAX_CANDIDATE_COLLIDERS];
        private readonly List<InteractionCandidate> _candidates = new(MAX_CANDIDATE_COLLIDERS);
        private readonly HashSet<IInteractable> _candidateInteractables = new();
        private readonly IInputService _inputService;
        private readonly IEntityLocator _entityLocator;
        private readonly ViewEntity _actorView;
        private readonly Character _character;
        private readonly LayerMask _interactionMask;

        private CancellationTokenSource _lifetimeCancellation;
        private InteractionCommand _interactionCommand;
        private IInteractable _currentInteractable;
        private bool _isInteracting;
        private bool _selectionCycled;

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

            IEntity actor = _entityLocator.GetEntity(_actorView.Id);
            if (!actor.TryGetComponent(out _interactionCommand))
            {
                throw new InvalidOperationException(
                    $"{nameof(InteractionCommand)} is not registered on entity {actor.Id}.");
            }
        }

        public void Tick()
        {
            if (_character.IsInLadderOperation)
            {
                ClearTarget();
                return;
            }

            RefreshCandidates();

            if (_currentInteractable != null
                && !_isInteracting
                && _inputService.CharacterActions.Interact.WasPressedThisFrame())
            {
                InteractAsync(_currentInteractable).Forget();
            }
        }

        public void CycleTarget()
        {
            if (_candidates.Count < 2)
            {
                return;
            }

            int currentIndex = _candidates.FindIndex(candidate =>
                ReferenceEquals(candidate.Interactable, _currentInteractable));
            int nextIndex = (currentIndex + 1) % _candidates.Count;
            _selectionCycled = true;
            SetCurrentInteractable(_candidates[nextIndex].Interactable);
        }

        public void ClearTarget()
        {
            _candidates.Clear();
            _candidateInteractables.Clear();
            _selectionCycled = false;
            SetCurrentInteractable(null);
        }

        public void Dispose()
        {
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        private void RefreshCandidates()
        {
            _candidates.Clear();
            _candidateInteractables.Clear();

            Transform actorTransform = _character.transform;
            int colliderCount = Physics.OverlapSphereNonAlloc(
                actorTransform.position,
                INTERACTION_RADIUS,
                _colliderBuffer,
                _interactionMask,
                QueryTriggerInteraction.Collide);

            for (int index = 0; index < colliderCount; index++)
            {
                Collider collider = _colliderBuffer[index];
                IInteractable interactable = collider.GetComponentInParent<IInteractable>();
                if (interactable == null
                    || !_candidateInteractables.Add(interactable)
                    || interactable is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                Vector3 offset = interactable.InteractionAnchor.position - actorTransform.position;
                float distanceSqr = offset.sqrMagnitude;
                if (distanceSqr > INTERACTION_RADIUS_SQR)
                {
                    continue;
                }

                offset.y = 0f;
                float alignment = offset.sqrMagnitude <= Mathf.Epsilon
                    ? 1f
                    : Vector3.Dot(actorTransform.forward, offset.normalized);
                if (alignment < MIN_FACING_DOT)
                {
                    continue;
                }

                _candidates.Add(new InteractionCandidate(interactable, alignment, distanceSqr));
            }

            _candidates.Sort(CompareCandidates);
            SetCurrentInteractable(SelectStableInteractable());
        }

        private IInteractable SelectStableInteractable()
        {
            if (_candidates.Count == 0)
            {
                _selectionCycled = false;
                return null;
            }

            IInteractable bestInteractable = _candidates[0].Interactable;
            foreach (InteractionCandidate candidate in _candidates)
            {
                if (ReferenceEquals(candidate.Interactable, _currentInteractable))
                {
                    if (_selectionCycled
                        || candidate.Interactable.Priority == bestInteractable.Priority)
                    {
                        return _currentInteractable;
                    }

                    break;
                }
            }

            _selectionCycled = false;
            return bestInteractable;
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            _currentInteractable = interactable;
            InteractionPrompt prompt = interactable == null
                ? default
                : _interactionCommand.GetPrompt(interactable);
            if (prompt.Equals(CurrentPrompt))
            {
                return;
            }

            CurrentPrompt = prompt;
            PromptChanged?.Invoke(prompt);
        }

        private async UniTask InteractAsync(IInteractable interactable)
        {
            if (!_interactionCommand.CanInteract(interactable))
            {
                InteractionFailed?.Invoke(
                    _interactionCommand.GetFailurePrompt(interactable));
                return;
            }

            _isInteracting = true;
            try
            {
                await _interactionCommand.InteractAsync(
                        interactable,
                        _lifetimeCancellation.Token)
                    .SuppressCancellationThrow();
            }
            finally
            {
                _isInteracting = false;
            }
        }

        private static int CompareCandidates(
            InteractionCandidate first,
            InteractionCandidate second)
        {
            int priorityComparison = second.Interactable.Priority.CompareTo(
                first.Interactable.Priority);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            int alignmentComparison = second.Alignment.CompareTo(first.Alignment);
            return alignmentComparison != 0
                ? alignmentComparison
                : first.DistanceSqr.CompareTo(second.DistanceSqr);
        }

        private readonly struct InteractionCandidate
        {
            public IInteractable Interactable { get; }
            public float Alignment { get; }
            public float DistanceSqr { get; }

            public InteractionCandidate(
                IInteractable interactable,
                float alignment,
                float distanceSqr)
            {
                Interactable = interactable;
                Alignment = alignment;
                DistanceSqr = distanceSqr;
            }
        }
    }
}
