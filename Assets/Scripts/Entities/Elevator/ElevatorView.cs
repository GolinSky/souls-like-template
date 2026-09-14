using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorView : MonoBehaviour
    {
        private const string MOVE_PROMPT = "Operate elevator";
        private const string LOCKED_PROMPT = "Elevator is locked";
        private const string MOVING_PROMPT = "Elevator is moving";
        private const string ARRIVED_PROMPT = "Elevator is already here";

        [SerializeField] private string saveIdentifier;
        [SerializeField] private bool startsLocked;
        [SerializeField] private ElevatorFloor startingFloor;
        [SerializeField] private Transform platform;
        [SerializeField] private BoxCollider riderDetectionVolume;
        [SerializeField] private Collider platformSupportCollider;
        [SerializeField] private Transform bottomDock;
        [SerializeField] private Transform topDock;
        [SerializeField, Min(0.1f)] private float maxSpeed = 4f;
        [SerializeField, Min(0.1f)] private float acceleration = 6f;
        [SerializeField] private ElevatorEndpoint[] endpoints = Array.Empty<ElevatorEndpoint>();
        [SerializeField] private ElevatorShaftHazard[] shaftHazards = Array.Empty<ElevatorShaftHazard>();
        [SerializeField] private NavMeshLink[] bottomNavMeshLinks = Array.Empty<NavMeshLink>();
        [SerializeField] private NavMeshLink[] topNavMeshLinks = Array.Empty<NavMeshLink>();
        [SerializeField] private AudioSource movementAudioSource;
        [SerializeField] private AudioClip startClip;
        [SerializeField] private AudioClip loopClip;
        [SerializeField] private AudioClip stopClip;
        [SerializeField] private ParticleSystem[] startEffects = Array.Empty<ParticleSystem>();
        [SerializeField] private ParticleSystem[] stopEffects = Array.Empty<ParticleSystem>();
        [SerializeField] private UnityEvent movementStarted;
        [SerializeField] private UnityEvent movementStopped;

        private bool _isUnlocked;
        private bool _isMoving;
        private ElevatorFloor _currentFloor;
        private ElevatorFloor _destinationFloor;
        private ElevatorMotion _motion;
        private Vector3 _motionStartPosition;
        private Vector3 _motionDestinationPosition;
        private float _motionElapsed;
        private ElevatorSystem _system;

        public string SaveIdentifier => saveIdentifier;
        public bool StartsLocked => startsLocked;
        public bool IsUnlocked => !startsLocked || _isUnlocked;
        public bool IsMoving => _isMoving;
        public ElevatorFloor CurrentFloor => _currentFloor;
        public IEnumerable<ElevatorEndpoint> Endpoints => endpoints;
        public IEnumerable<ElevatorShaftHazard> ShaftHazards => shaftHazards;
        public BoxCollider RiderDetectionVolume => riderDetectionVolume;
        public Collider PlatformSupportCollider => platformSupportCollider;

        public event Action StateChanged;

        public void AssignSystem(ElevatorSystem system) => _system = system;

        public void Initialize(bool unlocked)
        {
            _isUnlocked = unlocked;
            _currentFloor = startingFloor;
            SetPlatformPosition(GetDockPosition(_currentFloor));
            ApplyNavigationLinks();
            NotifyStateChanged();
        }

        public void Unlock()
        {
            if (!startsLocked || _isUnlocked)
            {
                return;
            }

            _isUnlocked = true;
            NotifyStateChanged();
        }

        public bool CanInteract(ElevatorEndpoint endpoint, bool actorAllowed)
        {
            if (!actorAllowed || _isMoving)
            {
                return false;
            }

            if (endpoint.EndpointType == ElevatorEndpointType.CallLever && !IsUnlocked)
            {
                return false;
            }

            return endpoint.GetRequestedFloor(_currentFloor) != _currentFloor;
        }

        public string GetFailurePrompt(ElevatorEndpoint endpoint, bool actorAllowed)
        {
            if (!actorAllowed)
            {
                return "Cannot operate elevator";
            }

            if (_isMoving)
            {
                return MOVING_PROMPT;
            }

            if (endpoint.EndpointType == ElevatorEndpointType.CallLever && !IsUnlocked)
            {
                return LOCKED_PROMPT;
            }

            return ARRIVED_PROMPT;
        }

        public string GetPrompt() => MOVE_PROMPT;

        public bool TryStartMove(ElevatorFloor destination)
        {
            if (_isMoving || destination == _currentFloor)
            {
                return false;
            }

            _destinationFloor = destination;
            _motionStartPosition = platform.position;
            _motionDestinationPosition = GetDockPosition(destination);
            _motion = new ElevatorMotion(
                Vector3.Distance(_motionStartPosition, _motionDestinationPosition),
                maxSpeed,
                acceleration);
            _motionElapsed = 0f;
            _isMoving = true;
            ApplyNavigationLinks();
            NotifyStateChanged();
            StartPresentation();
            return true;
        }

        public bool AdvanceMotion(float deltaTime, out Vector3 displacement)
        {
            displacement = Vector3.zero;
            if (!_isMoving)
            {
                return false;
            }

            _motionElapsed += deltaTime;
            Vector3 nextPosition = GetMotionPosition(_motionElapsed);
            displacement = nextPosition - platform.position;
            SetPlatformPosition(nextPosition);

            if (_motionElapsed < _motion.Duration)
            {
                return false;
            }

            SetPlatformPosition(_motionDestinationPosition);
            _currentFloor = _destinationFloor;
            _isMoving = false;
            ApplyNavigationLinks();
            StopPresentation();
            NotifyStateChanged();
            return true;
        }

        public Vector3 GetNextMotionDisplacement(float deltaTime) => _isMoving
            ? GetMotionPosition(_motionElapsed + deltaTime) - platform.position
            : Vector3.zero;

        public void DisposeEntity()
        {
            if (_isMoving)
            {
                ElevatorFloor nearestFloor = Vector3.SqrMagnitude(
                        _motionStartPosition - platform.position)
                    <= Vector3.SqrMagnitude(_motionDestinationPosition - platform.position)
                    ? _currentFloor
                    : _destinationFloor;
                _currentFloor = nearestFloor;
                SetPlatformPosition(GetDockPosition(nearestFloor));
            }

            _isMoving = false;
            ApplyNavigationLinks();
            StopPresentation();
        }

        private void OnEnable() => _system?.Register(this);

        private void OnDisable() => _system?.Unregister(this);

        private void OnDestroy() => _system?.Unregister(this);
        

        private Vector3 GetDockPosition(ElevatorFloor floor) =>
            floor == ElevatorFloor.Bottom ? bottomDock.position : topDock.position;

        private Vector3 GetMotionPosition(float elapsed) => _motion.Distance <= Mathf.Epsilon
            ? _motionDestinationPosition
            : Vector3.LerpUnclamped(
                _motionStartPosition,
                _motionDestinationPosition,
                _motion.SampleDistance(elapsed) / _motion.Distance);

        private void SetPlatformPosition(Vector3 position)
        {
            platform.position = position;
            Physics.SyncTransforms();
        }

        private void ApplyNavigationLinks()
        {
            bool bottomDocked = !_isMoving && _currentFloor == ElevatorFloor.Bottom;
            foreach (NavMeshLink link in bottomNavMeshLinks)
            {
                link.enabled = bottomDocked;
            }

            bool topDocked = !_isMoving && _currentFloor == ElevatorFloor.Top;
            foreach (NavMeshLink link in topNavMeshLinks)
            {
                link.enabled = topDocked;
            }
        }

        private void StartPresentation()
        {
            if (movementAudioSource != null)
            {
                if (startClip != null)
                {
                    movementAudioSource.PlayOneShot(startClip);
                }

                if (loopClip != null)
                {
                    movementAudioSource.clip = loopClip;
                    movementAudioSource.loop = true;
                    movementAudioSource.Play();
                }
            }

            foreach (ParticleSystem effect in startEffects)
            {
                effect.Play();
            }

            movementStarted?.Invoke();
        }

        private void StopPresentation()
        {
            if (movementAudioSource != null)
            {
                movementAudioSource.Stop();
                if (stopClip != null)
                {
                    movementAudioSource.PlayOneShot(stopClip);
                }
            }

            foreach (ParticleSystem effect in stopEffects)
            {
                effect.Play();
            }

            movementStopped?.Invoke();
        }

        private void NotifyStateChanged() => StateChanged?.Invoke();
    }
}
