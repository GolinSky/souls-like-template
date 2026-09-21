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
            Type defenseType = GetRequiredType("SoulsLike.Entities.Combat.CombatDefenseComponent");
            Type motorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyNavigationMotor");
            Type navMeshAgentType = GetRequiredType("UnityEngine.AI.NavMeshAgent");
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode");

            var gameObject = new GameObject("ExecutorTest");
            var healthGo = new GameObject("HealthGo");
            try
            {
                Component executor = gameObject.AddComponent(executorType);
                Component health = healthGo.AddComponent(healthType);
                Component defense = gameObject.AddComponent(defenseType);
                Component navMeshAgent = gameObject.AddComponent(navMeshAgentType);
                CharacterController characterController = gameObject.AddComponent<CharacterController>();
                Component motor = gameObject.AddComponent(motorType);
                Animator animator = gameObject.AddComponent<Animator>();

                FieldInfo healthField = executorType.GetField(
                    "_health",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                healthField.SetValue(executor, health);
                executorType.GetField("_defense", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, defense);
                executorType.GetField("motor", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, motor);
                executorType.GetField("animator", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, animator);
                motorType.GetField("agent", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(motor, navMeshAgent);
                motorType.GetField("controller", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(motor, characterController);

                MethodInfo reportEnterMethod = executorType.GetMethod("ReportGetUpEntered");
                MethodInfo reportExitMethod = executorType.GetMethod("ReportGetUpExited");
                PropertyInfo modeProp = executorType.GetProperty("Mode");
                PropertyInfo blocksDecisionsProp = executorType.GetProperty("BlocksDecisions");
                PropertyInfo isInvulnerableProp = healthType.GetProperty("IsInvulnerable");
                PropertyInfo isInCriticalStateProp = defenseType.GetProperty("IsInCriticalState");

                defenseType.GetMethod("SetCriticalState").Invoke(defense, new object[] { true });
                reportEnterMethod.Invoke(executor, null);
                Assert.That(modeProp.GetValue(executor), Is.EqualTo(Enum.Parse(modeType, "GetUp")));
                Assert.That(blocksDecisionsProp.GetValue(executor), Is.True);
                Assert.That(isInvulnerableProp.GetValue(health), Is.True);
                Assert.That(isInCriticalStateProp.GetValue(defense), Is.False);

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

        [TestCase(65f, 1f)]
        [TestCase(-65f, -1f)]
        public void NavigationMotor_RootMotionRotation_ClampsAtTargetYawWithoutReversing(
            float requestedYaw,
            float direction)
        {
            Type motorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyNavigationMotor");
            Type navMeshAgentType = GetRequiredType("UnityEngine.AI.NavMeshAgent");
            var gameObject = new GameObject("NavigationMotorTest");
            try
            {
                Component navMeshAgent = gameObject.AddComponent(navMeshAgentType);
                CharacterController characterController = gameObject.AddComponent<CharacterController>();
                Component motor = gameObject.AddComponent(motorType);
                motorType.GetField("agent", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(motor, navMeshAgent);
                motorType.GetField("controller", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(motor, characterController);

                MethodInfo applyRotationMethod = motorType.GetMethod("ApplyRootMotionRotation");
                float targetYaw = Mathf.Repeat(requestedYaw, 360f);
                object[] arguments = { Quaternion.Euler(0f, 90f, 0f), targetYaw, direction };
                applyRotationMethod.Invoke(motor, arguments);
                Assert.That(Mathf.DeltaAngle(gameObject.transform.eulerAngles.y, targetYaw),
                    Is.EqualTo(0f).Within(0.01f));

                applyRotationMethod.Invoke(motor, arguments);
                Assert.That(Mathf.DeltaAngle(gameObject.transform.eulerAngles.y, targetYaw),
                    Is.EqualTo(0f).Within(0.01f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [TestCase(180f, "TurnR180")]
        [TestCase(-180f, "Turn180")]
        public void Executor_TurnStateMappingUsesSigned180Clips(float signedAngle, string expectedState)
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            MethodInfo resolveTurnStateMethod = executorType.GetMethod("ResolveTurnStateName");

            Assert.That(resolveTurnStateMethod.Invoke(null, new object[] { signedAngle }),
                Is.EqualTo(expectedState));
        }

        private static Type GetRequiredType(string typeName)
        {
            Type direct = Type.GetType(typeName);
            if (direct != null)
            {
                return direct;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
        }
    }
}
