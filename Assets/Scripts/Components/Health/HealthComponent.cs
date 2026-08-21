using System;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Health
{
    public class HealthComponent : BaseComponent<HealthModel>, IHealthComponent, IInitializable
    {
        public HealthStats Stats => Model.Stats;

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
                stats.CurrentStamina = Mathf.Clamp(stats.CurrentStamina, 0f, stats.MaxStamina);
            }

            if (update.SetCurrentStamina)
            {
                stats.CurrentStamina = Mathf.Clamp(update.CurrentStamina, 0f, stats.MaxStamina);
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

        public void ConsumeStamina(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            stats.CurrentStamina = Mathf.Clamp(stats.CurrentStamina - amount, 0f, stats.MaxStamina);
            ApplyAuthoritativeStats(stats);
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f) return;
            HealthStats stats = Stats;
            stats.CurrentStamina = Mathf.Clamp(stats.CurrentStamina + amount, 0f, stats.MaxStamina);
            ApplyAuthoritativeStats(stats);
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

        private HealthStats NormalizeStats(HealthStats stats)
        {
            stats.MaxHealth = Mathf.Max(1f, stats.MaxHealth);
            stats.CurrentHealth = Mathf.Clamp(stats.CurrentHealth, 0f, stats.MaxHealth);
            stats.MaxFocus = Mathf.Max(1f, stats.MaxFocus);
            stats.CurrentFocus = Mathf.Clamp(stats.CurrentFocus, 0f, stats.MaxFocus);
            stats.MaxStamina = Mathf.Max(1f, stats.MaxStamina);
            stats.CurrentStamina = Mathf.Clamp(stats.CurrentStamina, 0f, stats.MaxStamina);
            stats.IsAlive = stats.CurrentHealth > 0f;
            return stats;
        }
    }
}
