using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyActorLifetimeTests
    {
        private static int _despawnNotifications;

        [Test]
        public void DespawnRemainsIdempotentWhenNotificationThrows()
        {
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            Type scopeType = GetRequiredType("SoulsLike.Services.VContainer.EntityLifetimeScope");
            Type concreteScopeType = GetRequiredType("SoulsLike.Services.VContainer.EnemyScopeInstaller");
            Type handlerType = GetRequiredType("SoulsLike.Services.VContainer.EnemyDespawnHandler");
            var actorObject = new GameObject("EnemyActor");
            var scopeObject = new GameObject("EntityScope");
            scopeObject.SetActive(false);
            _despawnNotifications = 0;

            try
            {
                Component scope = scopeObject.AddComponent(concreteScopeType);
                scopeType.GetField("autoRun").SetValue(scope, false);
                Component actor = actorObject.AddComponent(actorType);
                ConstructorInfo handlerConstructor = handlerType.GetConstructor(new[] { scopeType });
                Assert.That(handlerConstructor, Is.Not.Null);
                object handler = handlerConstructor.Invoke(new object[] { scope });
                actorType.GetMethod("Construct").Invoke(actor, new[] { null, null, null, handler });

                EventInfo despawned = actorType.GetEvent("Despawned");
                MethodInfo callback = typeof(EnemyActorLifetimeTests).GetMethod(
                    nameof(ThrowOnDespawn),
                    BindingFlags.Static | BindingFlags.NonPublic);
                Delegate throwingSubscriber = Delegate.CreateDelegate(despawned.EventHandlerType, callback);
                despawned.AddEventHandler(actor, throwingSubscriber);

                MethodInfo despawn = actorType.GetMethod("Despawn");
                LogAssert.Expect(
                    LogType.Error,
                    new Regex("Destroy may not be called from edit mode! Use DestroyImmediate instead\\."));
                TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                    () => despawn.Invoke(actor, null));

                Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(_despawnNotifications, Is.EqualTo(1));
                Assert.DoesNotThrow(() => despawn.Invoke(actor, null));
                Assert.That(_despawnNotifications, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(actorObject);
                UnityEngine.Object.DestroyImmediate(scopeObject);
            }
        }

        private static void ThrowOnDespawn(Component actor)
        {
            _despawnNotifications++;
            throw new InvalidOperationException($"{actor.name} despawn notification failed.");
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
