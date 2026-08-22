using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Items;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Combat
{
    public sealed class PlayerMeleeCombatRelay : MonoBehaviour
    {
        private IEntityLocator _entityLocator;
        private Entity _entity;
        private AttackComponent _attackComponent;
        private CharacterAudioComponent _audioComponent;
        private WeaponDatabase _weaponDatabase;
        private MeleeHitboxController _activeHitbox;
        private CharacterActionId _actionId;
        private float _damage;
        private int _attackSequence;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            Entity entity,
            AttackComponent attackComponent,
            CharacterAudioComponent audioComponent,
            WeaponDatabase weaponDatabase)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _attackComponent = attackComponent;
            _audioComponent = audioComponent;
            _weaponDatabase = weaponDatabase;
        }

        public int Begin(CharacterActionId actionId)
        {
            if (_activeHitbox != null)
            {
                _activeHitbox.OnHitConfirmed -= OnHitConfirmed;
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
            _actionId = actionId;
            _damage = _weaponDatabase
                .GetRequired(weaponId)
                .Stats
                .PhysicalAttack * multiplier;
            _activeHitbox.Initialize(_entityLocator, _entity.Id, weaponId);
            _activeHitbox.OnHitConfirmed += OnHitConfirmed;
            return _attackSequence;
        }

        public void Open(int attackSequence)
        {
            if (attackSequence != _attackSequence)
            {
                Debug.LogError($"attackSequence: {attackSequence} same value error");
                return;
            }

            _activeHitbox.Open(_actionId, _damage);
        }

        public void Close(int attackSequence)
        {
            if (attackSequence != _attackSequence)
            {
                return;
            }

            if (_activeHitbox != null)
            {
                _activeHitbox.OnHitConfirmed -= OnHitConfirmed;
                _activeHitbox.Close();
                _activeHitbox = null;
            }
        }

        private void OnHitConfirmed() => _audioComponent.NotifySwordClash();
    }
}
