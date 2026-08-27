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
        private bool _isDisposed;

        public event Action<float, float> HealthChanged;

        public bool IsAvailable => !_isDisposed && this != null;
        public bool ShouldShow => isActiveAndEnabled && _healthModel.Stats.IsAlive;
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
            _enemyHealthUiService.Track(this);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _healthModel.OnStatsChanged -= HandleStatsChanged;
        }

        private void HandleStatsChanged(HealthStats stats)
        {
            HealthChanged?.Invoke(stats.CurrentHealth, stats.MaxHealth);
        }
    }
}
