using System;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyHealthUiComponent : MonoBehaviour, IEnemyHealthUiSource,
        IInitializable, IDisposable
    {
        private HealthModel _healthModel;
        private IEnemyHealthUiService _enemyHealthUiService;

        public float CurrentHealth => _healthModel.Stats.CurrentHealth;
        public float MaxHealth => _healthModel.Stats.MaxHealth;
        public Vector3 WorldPosition => transform.position;

        [Inject]
        public void Construct(
            HealthModel healthModel,
            IEnemyHealthUiService enemyHealthUiService)
        {
            _healthModel = healthModel;
            _enemyHealthUiService = enemyHealthUiService;
        }

        public void Initialize()
        {
            _healthModel.OnStatsChanged += HandleStatsChanged;
            _healthModel.OnDied += HandleDead;
            _enemyHealthUiService.Track(this);
            _enemyHealthUiService.NotifyVisibilityChanged(this, true);
        }

        public void Dispose()
        {
            _healthModel.OnStatsChanged -= HandleStatsChanged;
            _healthModel.OnDied -= HandleDead;
            _enemyHealthUiService.Release(this);
        }

        private void HandleDead(long _)
        {
            _healthModel.OnStatsChanged -= HandleStatsChanged;
            _healthModel.OnDied -= HandleDead;
            _enemyHealthUiService.Release(this);
        }

        private void HandleStatsChanged(HealthStats stats)
        {
            _enemyHealthUiService.NotifyHealthChanged(
                this,
                stats.CurrentHealth,
                stats.MaxHealth);
        }

        // private void OnBecameVisible()
        // {
        //     _enemyHealthUiService.NotifyVisibilityChanged(this, true);
        // }
        //
        // private void OnBecameInvisible()
        // {
        //     _enemyHealthUiService.NotifyVisibilityChanged(this, false);
        // }
    }
}
