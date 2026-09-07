using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SoulsLike.Entities.Enemy;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Settings;
using SoulsLike.Ui.Base;
using SoulsLike.Ui.EnemyHealth;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.EnemyHealth
{
    public sealed class EnemyHealthUiLifetimeTests
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Ui/EnemyHealth/EnemyHealthUi.prefab";

        private readonly List<EnemyHealthUiController> _controllers = new();
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (EnemyHealthUiController controller in _controllers)
            {
                controller.Dispose();
            }

            foreach (GameObject gameObject in _objects)
            {
                if (gameObject != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }

            _controllers.Clear();
            _objects.Clear();
        }

        [Test]
        public void DestroyedView_IgnoresLateSourceCallbacks()
        {
            EnemyHealthUiController controller = CreateController(out EnemyHealthUi view);
            var source = new TestEnemyHealthSource();
            controller.Track(source);

            // EditMode tests do not invoke this runtime lifecycle callback during destruction.
            typeof(EnemyHealthUi).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(view, null);
            Object.DestroyImmediate(view.gameObject);

            Assert.DoesNotThrow(() =>
            {
                controller.NotifyVisibilityChanged(source, false);
                controller.NotifyHealthChanged(source, 25f, 100f);
                controller.Release(source);
                controller.PostLateTick();
                controller.Dispose();
            });
        }

        [Test]
        public void Hide_PreservesTrackedBarForLiveHealthUpdates()
        {
            EnemyHealthUiController controller = CreateController(out EnemyHealthUi view);
            var source = new TestEnemyHealthSource();
            controller.Track(source);
            EnemyHealthBarUi bar = GetTrackedBar(controller);

            controller.NotifyHealthChanged(source, 75f, 100f);
            Assert.That(GetTargetFill(bar), Is.EqualTo(0.75f));

            view.Hide();
            controller.NotifyHealthChanged(source, 25f, 100f);

            Assert.That(GetTargetFill(bar), Is.EqualTo(0.25f));
        }

        private EnemyHealthUiController CreateController(out EnemyHealthUi view)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null);

            GameObject root = Object.Instantiate(prefab);
            _objects.Add(root);
            view = root.GetComponent<EnemyHealthUi>();
            Assert.That(view, Is.Not.Null);

            var controller = new EnemyHealthUiController(
                new TestUiService(view),
                new TestCameraService());
            controller.Initialize();
            _controllers.Add(controller);
            return controller;
        }

        private static EnemyHealthBarUi GetTrackedBar(EnemyHealthUiController controller)
        {
            var trackedEnemies = (List<TrackedEnemyData>)typeof(EnemyHealthUiController)
                .GetField("_trackedEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(controller);
            return trackedEnemies[0].Bar;
        }

        private static float GetTargetFill(EnemyHealthBarUi bar)
        {
            return (float)typeof(EnemyHealthBarUi)
                .GetField("_targetFill", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(bar);
        }

        private sealed class TestEnemyHealthSource : IEnemyHealthUiSource
        {
            public float CurrentHealth => 100f;
            public float MaxHealth => 100f;
            public Vector3 WorldPosition => Vector3.zero;
        }

        private sealed class TestUiService : IUiService
        {
            private readonly EnemyHealthUi _view;

            public TestUiService(EnemyHealthUi view)
            {
                _view = view;
            }

            public TUI CreateUi<TUI>(Transform uiParent = null)
                where TUI : IBaseUi
            {
                return (TUI)(IBaseUi)_view;
            }

            public void MarkUiAsOverlay(BaseUi baseUi)
            {
            }
        }

        private sealed class TestCameraService : ICameraService
        {
            public void SetTarget(Transform target)
            {
            }

            public void UpdateFollowTarget(bool grounded, float verticalVelocity)
            {
            }

            public void UpdateRotation(Vector2 look)
            {
            }

            public float GetYaw() => 0f;
            public void SwitchAngle()
            {
            }

            public void SetLockOnTarget(long? targetEntityId)
            {
            }

            public void ClearLockOnTarget()
            {
            }

            public void RecenterCamera()
            {
            }

            public Camera GetMainCamera() => null;

            public void ApplySettings(CameraSettingsData settings)
            {
            }
        }
    }
}
