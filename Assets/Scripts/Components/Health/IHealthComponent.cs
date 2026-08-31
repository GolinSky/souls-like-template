namespace SoulsLike.Entities.Character.Components.Health
{
    public interface IHealthComponent
    {
        HealthStats Stats { get; }
        bool IsInvulnerable { get; }

        HealthStats BuildDefaultStats();
        HealthStats ApplyStatUpdate(HealthStats currentStats, HealthStatUpdate update);
        DamageResult CalculateDamage(DamageRequest request, HealthStats currentStats);
        DamageResult ApplyDamage(in DamageRequest request);
        HealthStats CalculateHeal(HealthStats currentStats, float amount);
        HealthStats CalculateRevive(HealthStats currentStats, float health);
        void ConsumeFocus(float amount);
        void RestoreFocus(float amount);
        bool CanConsumeStamina(float amount, float startThreshold = 0f);
        bool TryConsumeStamina(float amount, float startThreshold = 0f);
        void ConsumeStamina(float amount);
        void RestoreStamina(float amount);
        void TickStaminaRecovery(float deltaTime, bool isGuarding);
        void ApplyAuthoritativeStats(HealthStats stats);
        void NotifyDamageApplied(DamageResult result);
        void SetInvulnerable(bool isInvulnerable);
    }
}
