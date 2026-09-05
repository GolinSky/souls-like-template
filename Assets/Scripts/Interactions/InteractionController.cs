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
        private readonly HashSet<IEntity> _candidateEntities = new();
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

            RefreshCandidates();

            if (_currentCommand != null
                && !_isInteracting
                && _inputService.CharacterActions.Interact.WasPressedThisFrame())
            {
                InteractAsync(_currentCommand).Forget();
            }
        }

        public void CycleTarget()
        {
            if (_candidates.Count < 2)
            {
                return;
            }

            int currentIndex = _candidates.FindIndex(candidate =>
                ReferenceEquals(candidate.Command, _currentCommand));
            int nextIndex = (currentIndex + 1) % _candidates.Count;
            _selectionCycled = true;
            SetCurrentTarget(_candidates[nextIndex].Command);
        }

        public void ClearTarget()
        {
            _candidates.Clear();
            _candidateEntities.Clear();
            _selectionCycled = false;
            SetCurrentTarget(null);
        }

        public void Dispose()
        {
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        private void RefreshCandidates()
        {
            _candidates.Clear();
            _candidateEntities.Clear();

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
                if (!_entityLocator.TryGetEntity(collider, out IEntity targetEntity))
                {
                    continue;
                }

                if (!_candidateEntities.Add(targetEntity))
                {
                    continue;
                }

                if (!targetEntity.TryGetComponent(out IInteractableCommand command))
                {
                    continue;
                }

                if (command is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                Transform anchor = command.GetInteractionAnchor(_actorEntity);
                if (anchor == null)
                {
                    continue;
                }

                Vector3 offset = anchor.position - actorTransform.position;
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

                _candidates.Add(new InteractionCandidate(targetEntity, command, alignment, distanceSqr));
            }

            _candidates.Sort(CompareCandidates);
            SetCurrentTarget(SelectStableCandidate());
        }

        private IInteractableCommand SelectStableCandidate()
        {
            if (_candidates.Count == 0)
            {
                _selectionCycled = false;
                return null;
            }

            IInteractableCommand bestCommand = _candidates[0].Command;
            foreach (InteractionCandidate candidate in _candidates)
            {
                if (ReferenceEquals(candidate.Command, _currentCommand))
                {
                    if (_selectionCycled
                        || candidate.Command.Priority == bestCommand.Priority)
                    {
                        return _currentCommand;
                    }

                    break;
                }
            }

            _selectionCycled = false;
            return bestCommand;
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

        private static int CompareCandidates(
            InteractionCandidate first,
            InteractionCandidate second)
        {
            int priorityComparison = second.Command.Priority.CompareTo(
                first.Command.Priority);
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
            public IEntity Entity { get; }
            public IInteractableCommand Command { get; }
            public float Alignment { get; }
            public float DistanceSqr { get; }

            public InteractionCandidate(
                IEntity entity,
                IInteractableCommand command,
                float alignment,
                float distanceSqr)
            {
                Entity = entity;
                Command = command;
                Alignment = alignment;
                DistanceSqr = distanceSqr;
            }
        }
    }
}
