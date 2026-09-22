#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using SoulsLike.Services;
using SoulsLike.Services.PlayerSession;
using SoulsLike.Services.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Editor.Tests.DependencyInjection
{
    public sealed class CharacterScopeInstallerTests
    {
        private GameObject _scopeObject;

        [TearDown]
        public void TearDown()
        {
            if (_scopeObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_scopeObject);
            }
        }

        [Test]
        public void RegisterLocalPlayerServices_BuildsOnePlayerSessionRegistrationForAllContracts()
        {
            _scopeObject = new GameObject("CharacterScopeInstaller_Test");
            _scopeObject.SetActive(false);
            CharacterScopeInstaller installer = _scopeObject.AddComponent<CharacterScopeInstaller>();
            ContainerBuilder builder = new();
            MethodInfo registrationMethod = typeof(CharacterScopeInstaller).GetMethod(
                "RegisterLocalPlayerServices",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(registrationMethod, Is.Not.Null);
            registrationMethod.Invoke(installer, new object[] { builder });

            using IObjectResolver container = builder.Build();

            Assert.That(container.TryGetRegistration(typeof(IPlayerSession), out Registration sessionRegistration), Is.True);
            Assert.That(sessionRegistration.ImplementationType, Is.EqualTo(typeof(PlayerSessionCoordinator)));
            Assert.That(sessionRegistration.Lifetime, Is.EqualTo(Lifetime.Singleton));
            Assert.That(sessionRegistration.InterfaceTypes, Is.EquivalentTo(new[]
            {
                typeof(PlayerSessionCoordinator),
                typeof(IPlayerSession),
                typeof(IGameStateObserver),
                typeof(IDisposable),
                typeof(IInitializable)
            }));

            Assert.That(
                container.TryGetRegistration(typeof(PlayerSessionCoordinator), out Registration concreteRegistration),
                Is.True);
            Assert.That(concreteRegistration, Is.SameAs(sessionRegistration));
        }
    }
}
#endif
