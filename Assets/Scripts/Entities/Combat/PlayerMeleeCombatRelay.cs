using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Items;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace SoulsLike.Entities.Combat
{
    public sealed class PlayerMeleeCombatRelay : MonoBehaviour
    {
        private IEntityLocator _entityLocator;
        private Entity _entity;
        private AttackComponent _attackComponent;
        private AnimatorComponent _animatorComponent;
        private CharacterAudioComponent _audioComponent;
        private WeaponDatabase _weaponDatabase;
        private MeleeHitboxController _activeHitbox;
        private AudioResource _attackSfx;
        private MeleeAttackData _attack;
        private int _attackSequence;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            Entity entity,
            AttackComponent attackComponent,
            AnimatorComponent animatorComponent,
            CharacterAudioComponent audioComponent,
            WeaponDatabase weaponDatabase)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _attackComponent = attackComponent;
            _animatorComponent = animatorComponent;
            _audioComponent = audioComponent;
            _weaponDatabase = weaponDatabase;
        }

        public int Begin(CharacterActionId actionId)
        {
            if (_activeHitbox != null)
            {
                _activeHitbox.OnHitResolved -= OnHitResolved;
            }

            _attackSequence++;

            ItemId weaponId = _attackComponent.ActiveWeaponId
                ?? throw new InvalidOperationException(
                    "A melee attack requires an active weapon item ID.");
            WeaponRuntime weaponRuntime = _attackComponent.ActiveWeaponRuntime;
            if (weaponRuntime == null)
            {
                throw new InvalidOperationException(
                    $"Weapon '{weaponId}' requires {nameof(WeaponRuntime)}.");
            }

            _activeHitbox = weaponRuntime.MeleeHitbox;
            if (_activeHitbox == null)
            {
                throw new InvalidOperationException(
                    $"Weapon '{weaponId}' runtime requires a serialized " +
                    $"{nameof(MeleeHitboxController)} reference.");
            }

            CombatProfile combatProfile = _attackComponent.ActiveCombatProfile;
            float multiplier = actionId == CharacterActionId.HeavyAttack
                ? combatProfile.HeavyAttackMultiplier
                : combatProfile.LightAttackMultiplier;
            WeaponDefinition weaponDefinition = _weaponDatabase.GetRequired(weaponId);
            _attackSfx = weaponDefinition.AttackSfx;
            _attack = BuildAttack(actionId, weaponDefinition, combatProfile, multiplier);
            _activeHitbox.Initialize(_entityLocator, _entity.Id, weaponId);
            _activeHitbox.OnHitResolved += OnHitResolved;
            return _attackSequence;
        }

        public void Open(int attackSequence)
        {
            if (attackSequence != _attackSequence)
            {
                Debug.LogError($"attackSequence: {attackSequence} same value error");
                return;
            }

            _activeHitbox.Open(_attack);
        }

        public void PlayAttackSfx(int attackSequence)
        {
            if (attackSequence != _attackSequence)
            {
                return;
            }

            _audioComponent.NotifyAttack(_attackSfx);
        }

        public void Close(int attackSequence)
        {
            if (attackSequence != _attackSequence)
            {
                return;
            }

            if (_activeHitbox != null)
            {
                _activeHitbox.OnHitResolved -= OnHitResolved;
                _activeHitbox.Close();
                _activeHitbox = null;
            }
        }

        public void Cancel()
        {
            if (_activeHitbox != null)
            {
                _activeHitbox.Close();
            }
        }

        private void OnHitResolved(MeleeHitResult result)
        {
            if (result.Type is MeleeHitResultType.Blocked
                or MeleeHitResultType.GuardBroken
                or MeleeHitResultType.Parried)
            {
                _audioComponent.NotifySwordClash();
            }

            if (result.Type == MeleeHitResultType.Parried)
            {
                Cancel();
                _animatorComponent.TriggerParried();
            }
        }

        private static MeleeAttackData BuildAttack(
            CharacterActionId actionId,
            WeaponDefinition weaponDefinition,
            CombatProfile combatProfile,
            float damageMultiplier)
        {
            bool isHeavy = actionId == CharacterActionId.HeavyAttack;
            return new MeleeAttackData
            {
                ActionId = actionId,
                HealthDamage = weaponDefinition.Stats.PhysicalAttack * damageMultiplier,
                GuardDamage = isHeavy
                    ? combatProfile.HeavyGuardDamage
                    : combatProfile.LightGuardDamage,
                PoiseDamage = isHeavy
                    ? combatProfile.HeavyPoiseDamage
                    : combatProfile.LightPoiseDamage,
                StanceDamage = isHeavy
                    ? combatProfile.HeavyStanceDamage
                    : combatProfile.LightStanceDamage,
                ImpactLevel = isHeavy
                    ? combatProfile.HeavyImpactLevel
                    : combatProfile.LightImpactLevel,
                CanBeBlocked = isHeavy
                    ? combatProfile.HeavyCanBeBlocked
                    : combatProfile.LightCanBeBlocked,
                CanBeParried = isHeavy
                    ? combatProfile.HeavyCanBeParried
                    : combatProfile.LightCanBeParried
            };
        }
    }
}
