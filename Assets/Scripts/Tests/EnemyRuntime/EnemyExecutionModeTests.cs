using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyExecutionModeTests
    {
        [TestCase("Locomotion", false)]
        [TestCase("Action", true)]
        [TestCase("Turn", true)]
        [TestCase("Reaction", true)]
        [TestCase("CriticalVictim", true)]
        [TestCase("GetUp", true)]
        [TestCase("Death", true)]
        public void ExecutionModeControlsDecisionBlocking(string modeName, bool blocksDecisions)
        {
            Type controllerType = GetRequiredType(
                "SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode");
            var gameObject = new GameObject("EnemyActionExecutor");
            try
            {
                Component controller = gameObject.AddComponent(controllerType);
                controllerType.GetField(
                    "<Mode>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                    controller,
                    Enum.Parse(modeType, modeName));

                Assert.That(
                    controllerType.GetProperty("BlocksDecisions").GetValue(controller),
                    Is.EqualTo(blocksDecisions));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TurnBlocksAttackStartUntilItCompletes()
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode");
            Type moveType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyMove");
            Type actionType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionDefinition");
            var gameObject = new GameObject("EnemyActionExecutor");
            ScriptableObject action = ScriptableObject.CreateInstance(actionType);
            try
            {
                Component executor = gameObject.AddComponent(executorType);
                object move = Activator.CreateInstance(moveType);
                moveType.GetField("action", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(move, action);
                executorType.GetField("<Mode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, Enum.Parse(modeType, "Turn"));

                Assert.That(executorType.GetMethod("TryStart").Invoke(executor, new[] { move }), Is.False);
                Assert.That(executorType.GetProperty("Mode").GetValue(executor),
                    Is.EqualTo(Enum.Parse(modeType, "Turn")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(action);
            }
        }

        [TestCase(false, 0f)]
        [TestCase(true, 30f)]
        public void TurnDoesNotStartWhenProfileDisablesIt(bool locksFacing, float turnThreshold)
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            Type profileType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyBehaviourProfile");
            var gameObject = new GameObject("EnemyActionExecutor");
            ScriptableObject profile = ScriptableObject.CreateInstance(profileType);
            try
            {
                profileType.GetField("locksFacing", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, locksFacing);
                profileType.GetField("turnInPlaceAngleThreshold", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, turnThreshold);
                Component actor = gameObject.AddComponent(actorType);
                actorType.GetField("<BehaviourProfile>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(actor, profile);
                Component executor = gameObject.AddComponent(executorType);
                executorType.GetField("actor", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, actor);

                Assert.That(executorType.GetMethod("TryPlayTurn").Invoke(executor, new object[] { 45f, 0f }), Is.False);
                Assert.That(executorType.GetProperty("IsTurnRunning").GetValue(executor), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ExecutorExposesTrackingAndExecutionEndSignals()
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");

            Assert.That(executorType.GetProperty("TrackingOpen"), Is.Not.Null);
            Assert.That(executorType.GetProperty("CurrentMoveStarted"), Is.Not.Null);
            Assert.That(executorType.GetEvent("ActionCompleted"), Is.Not.Null);
            Assert.That(executorType.GetEvent("Interrupted"), Is.Not.Null);
            Assert.That(
                Enum.IsDefined(
                    GetRequiredType("SoulsLike.Entities.Enemy.EnemyInterruptReason"),
                    "AnimatorEntryTimeout"),
                Is.True);
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
