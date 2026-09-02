using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyTimingTests
    {
        [Test]
        public void Profile_ExposesPostActionDecisionDelaySeconds()
        {
            Type profileType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyBehaviourProfile");

            PropertyInfo delayProp = profileType.GetProperty("PostActionDecisionDelaySeconds");
            PropertyInfo waitProp = profileType.GetProperty("WaitSeconds");

            Assert.That(delayProp, Is.Not.Null, "EnemyBehaviourProfile must expose PostActionDecisionDelaySeconds.");
            Assert.That(waitProp, Is.Not.Null, "EnemyBehaviourProfile must expose backward-compatible WaitSeconds.");

            ScriptableObject profile = ScriptableObject.CreateInstance(profileType);
            try
            {
                FieldInfo field = profileType.GetField("postActionDecisionDelaySeconds", BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue(profile, 0.45f);

                Assert.That((float)delayProp.GetValue(profile), Is.EqualTo(0.45f));
                Assert.That((float)waitProp.GetValue(profile), Is.EqualTo(0.45f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void Controller_ExposesDedicatedTimerFields_AndNoWaitUntil()
        {
            Type controllerType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyController");

            FieldInfo waitUntilField = controllerType.GetField("_waitUntil", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo patrolWaitField = controllerType.GetField("_patrolWaitUntil", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo postActionField = controllerType.GetField("_postActionDecisionUntil", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(waitUntilField, Is.Null, "EnemyController must not have a generic _waitUntil field.");
            Assert.That(patrolWaitField, Is.Not.Null, "EnemyController must have _patrolWaitUntil.");
            Assert.That(postActionField, Is.Not.Null, "EnemyController must have _postActionDecisionUntil.");
        }

        [Test]
        public void Controller_ActionCompletion_SetsPostActionDelay_OnlyForLocomotionMode()
        {
            Type controllerType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyController");
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type profileType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyBehaviourProfile");
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            Type coordinatorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyGroupCoordinator");
            Type modeType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode");

            var gameObject = new GameObject("TimingTestEnemy");
            ScriptableObject profile = ScriptableObject.CreateInstance(profileType);
            try
            {
                FieldInfo delayField = profileType.GetField("postActionDecisionDelaySeconds", BindingFlags.Instance | BindingFlags.NonPublic);
                delayField.SetValue(profile, 0.5f);

                Component actor = gameObject.AddComponent(actorType);
                FieldInfo profileField = actorType.GetField("behaviourProfile", BindingFlags.Instance | BindingFlags.NonPublic);
                profileField.SetValue(actor, profile);

                Component executor = gameObject.AddComponent(executorType);
                Component controller = gameObject.AddComponent(controllerType);

                object coordinator = Activator.CreateInstance(coordinatorType, 1, 3f);

                // Inject actor, executor, coordinator into controller
                controllerType.GetField("_actor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, actor);
                controllerType.GetField("_executor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, executor);
                controllerType.GetField("_groupCoordinator", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, coordinator);

                FieldInfo postActionField = controllerType.GetField("_postActionDecisionUntil", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo onCompletedMethod = controllerType.GetMethod("OnActionCompleted", BindingFlags.Instance | BindingFlags.NonPublic);

                // 1. When executor Mode is Action (intermediate combo move), delay should NOT be set
                executorType.GetField("<Mode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, Enum.Parse(modeType, "Action"));
                postActionField.SetValue(controller, 0f);

                onCompletedMethod.Invoke(controller, new object[] { null });
                Assert.That((float)postActionField.GetValue(controller), Is.EqualTo(0f), "Intermediate combo completion must not set post-action delay.");

                // 2. When executor Mode is Locomotion (final completion), delay MUST be set
                executorType.GetField("<Mode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(executor, Enum.Parse(modeType, "Locomotion"));

                onCompletedMethod.Invoke(controller, new object[] { null });
                Assert.That((float)postActionField.GetValue(controller), Is.GreaterThan(0f), "Action chain completion must set post-action delay.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
