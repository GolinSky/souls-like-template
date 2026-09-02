using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyCriticalLifecycleTests
    {
        [Test]
        public void HealthComponent_RecoveryInvulnerability_OperatesIndependentlyFromGraceAndCheat()
        {
            Type healthType = GetRequiredType("SoulsLike.Entities.Character.Components.Health.HealthComponent");
            var gameObject = new GameObject("HealthComponentTest");
            try
            {
                Component health = gameObject.AddComponent(healthType);
                PropertyInfo isInvulnerableProp = healthType.GetProperty("IsInvulnerable");
                PropertyInfo isCheatInvulnerableProp = healthType.GetProperty("IsCheatInvulnerable");
                MethodInfo setRecoveryMethod = healthType.GetMethod("SetRecoveryInvulnerable");
                MethodInfo setInvulnerableMethod = healthType.GetMethod("SetInvulnerable");

                Assert.That(isInvulnerableProp.GetValue(health), Is.False);
                Assert.That(isCheatInvulnerableProp.GetValue(health), Is.False);

                setRecoveryMethod.Invoke(health, new object[] { true });
                Assert.That(isInvulnerableProp.GetValue(health), Is.True);
                Assert.That(isCheatInvulnerableProp.GetValue(health), Is.False);

                // Setting grace invulnerability should keep total invulnerable even if recovery ends
                setInvulnerableMethod.Invoke(health, new object[] { true });
                setRecoveryMethod.Invoke(health, new object[] { false });
                Assert.That(isInvulnerableProp.GetValue(health), Is.True);

                setInvulnerableMethod.Invoke(health, new object[] { false });
                Assert.That(isInvulnerableProp.GetValue(health), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Executor_ExposesGetUpAndCriticalLifecycleCallbacks()
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");

            Assert.That(executorType.GetMethod("ReportGetUpEntered"), Is.Not.Null);
            Assert.That(executorType.GetMethod("ReportGetUpExited"), Is.Not.Null);
            Assert.That(executorType.GetMethod("ReportCriticalVictimEntered"), Is.Not.Null);
            Assert.That(executorType.GetMethod("ReportCriticalVictimExited"), Is.Not.Null);
        }

        [Test]
        public void Executor_GetUpLifecycle_TransitionsModeAndTogglesRecoveryInvulnerability()
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type healthType = GetRequiredType("SoulsLike.Entities.Character.Components.Health.HealthComponent");
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode");

            var gameObject = new GameObject("ExecutorTest");
            var healthGo = new GameObject("HealthGo");
            try
            {
                Component executor = gameObject.AddComponent(executorType);
                Component health = healthGo.AddComponent(healthType);

                FieldInfo healthField = executorType.GetField(
                    "_health",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                healthField.SetValue(executor, health);

                MethodInfo reportEnterMethod = executorType.GetMethod("ReportGetUpEntered");
                MethodInfo reportExitMethod = executorType.GetMethod("ReportGetUpExited");
                PropertyInfo modeProp = executorType.GetProperty("Mode");
                PropertyInfo blocksDecisionsProp = executorType.GetProperty("BlocksDecisions");
                PropertyInfo isInvulnerableProp = healthType.GetProperty("IsInvulnerable");

                reportEnterMethod.Invoke(executor, null);
                Assert.That(modeProp.GetValue(executor), Is.EqualTo(Enum.Parse(modeType, "GetUp")));
                Assert.That(blocksDecisionsProp.GetValue(executor), Is.True);
                Assert.That(isInvulnerableProp.GetValue(health), Is.True);

                reportExitMethod.Invoke(executor, null);
                Assert.That(modeProp.GetValue(executor), Is.EqualTo(Enum.Parse(modeType, "Locomotion")));
                Assert.That(blocksDecisionsProp.GetValue(executor), Is.False);
                Assert.That(isInvulnerableProp.GetValue(health), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(healthGo);
            }
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
