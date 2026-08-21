using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class TargetingCommand : EntityCommand
    {
        private readonly Entity _entity;
        private readonly ViewEntity _viewEntity;
        private readonly TargetLockNode _lockNode;
        private readonly IHealthComponent _health;

        public Transform TargetTransform => _lockNode.TargetTransform;

        public TargetingCommand(Entity entity, ViewEntity viewEntity, TargetLockNode lockNode, IHealthComponent health)
            : base(entity)
        {
            _entity = entity;
            _viewEntity = viewEntity;
            _lockNode = lockNode;
            _health = health;
        }

        public TargetingSnapshot Read()
        {
            Transform root = _viewEntity.transform;
            return new TargetingSnapshot(
                _entity.Id,
                _entity.EntityType,
                root.position,
                root.forward,
                _lockNode.TargetTransform.position,
                _health.Stats.IsAlive);
        }
    }
}
