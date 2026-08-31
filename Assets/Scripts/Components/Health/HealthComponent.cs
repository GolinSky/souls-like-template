using System;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Health
{
    public class HealthComponent : BaseComponent<HealthModel>, IHealthComponent, IInitializable
    {
        private const float MAX_STAMINA_DEBT_MULTIPLIER = 1.0f;

        private float _staminaRecoveryDelayRemaining;
        private bool _staminaSpentSinceRecoveryTick;
        private bool _isInvulnerable;

        public HealthStats Stats => Model.Stats;
        public bool IsInvulnerable => _isInvulnerable;

        public void Initialize()
        {
            ApplyAuthoritativeStats(BuildDefaultStats());
        }

        public HealthStats BuildDefaultStats()
        {
            float maxHealth = Mathf.Max(1f, Model.MaxHealth);
            float currentHealth = Mathf.Clamp(Model.StartingHealth, 0f, maxHealth);
            float maxFocus = Mathf.Max(1f, Model.MaxFocus);
            float currentFocus = Mathf.Clamp(Model.StartingFocus, 0f, maxFocus);
            float maxStamina = Mathf.Max(1f, Model.MaxStamina);
            float currentStamina = Mathf.Clamp(Model.StartingStamina, 0f, maxStamina);

            return new HealthStats
            {
                CurrentHealth = currentHealth,
                MaxHealth = maxHealth,
                CurrentFocus = currentFocus,
                MaxFocus = maxFocus,
                CurrentStamina = currentStamina,
                MaxStamina = maxStamina,
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

            if (update.SetMaxFocus)
            {
                stats.MaxFocus = Mathf.Max(1f, update.MaxFocus);
                stats.CurrentFocus = Mathf.Clamp(stats.CurrentFocus, 0f, stats.MaxFocus);
            }

            if (update.SetCurrentFocus)
            {
                stats.CurrentFocus = Mathf.Clamp(update.CurrentFocus, 0f, stats.MaxFocus);
            }

            if (update.SetMaxStamina)
            {
                stats.MaxStamina = Mathf.Max(1f, update.MaxStamina);
                stats.CurrentStamina = ClampSignedStamina(stats.CurrentStamina, stats.MaxStamina);
            }

            if (update.SetCurrentStamina)
            {
                stats.CurrentStamina = ClampSignedStamina(update.CurrentStamina, stats.MaxStamina);
            }

            stats.IsAlive = stats.CurrentHealth > 0f;
            return NormalizeStats(stats);
        }

        public DamageResult CalculateDamage(DamageRequest request, HealthStats currentStats)
        {
            HealthStats stats = NormalizeStats(currentStats);
            float incomingAmount = Mathf.Max(0f, request.Amount);

            if (_isInvulnerable || !stats.IsAlive || incomingAmount <= 0f)
            {
                return new DamageResult
                {
                    SourceEntityId = request.SourceEntityId,
                    IncomingAmount = incomingAmount,
                    HealthDamageAmount = 0f,
                    NewStats = stats,
                    Killed = false
                };
            }

            float healthDamage = incomingAmount;
            float nextHealth = stats.CurrentHealth - healthDamage;

            if (!Model.CanDie)
            {
                nextHealth = Mathf.Max(1f, nextHealth);
            }

            stats.CurrentHealth = Mathf.Clamp(nextHealth, 0f, stats.MaxHealth);
            bool wasAlive = currentStats.IsAlive;
            stats.IsAlive = stats.CurrentHealth > 0f;

            return new DamageResult
            {
                SourceEntityId = request.SourceEntityId,
                IncomingAmount = incomingAmount,
                HealthDamageAmount = healthDamage,
                NewStats = stats,
                Killed = wasAlive && !stats.IsAlive
            };
        }

        public DamageResult ApplyDamage(in DamageRequest request)
        {
            DamageResult result = CalculateDamage(request, Stats);
            if (_isInvulnerable)
            {
                return result;
            }

            Model.SetDamageSource(request.SourceEntityId);
            ApplyAuthoritativeStats(result.NewStats);
            NotifyDamageApplied(result);
            return result;
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

        public void ConsumeFocus(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            stats.CurrentFocus = Mathf.Clamp(stats.CurrentFocus - amount, 0f, stats.MaxFocus);
            ApplyAuthoritativeStats(stats);
        }

        public void RestoreFocus(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            stats.CurrentFocus = Mathf.Clamp(stats.CurrentFocus + amount, 0f, stats.MaxFocus);
            ApplyAuthoritativeStats(stats);
        }

        public bool CanConsumeStamina(float amount, float startThreshold = 0f)
        {
            if (amount <= 0f) return true;

            HealthStats stats = Stats;
            float threshold = Mathf.Clamp(startThreshold, -stats.MaxStamina, stats.MaxStamina);
            return stats.CurrentStamina > threshold;
        }

        public bool TryConsumeStamina(float amount, float startThreshold = 0f)
        {
            if (!CanConsumeStamina(amount, startThreshold))
            {
                return false;
            }

            ConsumeStamina(amount);
            return true;
        }

        public void ConsumeStamina(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            stats.CurrentStamina = ClampSignedStamina(stats.CurrentStamina - amount, stats.MaxStamina);
            _staminaRecoveryDelayRemaining = Mathf.Max(
                _staminaRecoveryDelayRemaining,
                Model.StaminaRecoveryDelaySeconds);
            _staminaSpentSinceRecoveryTick = true;
            ApplyAuthoritativeStats(stats);
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            if (stats.CurrentStamina >= stats.MaxStamina) return;

            stats.CurrentStamina = Mathf.Min(stats.CurrentStamina + amount, stats.MaxStamina);
            ApplyAuthoritativeStats(stats);
        }

        public void TickStaminaRecovery(float deltaTime, bool isGuarding)
        {
            if (deltaTime <= 0f || !Stats.IsAlive || Model.StaminaRecoveryPerSecond <= 0f)
            {
                return;
            }

            if (_staminaSpentSinceRecoveryTick)
            {
                _staminaSpentSinceRecoveryTick = false;
                return;
            }

            float recoveryDeltaTime = deltaTime;
            if (_staminaRecoveryDelayRemaining > 0f)
            {
                if (recoveryDeltaTime <= _staminaRecoveryDelayRemaining)
                {
                    _staminaRecoveryDelayRemaining -= recoveryDeltaTime;
                    return;
                }

                recoveryDeltaTime -= _staminaRecoveryDelayRemaining;
                _staminaRecoveryDelayRemaining = 0f;
            }

            float recoveryMultiplier = isGuarding
                ? Model.GuardStaminaRecoveryMultiplier
                : 1f;
            RestoreStamina(Model.StaminaRecoveryPerSecond * recoveryMultiplier * recoveryDeltaTime);
        }

        public void ApplyAuthoritativeStats(HealthStats stats)
        {
            HealthStats normalizedStats = NormalizeStats(stats);
            bool died = Stats.IsAlive && !normalizedStats.IsAlive;
            Model.ApplyStats(normalizedStats);

            if (died)
            {
                Model.NotifyDeath();
            }
        }

        public void NotifyDamageApplied(DamageResult result)
        {
            Model.NotifyDamageApplied(result);
        }

        public void SetInvulnerable(bool isInvulnerable)
        {
            _isInvulnerable = isInvulnerable;
        }

        private HealthStats NormalizeStats(HealthStats stats)
        {
            stats.MaxHealth = Mathf.Max(1f, stats.MaxHealth);
            stats.CurrentHealth = Mathf.Clamp(stats.CurrentHealth, 0f, stats.MaxHealth);
            stats.MaxFocus = Mathf.Max(1f, stats.MaxFocus);
            stats.CurrentFocus = Mathf.Clamp(stats.CurrentFocus, 0f, stats.MaxFocus);
            stats.MaxStamina = Mathf.Max(1f, stats.MaxStamina);
            stats.CurrentStamina = ClampSignedStamina(stats.CurrentStamina, stats.MaxStamina);
            stats.IsAlive = stats.CurrentHealth > 0f;
            return stats;
        }

        private static float ClampSignedStamina(float currentStamina, float maxStamina)
        {
            float maxDebt = maxStamina * MAX_STAMINA_DEBT_MULTIPLIER;
            return Mathf.Clamp(currentStamina, -maxDebt, maxStamina);
        }
    }
}
