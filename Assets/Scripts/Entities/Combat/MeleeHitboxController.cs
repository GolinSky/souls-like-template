using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class MeleeHitboxController : MonoBehaviour
    {
        [SerializeField] private Collider hitbox;
        [SerializeField] private int hitZone;

        private readonly HashSet<long> _hitEntityIds = new();
        private IEntityLocator _entityLocator;
        private long _ownerEntityId;
        private ItemId _weaponId;
        private CharacterActionId _actionId;
        private float _damage;

        public void Initialize(
            IEntityLocator entityLocator,
            long ownerEntityId,
            ItemId weaponId)
        {
            _entityLocator = entityLocator;
            _ownerEntityId = ownerEntityId;
            _weaponId = weaponId;
            Close();
        }

        public void Open(CharacterActionId actionId, float damage)
        {
            _actionId = actionId;
            _damage = damage;
            _hitEntityIds.Clear();
            hitbox.enabled = true;
        }

        public void Close()
        {
            hitbox.enabled = false;
            _hitEntityIds.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_entityLocator.TryGetEntity(other, out IEntity target)
                || target.Id == _ownerEntityId
                || _hitEntityIds.Contains(target.Id))
            {
                return;
            }

            if (!_entityLocator.TryGetEntity(_ownerEntityId, out IEntity owner))
            {
                Close();
                return;
            }

            if (target.EntityType == owner.EntityType)
            {
                return;
            }

            if (!target.TryGetComponent(out ApplyDamageCommand applyDamage))
            {
                throw new InvalidOperationException(
                    $"Entity {target.Id} ({target.EntityType}) is missing "
                    + $"{nameof(ApplyDamageCommand)}.");
            }

            DamageRequest damage = new DamageRequest
            {
                Amount = _damage,
                HitPoint = other.ClosestPoint(transform.position),
                HitZone = hitZone
            };
            DamageResult result = applyDamage.Execute(new ApplyDamageRequest(
                _ownerEntityId,
                _weaponId,
                _actionId,
                damage));
            if (result.HealthDamageAmount > 0f)
            {
                _hitEntityIds.Add(target.Id);
            }
        }

        private void OnDisable()
        {
            _hitEntityIds.Clear();
        }
    }
}
