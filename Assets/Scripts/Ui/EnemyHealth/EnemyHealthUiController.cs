using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Enemy;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class EnemyHealthUiController : UiController, IInitializable, IPostLateTickable, IDisposable
    {
        private sealed class TrackedEnemy
        {
            public EnemyActor Actor { get; }
            public HealthModel HealthModel { get; }
            public EnemyHealthBarUi Bar { get; }
            public Action<HealthStats> StatsChangedHandler { get; }

            public TrackedEnemy(
                EnemyActor actor,
                HealthModel healthModel,
                EnemyHealthBarUi bar,
                Action<HealthStats> statsChangedHandler)
            {
                Actor = actor;
                HealthModel = healthModel;
                Bar = bar;
                StatsChangedHandler = statsChangedHandler;
            }
        }

        private readonly ICameraService _cameraService;
        private readonly List<TrackedEnemy> _trackedEnemies = new();

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

        public void Track(EnemyActor actor, HealthModel healthModel)
        {
            EnemyHealthBarUi bar = _enemyHealthUi.AcquireBar();
            Action<HealthStats> statsChangedHandler = stats => bar.SetValue(
                stats.CurrentHealth,
                stats.MaxHealth);
            var trackedEnemy = new TrackedEnemy(
                actor,
                healthModel,
                bar,
                statsChangedHandler);

            healthModel.OnStatsChanged += statsChangedHandler;
            statsChangedHandler(healthModel.Stats);
            _trackedEnemies.Add(trackedEnemy);
        }

        public void PostLateTick()
        {
            for (int index = _trackedEnemies.Count - 1; index >= 0; index--)
            {
                TrackedEnemy trackedEnemy = _trackedEnemies[index];
                if (trackedEnemy.Actor == null)
                {
                    ReleaseTrackedEnemy(index, trackedEnemy);
                    continue;
                }

                bool isVisible = trackedEnemy.HealthModel.Stats.IsAlive
                    && _enemyHealthUi.TrySetBarPosition(
                        trackedEnemy.Bar,
                        trackedEnemy.Actor.transform.position,
                        _targetCamera);
                trackedEnemy.Bar.SetVisible(isVisible);
            }
        }

        public void Dispose()
        {
            foreach (TrackedEnemy trackedEnemy in _trackedEnemies)
            {
                trackedEnemy.HealthModel.OnStatsChanged -= trackedEnemy.StatsChangedHandler;
            }

            _trackedEnemies.Clear();
        }

        private void ReleaseTrackedEnemy(int index, TrackedEnemy trackedEnemy)
        {
            trackedEnemy.HealthModel.OnStatsChanged -= trackedEnemy.StatsChangedHandler;
            _enemyHealthUi.ReleaseBar(trackedEnemy.Bar);
            _trackedEnemies.RemoveAt(index);
        }
    }
}
