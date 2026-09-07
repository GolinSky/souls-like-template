#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Elevator;
using SoulsLike.Services.IdGeneration;
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
            ElevatorSystem system = CreateSystem(locator, storage);
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out ElevatorEndpoint topLever);
            Entity actor = new(99, locator, EntityType.Player);
            actor.Initialize();

            system.Register(elevator);
            Assert.That(system.CanInteract(topLever, actor), Is.False);

            using CancellationTokenSource cancellation = new();
            system.InteractAsync(plate, actor, cancellation.Token).GetAwaiter().GetResult();
            cancellation.Cancel();

            Assert.That(elevator.IsUnlocked, Is.True);
            Assert.That(elevator.IsMoving, Is.True);
            Assert.That(storage.SaveCalls, Is.EqualTo(1));
            Assert.That(system.CanInteract(topLever, actor), Is.False);

            system.Unregister(elevator);
            actor.Dispose();
        }

        [Test]
        public void UnregisteringElevator_RemovesEndpointEntities()
        {
            EntityLocator locator = new();
            ElevatorSystem system = CreateSystem(locator, new TestStorage());
            ElevatorView elevator = CreateElevator(out ElevatorEndpoint plate, out _);

            system.Register(elevator);
            long endpointId = plate.GetComponent<ViewEntity>().Id;
            Assert.That(locator.TryGetEntity(endpointId, out _), Is.True);

            system.Unregister(elevator);

            Assert.That(locator.TryGetEntity(endpointId, out _), Is.False);
        }

        private ElevatorSystem CreateSystem(EntityLocator locator, TestStorage storage)
        {
            GameObject root = Track(new GameObject("ElevatorSystemTest"));
            ElevatorSystem system = root.AddComponent<ElevatorSystem>();
            system.Construct(locator, new TestIdGenerator(), storage);
            system.Initialize();
            _systems.Add(system);
            return system;
        }

        private ElevatorView CreateElevator(out ElevatorEndpoint plate, out ElevatorEndpoint topLever)
        {
            GameObject root = Track(new GameObject("Elevator"));
            ElevatorView elevator = root.AddComponent<ElevatorView>();
            Transform platform = CreateChild(root.transform, "Platform");
            BoxCollider support = platform.gameObject.AddComponent<BoxCollider>();
            support.size = new Vector3(4f, 0.25f, 4f);
            BoxCollider volume = platform.gameObject.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(4f, 2f, 4f);
            Transform bottom = CreateChild(root.transform, "BottomDock");
            Transform top = CreateChild(root.transform, "TopDock");
            top.position = Vector3.up * 8f;
            plate = CreateEndpoint(platform, ElevatorEndpointType.PressurePlate, ElevatorFloor.Bottom);
            topLever = CreateEndpoint(root.transform, ElevatorEndpointType.CallLever, ElevatorFloor.Top);

            SetField(elevator, "saveIdentifier", "test-elevator");
            SetField(elevator, "startsLocked", true);
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
            root.AddComponent<ViewEntity>();
            ElevatorEndpoint endpoint = root.AddComponent<ElevatorEndpoint>();
            SetField(endpoint, "endpointType", type);
            SetField(endpoint, "floor", floor);
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

        private static void SetField<T>(object target, string name, T value)
        {
            typeof(ElevatorView).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
            typeof(ElevatorEndpoint).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        private sealed class TestIdGenerator : IUniqueIdGenerator
        {
            private long _next;
            public long GenerateUniqueId() => ++_next;
        }

        private sealed class TestStorage : IStorageRegistry
        {
            public int SaveCalls { get; private set; }
            public void SaveData<T>(string key, T data) => SaveCalls++;
            public void SaveData<T>(Enum key, T data) => SaveCalls++;
            public T GetData<T>(string key, T defaultValue = default) => defaultValue;
            public T GetData<T>(Enum key, T defaultValue = default) => defaultValue;
            public bool HasData(string key) => false;
            public bool HasData(Enum key) => false;
            public void DeleteData(string key) { }
            public void DeleteData(Enum key) { }
        }
    }
}
#endif
