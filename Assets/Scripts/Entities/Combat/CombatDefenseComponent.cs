using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Combat
{
    public sealed class CombatDefenseComponent : MonoBehaviour, IEntityComponent, IInitializable, IDisposable
    {
        private const float DEFAULT_GUARD_ANGLE = 120f;

        [Header("Guard")]
        [SerializeField, Range(0f, 360f)] private float guardAngle = DEFAULT_GUARD_ANGLE;
        [SerializeField, Min(0f)] private float guardBreakDurationSeconds = 1.5f;

        [Header("Poise")]
        [SerializeField, Min(0f)] private float maxPoise = 100f;
        [SerializeField, Min(0f)] private float poiseRecoveryPerSecond = 25f;
        [SerializeField, Min(0f)] private float poiseRecoveryDelaySeconds = 1f;

        [Header("Stance")]
        [SerializeField, Min(0f)] private float maxStance = 100f;
        [SerializeField, Min(0f)] private float stanceRecoveryPerSecond = 10f;

        [Header("Critical Opportunity")]
        [SerializeField, Min(0f)] private float criticalOpportunityDurationSeconds = 2f;

        private Entity _entity;
        private IHealthComponent _health;
        private float _currentPoise;
        private float _currentStance;
        private float _poiseRecoveryDelayRemaining;
        private float _guardBreakRemaining;
        private float _criticalOpportunityRemaining;
        private bool _isBlocking;
        private bool _isParryWindowActive;
        private bool _isHyperArmorActive;
        private float _hyperArmorPoiseBonus;
        private bool _canBeInterrupted = true;

        public event Action<MeleeHitResult> OnHitResolved;

        public bool IsBlocking => _isBlocking;
        public bool IsParryWindowActive => _isParryWindowActive;
        public bool IsGuardBroken => _guardBreakRemaining > 0f;
        public bool IsInHitReaction { get; private set; }
        public bool IsInCriticalState { get; private set; }
        public bool HasCriticalOpportunity { get; private set; }
        public bool IsParryStunned { get; private set; }
        public bool CanBeInterrupted => _canBeInterrupted;
        public float CurrentPoise => _currentPoise;
        public float CurrentStance => _currentStance;

        [Inject]
        public void Construct(Entity entity, IHealthComponent health)
        {
            _entity = entity;
            _health = health;
        }

        public void Initialize()
        {
            _currentPoise = maxPoise;
            _currentStance = maxStance;
            _entity.RegisterComponent(this);
        }

        public void Dispose()
        {
            _entity.UnRegisterComponent(this);
        }

        public void SetBlocking(bool isBlocking)
        {
            _isBlocking = isBlocking
                && !IsGuardBroken
                && !HasCriticalOpportunity
                && !IsParryStunned
                && !IsInCriticalState
                && !IsInHitReaction;
        }

        public void SetParryWindowActive(bool isActive)
        {
            _isParryWindowActive = isActive;
        }

        public void SetHyperArmor(
            bool isActive,
            float poiseBonus = 0f,
            bool canBeInterrupted = true)
        {
            _isHyperArmorActive = isActive;
            _hyperArmorPoiseBonus = isActive ? Mathf.Max(0f, poiseBonus) : 0f;
            _canBeInterrupted = isActive ? canBeInterrupted : true;
        }

        public void SetHitReaction(bool isActive)
        {
            IsInHitReaction = isActive;
            if (isActive)
            {
                _isBlocking = false;
            }
        }

        public void SetCriticalState(bool isActive)
        {
            IsInCriticalState = isActive;
            if (isActive)
            {
                _isBlocking = false;
            }
        }

        public void SetCriticalOpportunity(bool isActive)
        {
            HasCriticalOpportunity = isActive;
            _criticalOpportunityRemaining = isActive
                ? criticalOpportunityDurationSeconds
                : 0f;
            if (isActive)
            {
                _isBlocking = false;
            }
        }

        public void SetParryStunned(bool isStunned)
        {
            IsParryStunned = isStunned;
            if (isStunned)
            {
                _isBlocking = false;
            }
        }

        public void BeginGuardBreak()
        {
            _guardBreakRemaining = guardBreakDurationSeconds;
            _isBlocking = false;
            SetCriticalOpportunity(true);
        }

        public bool IsWithinGuardAngle(Vector3 attackerPosition)
        {
            Vector3 toAttacker = attackerPosition - transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude <= 0f)
            {
                return true;
            }

            return Vector3.Angle(transform.forward, toAttacker) <= guardAngle * 0.5f;
        }

        public bool ApplyStanceDamage(float damage)
        {
            if (damage <= 0f || maxStance <= 0f)
            {
                return false;
            }

            _currentStance = Mathf.Max(0f, _currentStance - damage);
            if (_currentStance > 0f)
            {
                return false;
            }

            SetCriticalOpportunity(true);
            return true;
        }

        public bool ApplyPoiseDamage(float damage)
        {
            if (damage <= 0f || maxPoise <= 0f || !_canBeInterrupted)
            {
                return false;
            }

            float effectivePoise = _currentPoise;
            if (_isHyperArmorActive)
            {
                effectivePoise += _hyperArmorPoiseBonus;
            }

            if (damage < effectivePoise)
            {
                _currentPoise = Mathf.Max(0f, _currentPoise - damage);
                _poiseRecoveryDelayRemaining = poiseRecoveryDelaySeconds;
                return false;
            }

            _currentPoise = maxPoise;
            _poiseRecoveryDelayRemaining = poiseRecoveryDelaySeconds;
            return true;
        }

        public void TickRecovery(float deltaTime)
        {
            if (deltaTime <= 0f || !_health.Stats.IsAlive)
            {
                return;
            }

            if (_guardBreakRemaining > 0f)
            {
                _guardBreakRemaining = Mathf.Max(0f, _guardBreakRemaining - deltaTime);
            }

            if (HasCriticalOpportunity && !IsInCriticalState)
            {
                _criticalOpportunityRemaining = Mathf.Max(
                    0f,
                    _criticalOpportunityRemaining - deltaTime);
                if (_criticalOpportunityRemaining <= 0f)
                {
                    ResetStance();
                }
            }

            if (_poiseRecoveryDelayRemaining > 0f)
            {
                _poiseRecoveryDelayRemaining = Mathf.Max(
                    0f,
                    _poiseRecoveryDelayRemaining - deltaTime);
                return;
            }

            if (poiseRecoveryPerSecond > 0f)
            {
                _currentPoise = Mathf.Min(
                    maxPoise,
                    _currentPoise + poiseRecoveryPerSecond * deltaTime);
            }

            if (stanceRecoveryPerSecond > 0f && !HasCriticalOpportunity)
            {
                _currentStance = Mathf.Min(
                    maxStance,
                    _currentStance + stanceRecoveryPerSecond * deltaTime);
            }
        }

        public void ResetStance()
        {
            _currentStance = maxStance;
            HasCriticalOpportunity = false;
            _criticalOpportunityRemaining = 0f;
            _guardBreakRemaining = 0f;
        }

        public void PublishHitResolved(in MeleeHitResult result)
        {
            OnHitResolved?.Invoke(result);
        }
    }
}
