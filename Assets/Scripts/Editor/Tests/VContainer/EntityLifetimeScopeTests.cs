#if UNITY_EDITOR
using System;
using NUnit.Framework;
using SoulsLike.Services.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Editor.Tests.DependencyInjection
{
    public sealed class EntityLifetimeScopeTests
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
        public void BuildOnce_PropagatesInitializationFailureAndRejectsSecondBuild()
        {
            _scopeObject = new GameObject("EntityLifetimeScope_Test");
            _scopeObject.SetActive(false);
            ThrowingEntityLifetimeScope scope = _scopeObject.AddComponent<ThrowingEntityLifetimeScope>();
            scope.autoRun = false;
            _scopeObject.SetActive(true);

            InvalidOperationException initializationFailure = Assert.Throws<InvalidOperationException>(
                scope.BuildOnce);
            Assert.That(initializationFailure.Message, Is.EqualTo(ThrowingInitializable.FAILURE_MESSAGE));

            InvalidOperationException secondBuildFailure = Assert.Throws<InvalidOperationException>(
                scope.BuildOnce);
            Assert.That(secondBuildFailure.Message, Does.Contain("only be built once"));
        }

        private sealed class ThrowingEntityLifetimeScope : EntityLifetimeScope
        {
            protected override void Configure(IContainerBuilder builder)
            {
                builder.RegisterEntryPointExceptionHandler(exception => throw exception);
                builder.Register<ThrowingInitializable>(Lifetime.Singleton).As<IInitializable>();
            }
        }

        private sealed class ThrowingInitializable : IInitializable
        {
            public const string FAILURE_MESSAGE = "Expected entity scope initialization failure.";

            public void Initialize()
            {
                throw new InvalidOperationException(FAILURE_MESSAGE);
            }
        }
    }
}
#endif
