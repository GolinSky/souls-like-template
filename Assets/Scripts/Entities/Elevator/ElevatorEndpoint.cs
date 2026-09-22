using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorEndpoint : MonoBehaviour
    {
        [SerializeField] private ElevatorEndpointType endpointType;
        [SerializeField] private ElevatorFloor floor;
        [SerializeField] private ViewEntity viewEntity;
        [SerializeField] private Transform interactionAnchor;
        [SerializeField] private bool allowEnemyActivation;
        [SerializeField] private GameObject unavailableIndicator;
        [SerializeField] private Transform animatedTransform;
        [SerializeField] private Vector3 idleLocalPosition;
        [SerializeField] private Vector3 activatedLocalPosition;
        [SerializeField] private Quaternion idleLocalRotation = Quaternion.identity;
        [SerializeField] private Quaternion activatedLocalRotation = Quaternion.identity;
        [SerializeField, Min(0.01f)] private float animationDuration = 0.1f;
        [SerializeField, Min(0f)] private float leverReturnDelay = 0.15f;

        private readonly Dictionary<int, Collider> _actorColliders = new();
        private IElevatorPresenter _presenter;
        private ElevatorView _elevator;
        private IEntityLocator _entityLocator;
        private Entity _entity;
        private ViewEntity _viewEntity;
        private bool _isArmed = true;
        private float _leverReturnTime;
        private float _animationProgress;
        private bool _isActivatedVisual;

        public ElevatorEndpointType EndpointType => endpointType;
        public ElevatorFloor Floor => floor;
        public Transform InteractionAnchor => interactionAnchor == null ? transform : interactionAnchor;
        public EntityType EntityType =>
            endpointType == ElevatorEndpointType.PressurePlate
                ? EntityType.ElevatorPressurePlate
                : EntityType.ElevatorLever;
        public ViewEntity ViewEntity => viewEntity;
        public IElevatorPresenter Presenter => _presenter;
        public ElevatorView Elevator => _elevator;

        public void Construct(
            IElevatorPresenter presenter,
            ElevatorView elevator,
            IEntityLocator entityLocator,
            Entity entity,
            ViewEntity viewEntity)
        {
            _presenter = presenter;
            _elevator = elevator;
            _entityLocator = entityLocator;
            _entity = entity;
            _viewEntity = viewEntity;
            if (animatedTransform != null)
            {
                animatedTransform.localPosition = idleLocalPosition;
                animatedTransform.localRotation = idleLocalRotation;
            }
        }

        public void DisposeEntity()
        {
            _actorColliders.Clear();
            _isArmed = true;
            _isActivatedVisual = false;
            _entity = null;
            _viewEntity = null;
            _presenter = null;
            _elevator = null;
            _entityLocator = null;
        }

        public bool IsActorAllowed(IEntity actor) => actor.EntityType
                == EntityType.Player
            || allowEnemyActivation && actor.EntityType
                == EntityType.Enemy;

        public ElevatorFloor GetRequestedFloor(ElevatorFloor currentFloor) =>
            endpointType == ElevatorEndpointType.PressurePlate
                ? currentFloor == ElevatorFloor.Bottom ? ElevatorFloor.Top : ElevatorFloor.Bottom
                : floor;

        public void ApplyAvailability(bool available)
        {
            if (unavailableIndicator != null)
            {
                unavailableIndicator.SetActive(!available);
            }
        }

        public void PlayActivation()
        {
            _isActivatedVisual = true;
            _leverReturnTime = endpointType == ElevatorEndpointType.CallLever
                ? Time.time + leverReturnDelay
                : float.PositiveInfinity;
        }

        public void ResetPressurePlate()
        {
            if (endpointType != ElevatorEndpointType.PressurePlate)
            {
                return;
            }

            _isActivatedVisual = false;
            if (_actorColliders.Count == 0)
            {
                _isArmed = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (endpointType != ElevatorEndpointType.PressurePlate || _entityLocator == null)
            {
                return;
            }

            if (!_entityLocator.TryGetEntity(other, out IEntity actor) || !IsActorAllowed(actor))
            {
                return;
            }

            if (_actorColliders.ContainsKey(other.GetInstanceID()))
            {
                return;
            }

            _actorColliders.Add(other.GetInstanceID(), other);
            if (!_isArmed)
            {
                return;
            }

            _isArmed = false;
            _isActivatedVisual = true;
            DispatchInteractionAsync(actor).Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (endpointType != ElevatorEndpointType.PressurePlate)
            {
                return;
            }

            _actorColliders.Remove(other.GetInstanceID());
            if (_actorColliders.Count == 0)
            {
                _isArmed = true;
                _isActivatedVisual = false;
            }
        }

        private void Update()
        {
            PurgeDestroyedActorColliders();
            if (endpointType == ElevatorEndpointType.CallLever
                && _isActivatedVisual
                && Time.time >= _leverReturnTime)
            {
                _isActivatedVisual = false;
            }

            if (animatedTransform == null)
            {
                return;
            }

            float target = _isActivatedVisual ? 1f : 0f;
            _animationProgress = Mathf.MoveTowards(
                _animationProgress,
                target,
                Time.deltaTime / animationDuration);
            animatedTransform.localPosition = Vector3.Lerp(
                idleLocalPosition,
                activatedLocalPosition,
                _animationProgress);
            animatedTransform.localRotation = Quaternion.Slerp(
                idleLocalRotation,
                activatedLocalRotation,
                _animationProgress);
        }

        private async UniTask DispatchInteractionAsync(IEntity actor)
        {
            if (!_entityLocator.TryGetEntity(_viewEntity.Id, out IEntity targetEntity)
                || !targetEntity.TryGetComponent(out ElevatorInteractCommand command))
            {
                throw new InvalidOperationException(
                    $"Elevator endpoint '{name}' is not registered with {nameof(ElevatorInteractCommand)}.");
            }

            await command.InteractAsync(actor, CancellationToken.None);
        }

        private void OnDisable()
        {
            _actorColliders.Clear();
            _isArmed = true;
            _isActivatedVisual = false;
        }

        private void PurgeDestroyedActorColliders()
        {
            if (_actorColliders.Count == 0)
            {
                return;
            }

            List<int> inactiveColliderIds = null;
            foreach (KeyValuePair<int, Collider> pair in _actorColliders)
            {
                if (pair.Value != null && pair.Value.enabled)
                {
                    continue;
                }

                inactiveColliderIds ??= new List<int>();
                inactiveColliderIds.Add(pair.Key);
            }

            if (inactiveColliderIds == null)
            {
                return;
            }

            foreach (int colliderId in inactiveColliderIds)
            {
                _actorColliders.Remove(colliderId);
            }

            if (_actorColliders.Count == 0)
            {
                _isArmed = true;
                _isActivatedVisual = false;
            }
        }
    }
}
