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
        [SerializeField] private Renderer debugRenderer;
        public event Action OnHitConfirmed;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private readonly HashSet<long> _hitEntityIds = new();
        //todo: lot of duplicated data around 
        private IEntityLocator _entityLocator;
        private long _ownerEntityId;
        private ItemId _weaponId;
        private CharacterActionId _actionId;
        private float _damage;
        private Material _debugMaterialInstance;
        private Color _inactiveDebugColor;

        public void Initialize(
            IEntityLocator entityLocator,
            long ownerEntityId,
            ItemId weaponId)
        {
            hitbox.isTrigger = true;
            _entityLocator = entityLocator;
            _ownerEntityId = ownerEntityId;
            _weaponId = weaponId;
            if (debugRenderer != null)
            {
                _debugMaterialInstance = debugRenderer.material;
                _inactiveDebugColor = _debugMaterialInstance.GetColor(_baseColorId);
            }

            Close();
        }

        public void Open(CharacterActionId actionId, float damage)
        {
            _actionId = actionId;
            _damage = damage;
            _hitEntityIds.Clear();
            hitbox.enabled = true;
            SetDebugColor(Color.red);
        }

        public void Close()
        {
            hitbox.enabled = false;
            _hitEntityIds.Clear();
            SetDebugColor(_inactiveDebugColor);
        }

        private void SetDebugColor(Color color)
        {
            if (_debugMaterialInstance != null)
            {
                _debugMaterialInstance.SetColor(_baseColorId, color);
            }
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
                OnHitConfirmed?.Invoke();
            }
        }

        private void OnDisable()
        {
            _hitEntityIds.Clear();
        }

        private void OnDestroy()
        {
            if (_debugMaterialInstance != null)
            {
                Destroy(_debugMaterialInstance);
            }
        }
    }
}
