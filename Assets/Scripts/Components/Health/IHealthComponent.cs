using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    public interface IHealthComponent
    {
        event Action<HealthStats> OnStatsChanged;
        event Action<DamageResult> OnDamageApplied;
        event Action OnDied;

        HealthStats Stats { get; }

        void SetMediator(IComponentMediator mediator);
        HealthStats BuildDefaultStats();
        HealthStats ApplyStatUpdate(HealthStats currentStats, HealthStatUpdate update);
        DamageResult CalculateDamage(DamageRequest request, HealthStats currentStats);
        HealthStats CalculateHeal(HealthStats currentStats, float amount);
        HealthStats CalculateRevive(HealthStats currentStats, float health);
        void ApplyAuthoritativeStats(HealthStats stats);
        void NotifyDamageApplied(DamageResult result);
    }
}
