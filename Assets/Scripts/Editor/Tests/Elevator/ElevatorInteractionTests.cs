#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Elevator;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Services.Settings;
using SoulsLike.Services.Storage;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Elevator
{
    public sealed class ElevatorInteractionTests
    {
        private readonly List<UnityEngine.Object> _objects = new();
        private readonly List<ElevatorSystem> _systems = new();

        [TearDown]
        public void TearDown()
        {
            foreach (ElevatorSystem system in _systems)
            {
                system.Dispose();
            }

            foreach (UnityEngine.Object target in _objects)
            {
                if (target != null)
                {
                    UnityEngine.Object.DestroyImmediate(target);
                }
            }

            _systems.Clear();
            _objects.Clear();
        }

        [Test]
        public void LockedLever_IsDenied_PressurePlateUnlocksAndPersists_AndCallerCancellationDoesNotStopTravel()
        {
            EntityLocator locator = new();
            TestStorage storage = new();
            TestCameraService camera = new();
            ElevatorSystem system = CreateSystem(locator, storage, camera);
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out ElevatorEndpoint topLever);
            Entity actor = new(99, locator, EntityType.Player);
            actor.Initialize();

            system.Register(elevator);
            Assert.That(system.CanInteract(topLever, actor), Is.False);

            using CancellationTokenSource cancellation = new();
            system.InteractAsync(plate, actor, cancellation.Token).GetAwaiter().GetResult();
            cancellation.Cancel();

            Assert.That(storage.SaveCalls, Is.EqualTo(1));
            Assert.That(camera.ImpulseCalls, Is.EqualTo(1));
            Assert.That(system.CanInteract(topLever, actor), Is.False);

            system.Unregister(elevator);
            Assert.That(camera.ImpulseCalls, Is.EqualTo(1));
            system.Register(elevator);
            Assert.That(system.CanInteract(topLever, actor), Is.True);
            actor.Dispose();
        }

        [Test]
        public void UnregisteringElevator_RemovesRootAndEndpointEntities()
        {
            EntityLocator locator = new();
            ElevatorSystem system = CreateSystem(locator, new TestStorage(), new TestCameraService());
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out _);

            system.Register(elevator);
            long rootId = elevator.ViewEntity.Id;
            long endpointId = plate.ViewEntity.Id;
            Assert.That(locator.TryGetEntity(rootId, out _), Is.True);
            Assert.That(locator.TryGetEntity(endpointId, out _), Is.True);

            system.Unregister(elevator);

            Assert.That(locator.TryGetEntity(rootId, out _), Is.False);
            Assert.That(locator.TryGetEntity(endpointId, out _), Is.False);
        }

        [Test]
        public void PersistedUnlock_IsAppliedIndependentlyToEachElevator()
        {
            EntityLocator locator = new();
            TestStorage storage = new();
            storage.SaveData("UnlockedElevators", new HashSet<string> { "unlocked" });
            ElevatorSystem system = CreateSystem(locator, storage, new TestCameraService());
            ElevatorView unlocked = CreateElevator(out _, out ElevatorEndpoint unlockedLever, "unlocked");
            ElevatorView locked = CreateElevator(out _, out ElevatorEndpoint lockedLever, "locked");
            Entity actor = new(99, locator, EntityType.Player);
            actor.Initialize();

            system.Register(unlocked);
            system.Register(locked);

            Assert.That(system.CanInteract(unlockedLever, actor), Is.True);
            Assert.That(system.CanInteract(lockedLever, actor), Is.False);
            actor.Dispose();
        }

        [Test]
        public void CameraImpulse_FiresOnlyForAcceptedStartAndArrival()
        {
            EntityLocator locator = new();
            TestCameraService camera = new();
            ElevatorSystem system = CreateSystem(locator, new TestStorage(), camera);
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out ElevatorEndpoint topLever, "test-elevator", 0f);
            Entity actor = new(99, locator, EntityType.Player);
            actor.Initialize();

            system.Register(elevator);
            system.InteractAsync(topLever, actor, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(camera.ImpulseCalls, Is.EqualTo(0));

            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() =>
                system.InteractAsync(plate, actor, cancellation.Token).GetAwaiter().GetResult());
            Assert.That(camera.ImpulseCalls, Is.EqualTo(0));

            system.InteractAsync(plate, actor, CancellationToken.None).GetAwaiter().GetResult();
            system.LateTick();

            Assert.That(camera.ImpulseCalls, Is.EqualTo(2));
            actor.Dispose();
        }

        [Test]
        public void BoundView_DisableAndReenable_RemovesAndReregistersFreshEntities()
        {
            EntityLocator locator = new();
            ElevatorSystem system = CreateSystem(locator, new TestStorage(), new TestCameraService());
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out _);

            system.Register(elevator);
            long initialRootId = elevator.ViewEntity.Id;
            long initialEndpointId = plate.ViewEntity.Id;

            InvokeLifecycle(elevator, "OnDisable");
            Assert.That(locator.TryGetEntity(initialRootId, out _), Is.False);
            Assert.That(locator.TryGetEntity(initialEndpointId, out _), Is.False);

            InvokeLifecycle(elevator, "OnEnable");
            Assert.That(locator.TryGetEntity(elevator.ViewEntity.Id, out _), Is.True);
            Assert.That(locator.TryGetEntity(plate.ViewEntity.Id, out _), Is.True);
            Assert.That(elevator.ViewEntity.Id, Is.Not.EqualTo(initialRootId));
            Assert.That(plate.ViewEntity.Id, Is.Not.EqualTo(initialEndpointId));
        }

        [Test]
        public void UnboundView_StartFails()
        {
            ElevatorView elevator = Track(new GameObject("UnboundElevator")).AddComponent<ElevatorView>();

            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() =>
                InvokeLifecycle(elevator, "Start"));

            Assert.That(exception.InnerException, Is.TypeOf<NullReferenceException>());
        }

        private ElevatorSystem CreateSystem(
            EntityLocator locator,
            TestStorage storage,
            TestCameraService camera)
        {
            GameObject root = Track(new GameObject("ElevatorSystemTest"));
            ElevatorSystem system = root.AddComponent<ElevatorSystem>();
            system.Construct(locator, new TestIdGenerator(), storage, camera);
            _systems.Add(system);
            return system;
        }

        private ElevatorView CreateElevator(
            out ElevatorEndpoint plate,
            out ElevatorEndpoint topLever,
            string identifier = "test-elevator",
            float topHeight = 8f)
        {
            GameObject root = Track(new GameObject("Elevator"));
            ViewEntity rootViewEntity = root.AddComponent<ViewEntity>();
            ElevatorView elevator = root.AddComponent<ElevatorView>();
            Transform platform = CreateChild(root.transform, "Platform");
            BoxCollider support = platform.gameObject.AddComponent<BoxCollider>();
            support.size = new Vector3(4f, 0.25f, 4f);
            BoxCollider volume = platform.gameObject.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(4f, 2f, 4f);
            Transform bottom = CreateChild(root.transform, "BottomDock");
            Transform top = CreateChild(root.transform, "TopDock");
            top.position = Vector3.up * topHeight;
            plate = CreateEndpoint(platform, ElevatorEndpointType.PressurePlate, ElevatorFloor.Bottom);
            topLever = CreateEndpoint(root.transform, ElevatorEndpointType.CallLever, ElevatorFloor.Top);

            SetField(elevator, "saveIdentifier", identifier);
            SetField(elevator, "startsLocked", true);
            SetField(elevator, "viewEntity", rootViewEntity);
            SetField(elevator, "platform", platform);
            SetField(elevator, "riderDetectionVolume", volume);
            SetField(elevator, "platformSupportCollider", support);
            SetField(elevator, "bottomDock", bottom);
            SetField(elevator, "topDock", top);
            SetField(elevator, "endpoints", new[] { plate, topLever });
            return elevator;
        }

        private static ElevatorEndpoint CreateEndpoint(
            Transform parent,
            ElevatorEndpointType type,
            ElevatorFloor floor)
        {
            GameObject root = new(type.ToString());
            root.transform.SetParent(parent);
            ViewEntity viewEntity = root.AddComponent<ViewEntity>();
            ElevatorEndpoint endpoint = root.AddComponent<ElevatorEndpoint>();
            SetField(endpoint, "endpointType", type);
            SetField(endpoint, "floor", floor);
            SetField(endpoint, "viewEntity", viewEntity);
            return endpoint;
        }

        private Transform CreateChild(Transform parent, string name)
        {
            GameObject child = Track(new GameObject(name));
            child.transform.SetParent(parent);
            return child.transform;
        }

        private GameObject Track(GameObject target)
        {
            _objects.Add(target);
            return target;
        }

        private static void InvokeLifecycle(ElevatorView elevator, string method) =>
            typeof(ElevatorView).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(elevator, null);

        private static void SetField<T>(object target, string name, T value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);

        private sealed class TestIdGenerator : IUniqueIdGenerator
        {
            private long _next;
            public long GenerateUniqueId() => ++_next;
        }

        private sealed class TestStorage : IStorageRegistry
        {
            private readonly Dictionary<string, object> _data = new();

            public int SaveCalls { get; private set; }

            public void SaveData<T>(string key, T data)
            {
                _data[key] = data;
                SaveCalls++;
            }

            public void SaveData<T>(Enum key, T data) => SaveData(key.ToString(), data);

            public T GetData<T>(string key, T defaultValue = default) =>
                _data.TryGetValue(key, out object value) ? (T)value : defaultValue;

            public T GetData<T>(Enum key, T defaultValue = default) => GetData(key.ToString(), defaultValue);
            public bool HasData(string key) => _data.ContainsKey(key);
            public bool HasData(Enum key) => HasData(key.ToString());
            public void DeleteData(string key) => _data.Remove(key);
            public void DeleteData(Enum key) => DeleteData(key.ToString());
        }

        private sealed class TestCameraService : ICameraService
        {
            public int ImpulseCalls { get; private set; }

            public void SetTarget(Transform target) { }
            public void UpdateFollowTarget(bool grounded, float verticalVelocity) { }
            public void UpdateRotation(Vector2 look) { }
            public float GetYaw() => 0f;
            public void SwitchAngle() { }
            public void SetLockOnTarget(long? targetEntityId) { }
            public void ClearLockOnTarget() { }
            public void RecenterCamera() { }
            public Camera GetMainCamera() => null;
            public void GenerateImpulse(Vector3 position, Vector3 velocity) => ImpulseCalls++;
            public void ApplySettings(CameraSettingsData settings) { }
        }
    }
}
#endif
