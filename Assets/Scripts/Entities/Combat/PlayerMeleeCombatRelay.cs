using System;
using SoulsLike.Entities.BaseEntity;
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
        private WeaponDatabase _weaponDatabase;
        private MeleeHitboxController _activeHitbox;
        private CharacterActionId _actionId;
        private float _damage;

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            Entity entity,
            AttackComponent attackComponent,
            WeaponDatabase weaponDatabase)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _attackComponent = attackComponent;
            _weaponDatabase = weaponDatabase;
        }

        public void Begin(CharacterActionId actionId)
        {
            ItemId weaponId = _attackComponent.ActiveWeaponId
                ?? throw new InvalidOperationException(
                    "A melee attack requires an active weapon item ID.");
            WeaponRuntime weaponRuntime = _attackComponent.ActiveWeaponRuntime;
            if (weaponRuntime == null
                || !weaponRuntime.TryGetComponent(out _activeHitbox))
            {
                throw new InvalidOperationException(
                    $"Weapon '{weaponId}' requires {nameof(MeleeHitboxController)}.");
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
        }

        public void Open()
        {
            _activeHitbox.Open(_actionId, _damage);
        }

        public void Close()
        {
            if (_activeHitbox != null)
            {
                _activeHitbox.Close();
            }
        }
    }
}
