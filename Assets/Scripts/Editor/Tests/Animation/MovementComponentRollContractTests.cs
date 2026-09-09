using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Movement;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class MovementComponentRollContractTests
    {
        private const string MOVEMENT_DATA_PATH = "Assets/Settings/Player/MovementData.asset";

        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in _createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void SetMovementBlocked_RepeatedUnblockedSynchronization_PreservesActiveLockedRollMetadata()
        {
            MovementComponent movement = CreateLockedRoll();
            Transform activeRollTarget = GetPrivateField<Transform>(movement, "_activeRollTarget");
            Vector2 activeRollDirection = GetPrivateField<Vector2>(movement, "_activeRollDirection");

            movement.SetMovementBlocked(false);

            Assert.That(GetPrivateField<Transform>(movement, "_activeRollTarget"), Is.SameAs(activeRollTarget));
            Assert.That(GetPrivateField<Vector2>(movement, "_activeRollDirection"), Is.EqualTo(activeRollDirection));
        }

        [Test]
        public void SetMovementBlocked_RootMotionContractSynchronizations_PreserveActiveLockedRollMetadata()
        {
            MovementComponent movement = CreateLockedRoll();
            Transform activeRollTarget = GetPrivateField<Transform>(movement, "_activeRollTarget");
            Vector2 activeRollDirection = GetPrivateField<Vector2>(movement, "_activeRollDirection");

            movement.SetMovementBlocked(true);
            movement.SetMovementBlocked(true);

            Assert.That(GetPrivateField<Transform>(movement, "_activeRollTarget"), Is.SameAs(activeRollTarget));
            Assert.That(GetPrivateField<Vector2>(movement, "_activeRollDirection"), Is.EqualTo(activeRollDirection));
        }

        [Test]
        public void SetMovementBlocked_BlockedToUnblocked_ClearsActiveLockedRollMetadata()
        {
            MovementComponent movement = CreateLockedRoll();

            movement.SetMovementBlocked(true);
            movement.SetMovementBlocked(false);

            Assert.That(GetPrivateField<Transform>(movement, "_activeRollTarget"), Is.Null);
            Assert.That(GetPrivateField<Vector2>(movement, "_activeRollDirection"), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SetMovementBlocked_RepeatedBlockedSynchronization_StopsGroundedHorizontalVelocity()
        {
            MovementComponent movement = CreateLockedRoll();

            SetPrivateField(movement, "_horizontalVelocity", Vector3.right);
            movement.SetMovementBlocked(true);
            Assert.That(GetPrivateField<Vector3>(movement, "_horizontalVelocity"), Is.EqualTo(Vector3.zero));

            SetPrivateField(movement, "_horizontalVelocity", Vector3.forward);
            movement.SetMovementBlocked(true);

            Assert.That(GetPrivateField<Vector3>(movement, "_horizontalVelocity"), Is.EqualTo(Vector3.zero));
        }

        private MovementComponent CreateLockedRoll()
        {
            GameObject characterObject = new GameObject("MovementComponentRollContractTestCharacter");
            _createdObjects.Add(characterObject);
            CharacterController controller = characterObject.AddComponent<CharacterController>();
            MovementComponent movement = characterObject.AddComponent<MovementComponent>();
            MovementData movementData = AssetDatabase.LoadAssetAtPath<MovementData>(MOVEMENT_DATA_PATH);

            Assert.That(movementData, Is.Not.Null, $"Missing movement data at {MOVEMENT_DATA_PATH}.");
            SetPrivateField(movement, "controller", controller);
            movement.Model = new MovementModel(movementData);
            movement.Initialize();
            movement.Model.Grounded = true;

            GameObject targetObject = new GameObject("MovementComponentRollContractTestTarget");
            _createdObjects.Add(targetObject);
            targetObject.transform.position = Vector3.right * 3.0f;
            movement.SetLockOnTarget(true, targetObject.transform);

            Assert.That(movement.TryStartRoll(Vector2.right, 0.0f, true, false), Is.True);
            return movement;
        }

        private static T GetPrivateField<T>(MovementComponent movement, string fieldName)
        {
            FieldInfo field = typeof(MovementComponent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing MovementComponent field '{fieldName}'.");
            return (T)field.GetValue(movement);
        }

        private static void SetPrivateField(MovementComponent movement, string fieldName, object value)
        {
            FieldInfo field = typeof(MovementComponent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing MovementComponent field '{fieldName}'.");
            field.SetValue(movement, value);
        }
    }
}
