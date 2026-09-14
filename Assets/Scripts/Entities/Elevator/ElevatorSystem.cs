using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Storage;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorSystem : MonoBehaviour, IElevatorPresenter, ILateTickable, IDisposable
    {
        private const string UNLOCKED_ELEVATORS_KEY = "UnlockedElevators";
        private const int RIDER_COLLIDER_CAPACITY = 32;
        private static readonly Vector3 ImpulseVelocity = new(0f, -0.05f, 0f);

        private readonly Dictionary<ElevatorView, ElevatorModel> _elevators = new();
        private readonly Collider[] _riderColliderBuffer = new Collider[RIDER_COLLIDER_CAPACITY];
        private readonly List<PlatformRideCommand> _sampledRiders = new(RIDER_COLLIDER_CAPACITY);
        private readonly HashSet<long> _sampledRiderIds = new();
        private HashSet<string> _unlockedElevatorIds;
        private IEntityLocator _entityLocator;
        private IUniqueIdGenerator _idGenerator;
        private IStorageRegistry _storageRegistry;
        private ICameraService _cameraService;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            IUniqueIdGenerator idGenerator,
            IStorageRegistry storageRegistry,
            ICameraService cameraService)
        {
            _entityLocator = entityLocator;
            _idGenerator = idGenerator;
            _storageRegistry = storageRegistry;
            _cameraService = cameraService;
            _unlockedElevatorIds = storageRegistry.GetData(
                UNLOCKED_ELEVATORS_KEY,
                new HashSet<string>());
        }

        public void Dispose()
        {
            foreach (ElevatorView elevator in new List<ElevatorView>(_elevators.Keys))
            {
                Unregister(elevator);
                elevator.ClearPresenter(this);
            }
        }

        public void Register(ElevatorView elevator)
        {
            elevator.AssignPresenter(this);
            if (!elevator.isActiveAndEnabled || _elevators.ContainsKey(elevator))
            {
                return;
            }

            ValidateSaveIdentifier(elevator);
            ElevatorModel model = new()
            {
                IsUnlocked = !elevator.StartsLocked
                    || _unlockedElevatorIds.Contains(elevator.SaveIdentifier),
                CurrentFloor = elevator.StartingFloor,
                DestinationFloor = elevator.StartingFloor
            };
            elevator.SetPlatformPosition(elevator.GetDockPosition(model.CurrentFloor));
            elevator.ApplyNavigationLinks(false, model.CurrentFloor);
            model.RootEntity = RegisterRootEntity(elevator);
            foreach (ElevatorEndpoint endpoint in elevator.Endpoints)
            {
                model.Endpoints.Add(RegisterEndpoint(elevator, endpoint));
            }
            _elevators.Add(elevator, model);

            foreach (ElevatorShaftHazard shaftHazard in elevator.ShaftHazards)
            {
                shaftHazard.Construct(_entityLocator);
            }

            ApplyEndpointAvailability(model);
        }

        public void Unregister(ElevatorView elevator)
        {
            if (!_elevators.TryGetValue(elevator, out ElevatorModel model))
            {
                return;
            }
            _elevators.Remove(elevator);

            if (model.IsMoving)
            {
                model.CurrentFloor = Vector3.SqrMagnitude(model.MotionStartPosition - elevator.PlatformPosition)
                    <= Vector3.SqrMagnitude(model.MotionDestinationPosition - elevator.PlatformPosition)
                    ? model.CurrentFloor
                    : model.DestinationFloor;
                elevator.SetPlatformPosition(elevator.GetDockPosition(model.CurrentFloor));
                model.IsMoving = false;
            }
            elevator.ApplyNavigationLinks(false, model.CurrentFloor);
            elevator.StopPresentation();

            foreach (ElevatorShaftHazard shaftHazard in elevator.ShaftHazards)
            {
                shaftHazard.DisposeEntity();
            }

            foreach (ElevatorModel.EndpointRegistration endpoint in model.Endpoints)
            {
                endpoint.Command.Dispose();
                endpoint.Endpoint.DisposeEntity();
                endpoint.Entity.Dispose();
            }

            model.RootEntity.Dispose();
        }

        public void LateTick()
        {
            foreach ((ElevatorView elevator, ElevatorModel model) in _elevators)
            {
                if (!model.IsMoving)
                {
                    continue;
                }

                SampleSupportedRiders(elevator);
                Vector3 nextDisplacement = GetMotionPosition(model, model.MotionElapsed + Time.deltaTime)
                    - elevator.PlatformPosition;
                bool moveRidersFirst = nextDisplacement.y > 0f;
                if (moveRidersFirst)
                {
                    ApplyRiderDisplacement(nextDisplacement);
                }

                model.MotionElapsed += Time.deltaTime;
                Vector3 nextPosition = GetMotionPosition(model, model.MotionElapsed);
                Vector3 displacement = nextPosition - elevator.PlatformPosition;
                elevator.SetPlatformPosition(nextPosition);
                if (!moveRidersFirst)
                {
                    ApplyRiderDisplacement(displacement);
                }

                if (model.MotionElapsed < model.Motion.Duration)
                {
                    continue;
                }

                elevator.SetPlatformPosition(model.MotionDestinationPosition);
                model.CurrentFloor = model.DestinationFloor;
                model.IsMoving = false;
                elevator.ApplyNavigationLinks(false, model.CurrentFloor);
                elevator.StopPresentation();
                ApplyEndpointAvailability(model);
                SynchronizeRiders();
                _cameraService.GenerateImpulse(elevator.PlatformPosition, ImpulseVelocity);
            }
        }

        public bool CanInteract(ElevatorEndpoint endpoint, IEntity actor)
        {
            ElevatorModel model = GetModel(endpoint);
            return endpoint.IsActorAllowed(actor) && CanMove(model, endpoint);
        }

        public string GetPrompt() => "Operate elevator";

        public string GetFailurePrompt(ElevatorEndpoint endpoint, IEntity actor)
        {
            ElevatorModel model = GetModel(endpoint);
            if (!endpoint.IsActorAllowed(actor))
            {
                return "Cannot operate elevator";
            }

            if (model.IsMoving)
            {
                return "Elevator is moving";
            }

            if (endpoint.EndpointType == ElevatorEndpointType.CallLever && !model.IsUnlocked)
            {
                return "Elevator is locked";
            }

            return "Elevator is already here";
        }

        public async UniTask InteractAsync(
            ElevatorEndpoint endpoint,
            IEntity actor,
            CancellationToken token)
        {
            ElevatorModel model = GetModel(endpoint);
            if (!endpoint.IsActorAllowed(actor) || !CanMove(model, endpoint))
            {
                return;
            }

            token.ThrowIfCancellationRequested();
            if (endpoint.Elevator.StartsLocked && !model.IsUnlocked)
            {
                model.IsUnlocked = true;
                _unlockedElevatorIds.Add(endpoint.Elevator.SaveIdentifier);
                _storageRegistry.SaveData(UNLOCKED_ELEVATORS_KEY, _unlockedElevatorIds);
            }

            endpoint.PlayActivation();
            StartMotion(endpoint.Elevator, model, endpoint.GetRequestedFloor(model.CurrentFloor));
            await UniTask.CompletedTask;
        }

        private Entity RegisterRootEntity(ElevatorView elevator)
        {
            if (elevator.ViewEntity == null)
            {
                throw new InvalidOperationException(
                    $"Elevator '{elevator.name}' requires an assigned {nameof(ViewEntity)}.");
            }

            long id = _idGenerator.GenerateUniqueId();
            elevator.ViewEntity.Construct(id, EntityType.Elevator);
            Entity entity = new(id, _entityLocator, EntityType.Elevator);
            entity.Initialize();
            return entity;
        }

        private ElevatorModel.EndpointRegistration RegisterEndpoint(
            ElevatorView elevator,
            ElevatorEndpoint endpoint)
        {
            if (endpoint.ViewEntity == null)
            {
                throw new InvalidOperationException(
                    $"Elevator endpoint '{endpoint.name}' requires an assigned {nameof(ViewEntity)}.");
            }

            long id = _idGenerator.GenerateUniqueId();
            endpoint.ViewEntity.Construct(id, endpoint.EntityType);
            Entity entity = new(id, _entityLocator, endpoint.EntityType);
            ElevatorInteractCommand command = new(entity, endpoint);
            endpoint.Construct(this, elevator, _entityLocator, entity, endpoint.ViewEntity);
            command.Initialize();
            entity.Initialize();
            return new ElevatorModel.EndpointRegistration(endpoint, entity, command);
        }

        private void StartMotion(
            ElevatorView elevator,
            ElevatorModel model,
            ElevatorFloor destination)
        {
            model.DestinationFloor = destination;
            model.MotionStartPosition = elevator.PlatformPosition;
            model.MotionDestinationPosition = elevator.GetDockPosition(destination);
            model.Motion = new ElevatorMotion(
                Vector3.Distance(model.MotionStartPosition, model.MotionDestinationPosition),
                elevator.MaxSpeed,
                elevator.Acceleration);
            model.MotionElapsed = 0f;
            model.IsMoving = true;
            elevator.ApplyNavigationLinks(true, model.CurrentFloor);
            elevator.StartPresentation();
            ApplyEndpointAvailability(model);
            _cameraService.GenerateImpulse(elevator.PlatformPosition, ImpulseVelocity);
        }

        private static bool CanMove(ElevatorModel model, ElevatorEndpoint endpoint) =>
            !model.IsMoving
            && (endpoint.EndpointType != ElevatorEndpointType.CallLever || model.IsUnlocked)
            && endpoint.GetRequestedFloor(model.CurrentFloor) != model.CurrentFloor;

        private ElevatorModel GetModel(ElevatorEndpoint endpoint)
        {
            if (_elevators.TryGetValue(endpoint.Elevator, out ElevatorModel model))
            {
                return model;
            }

            throw new InvalidOperationException(
                $"Elevator endpoint '{endpoint.name}' is not registered.");
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
                if (registered != elevator
                    && registered.StartsLocked
                    && registered.SaveIdentifier == elevator.SaveIdentifier)
                {
                    throw new InvalidOperationException(
                        $"Locked elevator id '{elevator.SaveIdentifier}' is duplicated by "
                        + $"'{registered.name}' and '{elevator.name}'.");
                }
            }
        }

        private static void ApplyEndpointAvailability(ElevatorModel model)
        {
            foreach (ElevatorModel.EndpointRegistration registeredEndpoint in model.Endpoints)
            {
                ElevatorEndpoint endpoint = registeredEndpoint.Endpoint;
                bool available = endpoint.EndpointType == ElevatorEndpointType.PressurePlate
                    || model.IsUnlocked
                        && endpoint.GetRequestedFloor(model.CurrentFloor) != model.CurrentFloor;
                endpoint.ApplyAvailability(available && !model.IsMoving);
                if (!model.IsMoving)
                {
                    endpoint.ResetPressurePlate();
                }
            }
        }

        private static Vector3 GetMotionPosition(ElevatorModel model, float elapsed) =>
            model.Motion.Distance <= Mathf.Epsilon
                ? model.MotionDestinationPosition
                : Vector3.LerpUnclamped(
                    model.MotionStartPosition,
                    model.MotionDestinationPosition,
                    model.Motion.SampleDistance(elapsed) / model.Motion.Distance);

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
                    || !_sampledRiderIds.Add(entity.Id)
                    || !entity.TryGetComponent(out PlatformRideCommand command)
                    || !command.IsSupportedBy(elevator.PlatformSupportCollider))
                {
                    continue;
                }

                _sampledRiders.Add(command);
            }
        }

        private void ApplyRiderDisplacement(Vector3 displacement)
        {
            foreach (PlatformRideCommand rider in _sampledRiders)
            {
                rider.ApplyPlatformDisplacement(displacement);
            }
        }

        private void SynchronizeRiders()
        {
            foreach (PlatformRideCommand rider in _sampledRiders)
            {
                rider.SynchronizeAfterPlatformRide();
            }
        }

    }
}
