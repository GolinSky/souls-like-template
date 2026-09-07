using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorSystem : MonoBehaviour, IInitializable, ILateTickable, IDisposable
    {
        private const string UNLOCKED_ELEVATORS_KEY = "UnlockedElevators";
        private const int RIDER_COLLIDER_CAPACITY = 32;

        private readonly Dictionary<ElevatorView, List<RegisteredEndpoint>> _elevators = new();
        private readonly Collider[] _riderColliderBuffer = new Collider[RIDER_COLLIDER_CAPACITY];
        private readonly List<IPlatformRiderMotor> _sampledRiders = new(RIDER_COLLIDER_CAPACITY);
        private readonly HashSet<long> _sampledRiderIds = new();
        private HashSet<string> _unlockedElevatorIds;
        private IEntityLocator _entityLocator;
        private IUniqueIdGenerator _idGenerator;
        private IStorageRegistry _storageRegistry;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            IUniqueIdGenerator idGenerator,
            IStorageRegistry storageRegistry)
        {
            _entityLocator = entityLocator;
            _idGenerator = idGenerator;
            _storageRegistry = storageRegistry;
        }

        public void Initialize()
        {
            _unlockedElevatorIds = _storageRegistry.GetData(
                UNLOCKED_ELEVATORS_KEY,
                new HashSet<string>());
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                RegisterScene(SceneManager.GetSceneAt(index));
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            foreach (ElevatorView elevator in new List<ElevatorView>(_elevators.Keys))
            {
                Unregister(elevator);
            }
        }

        public void Register(ElevatorView elevator)
        {
            if (_elevators.ContainsKey(elevator))
            {
                return;
            }

            ValidateSaveIdentifier(elevator);
            elevator.AssignSystem(this);
            elevator.Initialize(
                elevator.StartsLocked && _unlockedElevatorIds.Contains(elevator.SaveIdentifier));
            elevator.StateChanged += OnElevatorStateChanged;

            List<RegisteredEndpoint> endpoints = new();
            foreach (ElevatorEndpoint endpoint in elevator.Endpoints)
            {
                endpoints.Add(RegisterEndpoint(elevator, endpoint));
            }

            _elevators.Add(elevator, endpoints);
            foreach (ElevatorShaftHazard shaftHazard in elevator.ShaftHazards)
            {
                shaftHazard.Construct(_entityLocator);
            }
            ApplyEndpointAvailability(elevator);
        }

        public void Unregister(ElevatorView elevator)
        {
            if (!_elevators.TryGetValue(elevator, out List<RegisteredEndpoint> endpoints))
            {
                return;
            }

            _elevators.Remove(elevator);
            elevator.StateChanged -= OnElevatorStateChanged;
            elevator.DisposeEntity();
            foreach (ElevatorShaftHazard shaftHazard in elevator.ShaftHazards)
            {
                shaftHazard.DisposeEntity();
            }
            foreach (RegisteredEndpoint endpoint in endpoints)
            {
                endpoint.Command.Dispose();
                endpoint.Endpoint.DisposeEntity();
                endpoint.Entity.Dispose();
            }
        }

        public void LateTick()
        {
            foreach (ElevatorView elevator in _elevators.Keys)
            {
                if (!elevator.IsMoving)
                {
                    continue;
                }

                SampleSupportedRiders(elevator);
                Vector3 nextDisplacement = elevator.GetNextMotionDisplacement(Time.deltaTime);
                bool moveRidersFirst = nextDisplacement.y > 0f;
                if (moveRidersFirst)
                {
                    ApplyRiderDisplacement(nextDisplacement);
                }

                bool arrived = elevator.AdvanceMotion(Time.deltaTime, out Vector3 displacement);
                if (!moveRidersFirst)
                {
                    ApplyRiderDisplacement(displacement);
                }

                if (arrived)
                {
                    foreach (IPlatformRiderMotor rider in _sampledRiders)
                    {
                        if (rider is UnityEngine.Object unityObject && unityObject == null)
                        {
                            continue;
                        }

                        rider.SynchronizeAfterPlatformRide();
                    }

                }
            }
        }

        public bool CanInteract(ElevatorEndpoint endpoint, IEntity actor) =>
            endpoint.Elevator.CanInteract(endpoint, endpoint.IsActorAllowed(actor));

        public ElevatorView GetElevator(ElevatorEndpoint endpoint)
        {
            foreach ((ElevatorView elevator, List<RegisteredEndpoint> endpoints) in _elevators)
            {
                foreach (RegisteredEndpoint registeredEndpoint in endpoints)
                {
                    if (registeredEndpoint.Endpoint == endpoint)
                    {
                        return elevator;
                    }
                }
            }

            throw new InvalidOperationException(
                $"Elevator endpoint '{endpoint.name}' is not registered.");
        }

        public string GetPrompt() => "Operate elevator";

        public string GetFailurePrompt(ElevatorEndpoint endpoint, IEntity actor) =>
            endpoint.Elevator.GetFailurePrompt(endpoint, endpoint.IsActorAllowed(actor));

        public async UniTask InteractAsync(
            ElevatorEndpoint endpoint,
            IEntity actor,
            CancellationToken token)
        {
            ElevatorView elevator = endpoint.Elevator;
            if (!elevator.CanInteract(endpoint, endpoint.IsActorAllowed(actor)))
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            if (elevator.StartsLocked && !elevator.IsUnlocked)
            {
                elevator.Unlock();
                _unlockedElevatorIds.Add(elevator.SaveIdentifier);
                _storageRegistry.SaveData(UNLOCKED_ELEVATORS_KEY, _unlockedElevatorIds);
            }

            endpoint.PlayActivation();
            elevator.TryStartMove(endpoint.GetRequestedFloor(elevator.CurrentFloor));
            await UniTask.CompletedTask;
        }

        private RegisteredEndpoint RegisterEndpoint(ElevatorView elevator, ElevatorEndpoint endpoint)
        {
            ViewEntity viewEntity = endpoint.GetComponent<ViewEntity>();
            if (viewEntity == null)
            {
                throw new InvalidOperationException(
                    $"Elevator endpoint '{endpoint.name}' requires {nameof(ViewEntity)} on its root.");
            }

            long id = _idGenerator.GenerateUniqueId();
            viewEntity.Construct(id, endpoint.EntityType);
            Entity entity = new(id, _entityLocator, endpoint.EntityType);
            ElevatorInteractCommand command = new(entity, endpoint);
            endpoint.Construct(this, _entityLocator, entity, viewEntity);
            command.Initialize();
            entity.Initialize();
            return new RegisteredEndpoint(endpoint, entity, command);
        }

        private void ValidateSaveIdentifier(ElevatorView elevator)
        {
            if (!elevator.StartsLocked)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(elevator.SaveIdentifier))
            {
                throw new InvalidOperationException(
                    $"Locked elevator '{elevator.name}' requires a stable {nameof(ElevatorView.SaveIdentifier)}.");
            }

            foreach (ElevatorView registered in _elevators.Keys)
            {
                if (registered.StartsLocked && registered.SaveIdentifier == elevator.SaveIdentifier)
                {
                    throw new InvalidOperationException(
                        $"Locked elevator id '{elevator.SaveIdentifier}' is duplicated by "
                        + $"'{registered.name}' and '{elevator.name}'.");
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RegisterScene(scene);

        private void OnSceneUnloaded(Scene scene)
        {
            foreach (ElevatorView elevator in new List<ElevatorView>(_elevators.Keys))
            {
                if (elevator.gameObject.scene == scene)
                {
                    Unregister(elevator);
                }
            }
        }

        private void RegisterScene(Scene scene)
        {
            if (!scene.isLoaded)
            {
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (ElevatorView elevator in root.GetComponentsInChildren<ElevatorView>(true))
                {
                    Register(elevator);
                }
            }
        }

        private void OnElevatorStateChanged()
        {
            foreach (ElevatorView elevator in _elevators.Keys)
            {
                ApplyEndpointAvailability(elevator);
            }
        }

        private static void ApplyEndpointAvailability(ElevatorView elevator)
        {
            foreach (ElevatorEndpoint endpoint in elevator.Endpoints)
            {
                bool available = endpoint.EndpointType == ElevatorEndpointType.PressurePlate
                    || elevator.IsUnlocked
                        && endpoint.GetRequestedFloor(elevator.CurrentFloor) != elevator.CurrentFloor;
                endpoint.ApplyAvailability(available && !elevator.IsMoving);
                if (!elevator.IsMoving)
                {
                    endpoint.ResetPressurePlate();
                }
            }
        }

        private void SampleSupportedRiders(ElevatorView elevator)
        {
            _sampledRiders.Clear();
            _sampledRiderIds.Clear();
            BoxCollider volume = elevator.RiderDetectionVolume;
            Transform volumeTransform = volume.transform;
            Vector3 lossyScale = volumeTransform.lossyScale;
            Vector3 halfExtents = Vector3.Scale(
                volume.size * 0.5f,
                new Vector3(
                    Mathf.Abs(lossyScale.x),
                    Mathf.Abs(lossyScale.y),
                    Mathf.Abs(lossyScale.z)));
            int colliderCount = Physics.OverlapBoxNonAlloc(
                volumeTransform.TransformPoint(volume.center),
                halfExtents,
                _riderColliderBuffer,
                volumeTransform.rotation,
                ~0,
                QueryTriggerInteraction.Collide);
            for (int index = 0; index < colliderCount; index++)
            {
                Collider collider = _riderColliderBuffer[index];
                if (!_entityLocator.TryGetEntity(collider, out IEntity entity)
                    || !_sampledRiderIds.Add(entity.Id))
                {
                    continue;
                }

                IViewEntity viewEntity = collider.GetComponentInParent<IViewEntity>();
                if (viewEntity is not Component viewComponent)
                {
                    throw new InvalidOperationException(
                        $"Platform rider entity {entity.Id} requires a {nameof(ViewEntity)} component.");
                }

                IPlatformRiderMotor rider = viewComponent.GetComponent<IPlatformRiderMotor>();
                if (rider != null && rider.IsSupportedBy(elevator.PlatformSupportCollider))
                {
                    _sampledRiders.Add(rider);
                }
            }
        }

        private void ApplyRiderDisplacement(Vector3 displacement)
        {
            foreach (IPlatformRiderMotor rider in _sampledRiders)
            {
                if (rider is UnityEngine.Object unityObject && unityObject == null)
                {
                    continue;
                }

                rider.ApplyPlatformDisplacement(displacement);
            }
        }

        private readonly struct RegisteredEndpoint
        {
            public ElevatorEndpoint Endpoint { get; }
            public Entity Entity { get; }
            public ElevatorInteractCommand Command { get; }

            public RegisteredEndpoint(
                ElevatorEndpoint endpoint,
                Entity entity,
                ElevatorInteractCommand command)
            {
                Endpoint = endpoint;
                Entity = entity;
                Command = command;
            }
        }
    }
}
