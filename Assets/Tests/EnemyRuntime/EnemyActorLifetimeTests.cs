using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyActorLifetimeTests
    {
        [Test]
        public void LifetimeRootCanOnlyBeAttachedOnce()
        {
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            var actorObject = new GameObject("EnemyActor");
            var firstRoot = new GameObject("FirstLifetimeRoot");
            var secondRoot = new GameObject("SecondLifetimeRoot");
            try
            {
                Component actor = actorObject.AddComponent(actorType);
                MethodInfo attachLifetimeRoot = actorType.GetMethod("AttachLifetimeRoot");

                attachLifetimeRoot.Invoke(actor, new object[] { firstRoot });

                Assert.That(
                    () => attachLifetimeRoot.Invoke(actor, new object[] { secondRoot }),
                    Throws.TypeOf<TargetInvocationException>());
                Assert.That(actorType.GetEvent("Despawned"), Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(actorObject);
                UnityEngine.Object.DestroyImmediate(firstRoot);
                UnityEngine.Object.DestroyImmediate(secondRoot);
            }
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
