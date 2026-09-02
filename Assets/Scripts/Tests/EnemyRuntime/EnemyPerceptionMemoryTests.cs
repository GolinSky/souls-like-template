using System;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyPerceptionMemoryTests
    {
        [Test]
        public void MemoryFreezesTheObservedTargetCoordinatesAndStimulus()
        {
            Type observationType = GetRequiredType("SoulsLike.Entities.Enemy.TargetObservation");
            Type memoryType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyMemory");
            Type stimulusType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyStimulusType");
            Type stimulusValueType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyStimulus");
            var position = new Vector3(1f, 2f, 3f);
            var lockPoint = new Vector3(4f, 5f, 6f);
            var forward = new Vector3(7f, 8f, 9f);
            object observation = Activator.CreateInstance(
                observationType,
                42L,
                position,
                lockPoint,
                forward,
                10f);
            object stimulus = Activator.CreateInstance(
                stimulusValueType,
                Enum.Parse(stimulusType, "Damage"),
                lockPoint,
                (long?)42L,
                1f,
                400,
                10f,
                16f);
            object memory = Activator.CreateInstance(
                memoryType,
                observation,
                stimulus);

            Assert.That(memoryType.GetProperty("EntityId").GetValue(memory), Is.EqualTo(42L));
            Assert.That(memoryType.GetProperty("LastKnownPosition").GetValue(memory), Is.EqualTo(position));
            Assert.That(memoryType.GetProperty("LastKnownLockPoint").GetValue(memory), Is.EqualTo(lockPoint));
            Assert.That(memoryType.GetProperty("LastConfirmedTime").GetValue(memory), Is.EqualTo(10f));
            Assert.That(memoryType.GetProperty("StimulusType").GetValue(memory).ToString(), Is.EqualTo("Damage"));
            Assert.That(memoryType.GetProperty("ForgetTime").GetValue(memory), Is.EqualTo(16f));
        }

        [TestCase("Sight")]
        [TestCase("Sound")]
        [TestCase("Damage")]
        [TestCase("AllyAlert")]
        public void StimulusTypesCoverTheAuthoredAwarenessSources(string stimulusType)
        {
            Type type = GetRequiredType("SoulsLike.Entities.Enemy.EnemyStimulusType");

            Assert.That(Enum.IsDefined(type, stimulusType), Is.True);
        }

        [Test]
        public void PerceptionKeepsTheGeneralStimulusContractWithoutASoundProducerWrapper()
        {
            Type perceptionType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyPerception");

            Assert.That(perceptionType.GetMethod("RegisterStimulus"), Is.Not.Null);
            Assert.That(perceptionType.GetMethod("RegisterSoundStimulus"), Is.Null);
        }

        [TestCase("Immediate")]
        [TestCase("Perception")]
        [TestCase("Triggered")]
        public void ActivationModesAreExplicit(string activationMode)
        {
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActivationMode");

            Assert.That(Enum.IsDefined(modeType, activationMode), Is.True);
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
