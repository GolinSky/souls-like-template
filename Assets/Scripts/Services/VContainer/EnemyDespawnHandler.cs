using SoulsLike.Entities.Enemy;

namespace SoulsLike.Services.VContainer
{
    public sealed class EnemyDespawnHandler : IEnemyDespawnHandler
    {
        private readonly EntityLifetimeScope _scope;

        public EnemyDespawnHandler(EntityLifetimeScope scope)
        {
            _scope = scope;
        }

        public void Despawn()
        {
            _scope.Dispose();
        }
    }
}
