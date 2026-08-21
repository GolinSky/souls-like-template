using SoulsLike.Entities.Character.Components.Health;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class ApplyDamageCommand : EntityCommand
    {
        private readonly Entity _target;
        private readonly IEntityLocator _entityLocator;
        private readonly IHealthComponent _health;

        public ApplyDamageCommand(Entity target, IEntityLocator entityLocator, IHealthComponent health)
            : base(target)
        {
            _target = target;
            _entityLocator = entityLocator;
            _health = health;
        }

        public DamageResult Execute(in ApplyDamageRequest request)
        {
            if (!_entityLocator.TryGetEntity(request.SourceEntityId, out IEntity source)
                || source.Id == _target.Id
                || source.EntityType == _target.EntityType)
            {
                return new DamageResult { SourceEntityId = request.SourceEntityId, NewStats = _health.Stats };
            }

            DamageRequest damage = request.Damage;
            damage.SourceEntityId = request.SourceEntityId;
            return _health.ApplyDamage(in damage);
        }
    }
}
