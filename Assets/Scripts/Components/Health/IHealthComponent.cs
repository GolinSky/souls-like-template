namespace SoulsLike.Entities.Character.Components.Health
{
    public interface IHealthComponent
    {
        HealthStats Stats { get; }

        void SetMediator(IComponentMediator mediator);
        HealthStats BuildDefaultStats();
        HealthStats ApplyStatUpdate(HealthStats currentStats, HealthStatUpdate update);
        DamageResult CalculateDamage(DamageRequest request, HealthStats currentStats);
        HealthStats CalculateHeal(HealthStats currentStats, float amount);
        HealthStats CalculateRevive(HealthStats currentStats, float health);
        void ConsumeFocus(float amount);
        void RestoreFocus(float amount);
        void ConsumeStamina(float amount);
        void RestoreStamina(float amount);
        void ApplyAuthoritativeStats(HealthStats stats);
        void NotifyDamageApplied(DamageResult result);
    }
}
