using System;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;

namespace SoulsLike.Entities.Elevator
{
    public sealed class ElevatorShaftHazard : MonoBehaviour
    {
        private IEntityLocator _entityLocator;

        public void Construct(IEntityLocator entityLocator) => _entityLocator = entityLocator;

        public void DisposeEntity() => _entityLocator = null;

        private void OnTriggerEnter(Collider other)
        {
            ApplyFatalDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            ApplyFatalDamage(other);
        }

        private void ApplyFatalDamage(Collider other)
        {//todo:check health command - ability to die - instead of hardcoding repeating EntityType equals
            if (_entityLocator == null
                || !_entityLocator.TryGetEntity(other, out IEntity entity)
                || entity.EntityType != EntityType.Player && entity.EntityType != EntityType.Enemy) 
            {
                return;
            }

            if (!entity.TryGetComponent(out ApplyDamageCommand command))
            {
                throw new InvalidOperationException(
                    $"Shaft victim {entity.Id} ({entity.EntityType}) is missing {nameof(ApplyDamageCommand)}.");
            }

            DamageRequest request = new()
            {
                SourceEntityId = entity.Id,
                Amount = command.Stats.CurrentHealth,
                HitPoint = other.ClosestPoint(transform.position)
            };
            command.ExecuteDirect(in request);
        }
    }
}
