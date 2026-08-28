using System;
using SoulsLike.Components.Visibility;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyHealthUiComponent : MonoBehaviour, IEnemyHealthUiSource,
        IVisibilityObserver, IInitializable, IDisposable
    {
        private HealthModel _healthModel;
        private IEnemyHealthUiService _enemyHealthUiService;
        private VisibilityComponent _visibilityComponent;

        public float CurrentHealth => _healthModel.Stats.CurrentHealth;
        public float MaxHealth => _healthModel.Stats.MaxHealth;
        public Vector3 WorldPosition => transform.position;

        [Inject]
        public void Construct(
            HealthModel healthModel,
            IEnemyHealthUiService enemyHealthUiService,
            VisibilityComponent visibilityComponent)
        {
            _healthModel = healthModel;
            _enemyHealthUiService = enemyHealthUiService;
            _visibilityComponent = visibilityComponent;
        }

        public void Initialize()
        {
            _healthModel.OnStatsChanged += HandleStatsChanged;
            _healthModel.OnDied += HandleDead;
            _enemyHealthUiService.Track(this);
            _visibilityComponent.RegisterObserver(this);
        }

        public void Dispose()
        {
            _healthModel.OnStatsChanged -= HandleStatsChanged;
            _healthModel.OnDied -= HandleDead;
            _visibilityComponent.UnregisterObserver(this);
            _enemyHealthUiService.Release(this);
        }

        private void HandleDead(long _)
        {
            _healthModel.OnStatsChanged -= HandleStatsChanged;
            _healthModel.OnDied -= HandleDead;
            _visibilityComponent.UnregisterObserver(this);
            _enemyHealthUiService.Release(this);
        }

        public void OnVisibilityChanged(bool isVisible)
        {
            _enemyHealthUiService.NotifyVisibilityChanged(this, isVisible);
        }

        private void HandleStatsChanged(HealthStats stats)
        {
            _enemyHealthUiService.NotifyHealthChanged(
                this,
                stats.CurrentHealth,
                stats.MaxHealth);
        }
    }
}
