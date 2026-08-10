using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.BaseEntity
{
    public static class EntityRegistrationExt
    {
        public static void RegisterEntitySystemExt(this IContainerBuilder builder, EntityType entityType, long id)
        {
            builder.RegisterInstance(entityType);
            builder.RegisterInstance(id);
            builder.Register<Entity>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
