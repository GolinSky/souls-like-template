using System;
using System.Collections.Generic;
using SoulsLike.Entities.Enemy;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class EnemyHealthUiController : UiController, IEnemyHealthUiService,
        IInitializable, IPostLateTickable, IDisposable
    {
        private readonly ICameraService _cameraService;
        private readonly List<TrackedEnemyData> _trackedEnemies = new();

        private EnemyHealthUi _enemyHealthUi;
        private Camera _targetCamera;

        public EnemyHealthUiController(IUiService uiService, ICameraService cameraService)
            : base(uiService)
        {
            _cameraService = cameraService;
        }

        public void Initialize()
        {
            _enemyHealthUi = CreateUi<EnemyHealthUi>();
            UiService.MarkUiAsOverlay(_enemyHealthUi);
            _targetCamera = _cameraService.GetMainCamera();
            _enemyHealthUi.Show();
        }

        public void Track(IEnemyHealthUiSource source)
        {
            EnemyHealthBarUi bar = _enemyHealthUi.AcquireBar();
            Action<float, float> healthChangedHandler = bar.SetValue;
            var trackedEnemy = new TrackedEnemyData(
                source,
                bar,
                healthChangedHandler);

            source.HealthChanged += healthChangedHandler;
            healthChangedHandler(source.CurrentHealth, source.MaxHealth);
            _trackedEnemies.Add(trackedEnemy);
        }

        public void PostLateTick()
        {
            for (int index = _trackedEnemies.Count - 1; index >= 0; index--)
            {
                TrackedEnemyData trackedEnemyData = _trackedEnemies[index];
                if (!trackedEnemyData.Source.IsAvailable)
                {
                    ReleaseTrackedEnemy(index, trackedEnemyData);
                    continue;
                }

                bool isVisible = trackedEnemyData.Source.ShouldShow
                    && _enemyHealthUi.TrySetBarPosition(
                        trackedEnemyData.Bar,
                        trackedEnemyData.Source.WorldPosition,
                        _targetCamera);
                trackedEnemyData.Bar.SetVisible(isVisible);
            }
        }

        public void Dispose()
        {
            foreach (TrackedEnemyData trackedEnemy in _trackedEnemies)
            {
                trackedEnemy.Source.HealthChanged -= trackedEnemy.HealthChangedHandler;
            }

            _trackedEnemies.Clear();
        }

        private void ReleaseTrackedEnemy(int index, TrackedEnemyData trackedEnemyData)
        {
            trackedEnemyData.Source.HealthChanged -= trackedEnemyData.HealthChangedHandler;
            _enemyHealthUi.ReleaseBar(trackedEnemyData.Bar);
            _trackedEnemies.RemoveAt(index);
        }
    }
}
