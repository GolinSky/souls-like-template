using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class MeleeHitboxController : MonoBehaviour
    {
        [SerializeField] private Collider hitbox;
        [SerializeField] private int hitZone;
        [SerializeField] private Renderer debugRenderer;
        public event Action<MeleeHitResult> OnHitResolved;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private readonly HashSet<long> _hitEntityIds = new();
        //todo: lot of duplicated data around 
        private IEntityLocator _entityLocator;
        private long _ownerEntityId;
        private ItemId _weaponId;
        private MeleeAttackData _attack;
        private int _attackInstanceId;
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

        public void Open(in MeleeAttackData attack)
        {
            _attackInstanceId++;
            _attack = attack;
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
                || _hitEntityIds.Contains(target.Id))
            {
                return;
            }

            if (!target.TryGetComponent(out ResolveMeleeHitCommand resolveMeleeHit))
            {
                throw new InvalidOperationException(
                    $"Entity {target.Id} ({target.EntityType}) is missing "
                    + $"{nameof(ResolveMeleeHitCommand)}.");
            }

            if (!_entityLocator.TryGetEntity(_ownerEntityId, out IEntity owner))
            {
                Close();
                return;
            }

            if (!owner.TryGetComponent(out TargetingCommand targeting))
            {
                throw new InvalidOperationException(
                    $"Entity {owner.Id} ({owner.EntityType}) is missing "
                    + $"{nameof(TargetingCommand)}.");
            }

            MeleeHitRequest request = new(
                _ownerEntityId,
                _weaponId,
                _attackInstanceId,
                targeting.Read().Position,
                other.ClosestPoint(transform.position),
                hitZone,
                _attack);
            MeleeHitResult result = resolveMeleeHit.Execute(request);
            if (result.Type != MeleeHitResultType.Ignored)
            {
                _hitEntityIds.Add(target.Id);
                if (result.Type == MeleeHitResultType.Parried)
                {
                    Close();
                }

                OnHitResolved?.Invoke(result);
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
