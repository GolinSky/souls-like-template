using System;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyGroupCoordinatorTests
    {
        [Test]
        public void GroupCoordinatorOwnsEncounterScopedAlertAndPressureApis()
        {
            Type type = GetRequiredType("SoulsLike.Entities.Enemy.EnemyGroupCoordinator");

            Assert.That(type.GetMethod("Register"), Is.Not.Null);
            Assert.That(type.GetMethod("Unregister"), Is.Not.Null);
            Assert.That(type.GetMethod("BroadcastAllyAlert"), Is.Not.Null);
            Assert.That(type.GetMethod("TryAcquirePressureSlot"), Is.Not.Null);
            Assert.That(type.GetMethod("ReleasePressureSlot"), Is.Not.Null);
            Assert.That(type.GetMethod("RenewPressureSlot"), Is.Not.Null);
        }

        [Test]
        public void ControllerCanReceiveButNotBroadcastAllyAlerts()
        {
            Type controllerType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyController");

            Assert.That(controllerType.GetMethod("ReceiveAllyAlert"), Is.Not.Null);
            Assert.That(controllerType.GetMethod("BroadcastAllyAlert"), Is.Null);
        }

        [Test]
        public void PressureSlotsHonorCapacityAndTimeout()
        {
            Type coordinatorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyGroupCoordinator");
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            object coordinator = Activator.CreateInstance(coordinatorType, 1, 3f);
            var firstObject = new GameObject("First Enemy");
            var secondObject = new GameObject("Second Enemy");
            try
            {
                Component firstActor = firstObject.AddComponent(actorType);
                Component secondActor = secondObject.AddComponent(actorType);
                var acquire = coordinatorType.GetMethod("TryAcquirePressureSlot");
                var tick = coordinatorType.GetMethod("Tick");

                Assert.That(acquire.Invoke(coordinator, new object[] { firstActor, 0f }), Is.True);
                Assert.That(acquire.Invoke(coordinator, new object[] { secondActor, 0f }), Is.False);
                tick.Invoke(coordinator, new object[] { 3f });
                Assert.That(acquire.Invoke(coordinator, new object[] { secondActor, 3f }), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstObject);
                UnityEngine.Object.DestroyImmediate(secondObject);
            }
        }

        [Test]
        public void ProfileExposesConcreteAllyAndPressureSettings()
        {
            Type profileType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyBehaviourProfile");

            Assert.That(profileType.GetProperty("SharesAllyAlerts"), Is.Not.Null);
            Assert.That(profileType.GetProperty("UsesPressureSlot"), Is.Not.Null);
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
