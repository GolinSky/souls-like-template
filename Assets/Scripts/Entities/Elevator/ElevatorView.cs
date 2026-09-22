using System;
using Unity.AI.Navigation;
using SoulsLike.Entities.BaseEntity;
using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorView : MonoBehaviour
    {
        [SerializeField] private string saveIdentifier;
        [SerializeField] private bool startsLocked;
        [SerializeField] private ElevatorFloor startingFloor;
        [SerializeField] private ViewEntity viewEntity;
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
        public string SaveIdentifier => saveIdentifier;
        public bool StartsLocked => startsLocked;
        public ElevatorFloor StartingFloor => startingFloor;
        public ViewEntity ViewEntity => viewEntity;
        public ElevatorEndpoint[] Endpoints => endpoints;
        public ElevatorShaftHazard[] ShaftHazards => shaftHazards;
        public BoxCollider RiderDetectionVolume => riderDetectionVolume;
        public Collider PlatformSupportCollider => platformSupportCollider;
        public Vector3 PlatformPosition => platform.position;
        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;

        private IElevatorPresenter _presenter;
        private bool _isBound;

        public void AssignPresenter(IElevatorPresenter presenter)
        {
            _presenter = presenter;
            _isBound = true;
        }

        public void ClearPresenter(IElevatorPresenter presenter)
        {
            if (ReferenceEquals(_presenter, presenter))
            {
                _presenter = null;
                _isBound = false;
            }
        }

        public Vector3 GetDockPosition(ElevatorFloor floor) =>
            floor == ElevatorFloor.Bottom ? bottomDock.position : topDock.position;

        public void SetPlatformPosition(Vector3 position)
        {
            platform.position = position;
            Physics.SyncTransforms();
        }

        public void ApplyNavigationLinks(bool isMoving, ElevatorFloor currentFloor)
        {
            bool bottomDocked = !isMoving && currentFloor == ElevatorFloor.Bottom;
            foreach (NavMeshLink link in bottomNavMeshLinks)
            {
                link.enabled = bottomDocked;
            }

            bool topDocked = !isMoving && currentFloor == ElevatorFloor.Top;
            foreach (NavMeshLink link in topNavMeshLinks)
            {
                link.enabled = topDocked;
            }
        }

        public void StartPresentation()
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

        }

        public void StopPresentation()
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

        }

        private void OnEnable()
        {
            if (_isBound)
            {
                _presenter.Register(this);
            }
        }

        private void Start() => _presenter.Register(this);

        private void OnDisable()
        {
            if (_isBound)
            {
                _presenter.Unregister(this);
            }
        }

        private void OnDestroy() => OnDisable();
    }
}
