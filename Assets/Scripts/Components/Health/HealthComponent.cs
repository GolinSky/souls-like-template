using System;
using SoulsLike.Entities.Character.Components;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Health
{
    public class HealthComponent : BaseComponent<HealthModel>, IHealthComponent, IInitializable
    {
        private IComponentMediator _mediator;
        private HealthStats _stats;

        public event Action<HealthStats> OnStatsChanged;
        public event Action<DamageResult> OnDamageApplied;
        public event Action OnDied;

        public HealthStats Stats => _stats;

        public void Initialize()
        {
            ApplyAuthoritativeStats(BuildDefaultStats());
        }

        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
        }

        public HealthStats BuildDefaultStats()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{name} requires a HealthModel.");
            }

            float maxHealth = Mathf.Max(1f, Model.MaxHealth);
            float currentHealth = Mathf.Clamp(Model.StartingHealth, 0f, maxHealth);
            return new HealthStats
            {
                CurrentHealth = currentHealth,
                MaxHealth = maxHealth,
                IsAlive = currentHealth > 0f
            };
        }

        public HealthStats ApplyStatUpdate(HealthStats currentStats, HealthStatUpdate update)
        {
            HealthStats stats = NormalizeStats(currentStats);

            if (update.SetMaxHealth)
            {
                stats.MaxHealth = Mathf.Max(1f, update.MaxHealth);
                stats.CurrentHealth = Mathf.Clamp(stats.CurrentHealth, 0f, stats.MaxHealth);
            }

            if (update.SetCurrentHealth)
            {
                stats.CurrentHealth = Mathf.Clamp(update.CurrentHealth, 0f, stats.MaxHealth);
            }

            stats.IsAlive = stats.CurrentHealth > 0f;
            return NormalizeStats(stats);
        }

        public DamageResult CalculateDamage(DamageRequest request, HealthStats currentStats)
        {
            HealthStats stats = NormalizeStats(currentStats);
            float incomingAmount = Mathf.Max(0f, request.Amount);

            if (!stats.IsAlive || incomingAmount <= 0f)
            {
                return new DamageResult
                {
                    IncomingAmount = incomingAmount,
                    HealthDamageAmount = 0f,
                    NewStats = stats,
                    Killed = false
                };
            }

            float healthDamage = incomingAmount;
            float nextHealth = stats.CurrentHealth - healthDamage;

            if (Model != null && !Model.CanDie)
            {
                nextHealth = Mathf.Max(1f, nextHealth);
            }

            stats.CurrentHealth = Mathf.Clamp(nextHealth, 0f, stats.MaxHealth);
            bool wasAlive = currentStats.IsAlive;
            stats.IsAlive = stats.CurrentHealth > 0f;

            return new DamageResult
            {
                IncomingAmount = incomingAmount,
                HealthDamageAmount = healthDamage,
                NewStats = stats,
                Killed = wasAlive && !stats.IsAlive
            };
        }

        public HealthStats CalculateHeal(HealthStats currentStats, float amount)
        {
            HealthStats stats = NormalizeStats(currentStats);
            if (!stats.IsAlive || amount <= 0f) return stats;

            stats.CurrentHealth = Mathf.Clamp(stats.CurrentHealth + amount, 0f, stats.MaxHealth);
            stats.IsAlive = stats.CurrentHealth > 0f;
            return stats;
        }

        public HealthStats CalculateRevive(HealthStats currentStats, float health)
        {
            HealthStats stats = NormalizeStats(currentStats);
            stats.CurrentHealth = Mathf.Clamp(health, 0f, stats.MaxHealth);
            stats.IsAlive = stats.CurrentHealth > 0f;
            return stats;
        }

        public void ApplyAuthoritativeStats(HealthStats stats)
        {
            bool died = _stats.IsAlive && !stats.IsAlive;
            _stats = NormalizeStats(stats);
            OnStatsChanged?.Invoke(_stats);

            if (died)
            {
                OnDied?.Invoke();
            }
        }

        public void NotifyDamageApplied(DamageResult result)
        {
            OnDamageApplied?.Invoke(result);
        }

        private HealthStats NormalizeStats(HealthStats stats)
        {
            stats.MaxHealth = Mathf.Max(1f, stats.MaxHealth);
            stats.CurrentHealth = Mathf.Clamp(stats.CurrentHealth, 0f, stats.MaxHealth);
            stats.IsAlive = stats.CurrentHealth > 0f;
            return stats;
        }
    }
}
