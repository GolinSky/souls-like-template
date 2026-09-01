using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Combat
{
    public sealed class CriticalAttackController : MonoBehaviour, IInitializable
    {
        [SerializeField, Range(0f, 180f)] private float rearHalfAngleDegrees = 45f;
        [SerializeField, Min(0f)] private float maxCriticalDistance = 1.5f;
        [SerializeField, Min(0f)] private float maxHeightDifference = 0.5f;
        [SerializeField, Min(0f)] private float requiredNeutralSeconds = 0.1f;
        [SerializeField] private Vector3 attackerLocalOffset = new(0f, 0f, -0.9f);
        [SerializeField] private float attackerYawOffsetDegrees;
        [SerializeField] private float victimYawOffsetDegrees;

        private readonly List<IEntity> _enemyEntities = new();
        private Entity _entity;
        private IEntityLocator _entityLocator;
        private IHealthComponent _health;
        private AttackComponent _attack;
        private WeaponDatabase _weaponDatabase;
        private CombatDefenseComponent _defense;
        private MovementComponent _movement;
        private AnimatorComponent _animator;
        private PlayerMeleeCombatRelay _meleeCombatRelay;
        private float _neutralSince;
        private bool _wasNeutral;
        private CriticalTargetCommand _activeTarget;
        private DamageRequest _pendingDamage;
        private DamageResult _cachedPreview;
        private bool _isRunning;
        private bool _impactApplied;

        public bool IsRunning => _isRunning;
        public event Action OnCompleted;

        [Inject]
        public void Construct(
            Entity entity,
            IEntityLocator entityLocator,
            IHealthComponent health,
            AttackComponent attack,
            WeaponDatabase weaponDatabase,
            CombatDefenseComponent defense,
            MovementComponent movement,
            AnimatorComponent animator,
            PlayerMeleeCombatRelay meleeCombatRelay)
        {
            _entity = entity;
            _entityLocator = entityLocator;
            _health = health;
            _attack = attack;
            _weaponDatabase = weaponDatabase;
            _defense = defense;
            _movement = movement;
            _animator = animator;
            _meleeCombatRelay = meleeCombatRelay;
        }

        public void Initialize()
        {
            _neutralSince = Time.time;
        }

        public void UpdateNeutralEligibility(bool isNeutral)
        {
            if (isNeutral && !_wasNeutral)
            {
                _neutralSince = Time.time;
            }

            _wasNeutral = isNeutral;
        }

        public bool TryStart()
        {
            if (_isRunning
                || !_health.Stats.IsAlive
                || !_attack.RightWeaponId.HasValue
                || _defense.IsInHitReaction
                || _defense.IsParryStunned
                || _defense.IsInCriticalState)
            {
                return false;
            }

            CriticalTargetCommand target = FindTarget(out bool isRiposte);
            if (target == null || !IsCommonlyEligible(target))
            {
                return false;
            }

            ItemId weaponId = _attack.RightWeaponId.Value;
            WeaponDefinition weapon = _weaponDatabase.GetRequired(weaponId);
            _pendingDamage = new DamageRequest
            {
                SourceEntityId = _entity.Id,
                Amount = weapon.Stats.PhysicalAttack * weapon.Stats.Critical / 100f,
                HitPoint = target.Position,
                HitZone = 0
            };
            _cachedPreview = target.PreviewDamage(_pendingDamage);

            if (!IsCommonlyEligible(target)
                || (isRiposte
                    ? !IsRiposteEligible(target)
                    : !IsBackstabEligible(target)))
            {
                return false;
            }

            _isRunning = true;
            _impactApplied = false;
            _activeTarget = target;
            _defense.SetCriticalState(true);
            _meleeCombatRelay.Cancel();

            HandMode handMode = _attack.ActiveHandMode;
            AlignActors(target, isRiposte);
            target.BeginCritical(handMode, _cachedPreview.Killed);
            _animator.PlayCriticalAttack(handMode);
            return true;
        }

        public void ApplyCachedDamage()
        {
            if (!_isRunning || _impactApplied)
            {
                return;
            }

            _impactApplied = true;
            _activeTarget.ApplyDamage(_pendingDamage);
        }

        public void Complete()
        {
            if (!_isRunning)
            {
                return;
            }

            _activeTarget.EndCritical();
            _defense.SetCriticalState(false);
            _activeTarget = null;
            _isRunning = false;
            OnCompleted?.Invoke();
        }

        private CriticalTargetCommand FindTarget(out bool isRiposte)
        {
            _enemyEntities.Clear();
            _entityLocator.GetEntities(EntityType.Enemy, _enemyEntities);

            CriticalTargetCommand riposte = FindNearest(true);
            isRiposte = riposte != null;
            return riposte ?? FindNearest(false);
        }

        private CriticalTargetCommand FindNearest(bool riposte)
        {
            CriticalTargetCommand closest = null;
            float closestDistance = float.MaxValue;
            long closestId = long.MaxValue;
            foreach (IEntity enemy in _enemyEntities)
            {
                if (!enemy.TryGetComponent(out CriticalTargetCommand target)
                    || !IsCommonlyEligible(target)
                    || (riposte ? !IsRiposteEligible(target) : !IsBackstabEligible(target)))
                {
                    continue;
                }

                float distance = HorizontalDistance(target.Position);
                if (distance < closestDistance
                    || Mathf.Approximately(distance, closestDistance) && enemy.Id < closestId)
                {
                    closest = target;
                    closestDistance = distance;
                    closestId = enemy.Id;
                }
            }

            return closest;
        }

        private bool IsCommonlyEligible(CriticalTargetCommand target)
        {
            if (!target.IsAlive || target.IsInvulnerable || target.IsInCriticalState)
            {
                return false;
            }

            return HorizontalDistance(target.Position) <= maxCriticalDistance
                && Mathf.Abs(_movement.transform.position.y - target.Position.y)
                <= maxHeightDifference;
        }

        private bool IsRiposteEligible(CriticalTargetCommand target) =>
            target.IsRiposteEligible;

        private bool IsBackstabEligible(CriticalTargetCommand target)
        {
            if (Time.time - _neutralSince < requiredNeutralSeconds
                || !target.IsBackstabEligible)
            {
                return false;
            }

            Vector3 targetToAttacker = _movement.transform.position - target.Position;
            targetToAttacker.y = 0f;
            return targetToAttacker.sqrMagnitude > 0f
                && Vector3.Angle(-target.Forward, targetToAttacker)
                <= rearHalfAngleDegrees;
        }

        private void AlignActors(CriticalTargetCommand target, bool isRiposte)
        {
            float targetYaw = target.ActorTransform.eulerAngles.y + victimYawOffsetDegrees;
            target.ActorTransform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
            Vector3 attackerOffset = attackerLocalOffset;
            attackerOffset.z = isRiposte ? -attackerOffset.z : attackerOffset.z;
            _movement.SetPosition(target.ActorTransform.TransformPoint(attackerOffset));
            _movement.transform.rotation = Quaternion.Euler(
                0f,
                targetYaw + attackerYawOffsetDegrees + (isRiposte ? 180f : 0f),
                0f);
        }

        private float HorizontalDistance(Vector3 position)
        {
            Vector3 delta = _movement.transform.position - position;
            delta.y = 0f;
            return delta.magnitude;
        }
    }
}
