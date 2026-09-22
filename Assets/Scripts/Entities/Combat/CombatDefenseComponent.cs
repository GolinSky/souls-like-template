using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Combat
{
    //todo: why it is not component inherited - only naming is right
    public sealed class CombatDefenseComponent : MonoBehaviour, IEntityComponent, IInitializable, IDisposable
    {
        //todo: move to data if can be tuned.
        private const float DEFAULT_GUARD_ANGLE = 120f;

        public event Action<MeleeHitResult> OnHitResolved;
        
        //todo: all serialized field must be in Data 
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
        
        //todo: why don't use Getters with private set to reduce space of states code in this class
        private float _currentPoise;
        private float _currentStance;
        private float _poiseRecoveryDelayRemaining;//todo: replace with itimer 
        private float _guardBreakRemaining;//todo: replace with itimer 
        private float _criticalOpportunityRemaining;//todo: replace with itimer 
        private bool _isBlocking;
        private bool _isParryWindowActive;
        private bool _isHyperArmorActive;
        private float _hyperArmorPoiseBonus;
        private bool _canBeInterrupted = true;

        
        public bool IsBlocking => _isBlocking;
        public bool IsParryWindowActive => _isParryWindowActive;
        public bool IsGuardBroken => _guardBreakRemaining > 0f;
        public bool IsInHitReaction { get; private set; }
        public bool IsInCriticalState { get; private set; }
        public bool HasCriticalOpportunity { get; private set; }
        public bool IsParryStunned { get; private set; }
        public float CurrentPoise => _currentPoise;

        [Inject]
        public void Construct(Entity entity, IHealthComponent health)//todo: direct access for IHealthComponent .not modular.. investigate if axis of change is the same 
        {
            _entity = entity;
            _health = health;
        }

        public void Initialize()
        {
            _currentPoise = maxPoise;
            _currentStance = maxStance;
            _entity.RegisterComponent(this);//todo: can we inject CombatDefenseComponent while building binding in vcontainer layer 
                //todo: this gives more modularity for reusing this component for test
        }

        public void Dispose()
        {
            _entity.UnRegisterComponent(this);
        }

        //todo: rename - follow crying architecture rule 
        public void SetBlocking(bool isBlocking)
        {
            //todo:revisit this not readable condition
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
            _canBeInterrupted = !isActive || canBeInterrupted;
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

            return Vector3.Angle(transform.forward, toAttacker) <= guardAngle * 0.5f;//todo: magic number. made local const
        }

    
        public bool TryApplyStanceDamage(float damage)
        {
            AssertDamage(damage);
            if (maxStance <= 0f) 
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
            AssertDamage(damage);

            if (maxPoise <= 0f || !_canBeInterrupted)
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
            //todo: deltaTime - what must happen that delta time can be <=0. delta time can be 0 if timescale == 0. in this case make it more explicit
            //todo: otherwise if deltaTime can be <=0 - make assertation . don't make noise in core logic condition 
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

            if (stanceRecoveryPerSecond > 0f && !HasCriticalOpportunity)
            {
                _currentStance = Mathf.Min(
                    maxStance,
                    _currentStance + stanceRecoveryPerSecond * deltaTime);
            }

            if (_poiseRecoveryDelayRemaining > 0f)
            {
                _poiseRecoveryDelayRemaining = Mathf.Max(0f, _poiseRecoveryDelayRemaining - deltaTime);
                return;
            }

            if (poiseRecoveryPerSecond > 0f)
            {
                _currentPoise = Mathf.Min(
                    maxPoise,
                    _currentPoise + poiseRecoveryPerSecond * deltaTime);
            }
        }

        public void ResetStance()
        {
            _currentStance = maxStance;
            HasCriticalOpportunity = false;
            _criticalOpportunityRemaining = 0f;
            _guardBreakRemaining = 0f;
        }

        //todo: strange logic here - why this class contains this event action unless it hides ResolveMeleeHitCommand
        public void PublishHitResolved(in MeleeHitResult result)
        {
            OnHitResolved?.Invoke(result);
        }
        
        private void AssertDamage(float damage)
        {
            Debug.Assert(damage <= 0, $"damage({damage}) can not equals 0 or below 0");
            //todo: decide throw exception or just use assertation - Fast Fall 
            //todo: remove this assert - rework damage - dmg is 0 when we have iframes - it is not clear here
        }
    }
}
