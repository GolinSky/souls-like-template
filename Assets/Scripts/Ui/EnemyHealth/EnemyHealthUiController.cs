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
            var trackedEnemy = new TrackedEnemyData(source, bar);

            bar.SetValue(source.CurrentHealth, source.MaxHealth);
            bar.SetVisible(false);
            _trackedEnemies.Add(trackedEnemy);
        }

        public void Release(IEnemyHealthUiSource source)
        {
            int index = _trackedEnemies.FindIndex(trackedEnemy => trackedEnemy.Source == source);
            if (index < 0)
            {
                return;
            }

            _enemyHealthUi.ReleaseBar(_trackedEnemies[index].Bar);
            _trackedEnemies.RemoveAt(index);
        }

        public void NotifyHealthChanged(
            IEnemyHealthUiSource source,
            float currentHealth,
            float maxHealth)
        {
            TrackedEnemyData trackedEnemy = _trackedEnemies.Find(data => data.Source == source);
            trackedEnemy.Bar.SetValue(currentHealth, maxHealth);
        }

        public void NotifyVisibilityChanged(IEnemyHealthUiSource source, bool isVisible)
        {
            int index = _trackedEnemies.FindIndex(trackedEnemy => trackedEnemy.Source == source);
            if (index < 0)
            {
                return;
            }

            TrackedEnemyData trackedEnemy = _trackedEnemies[index];
            trackedEnemy.IsVisible = isVisible;

            if (!isVisible)
            {
                trackedEnemy.Bar.SetVisible(false);
            }
        }

        public void PostLateTick()
        {
            for (int index = _trackedEnemies.Count - 1; index >= 0; index--)
            {
                TrackedEnemyData trackedEnemyData = _trackedEnemies[index];
                if (!trackedEnemyData.IsVisible)
                {
                    continue;
                }

                bool isVisible = _enemyHealthUi.TrySetBarPosition(
                    trackedEnemyData.Bar,
                    trackedEnemyData.Source.WorldPosition,
                    _targetCamera);
                trackedEnemyData.Bar.SetVisible(isVisible);
            }
        }

        public void Dispose()
        {
            _trackedEnemies.Clear();
        }
    }
}
