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
