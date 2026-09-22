using VContainer;
using VContainer.Unity;
using SoulsLike.Services.IdGeneration;

namespace SoulsLike.Entities.BaseEntity
{
    public static class EntityRegistrationExt
    {
        public static void RegisterEntitySystemExt(this IContainerBuilder builder, EntityType entityType)
        {
            builder.RegisterInstance(entityType);
            builder.Register<long>(
                resolver => resolver.Resolve<IUniqueIdGenerator>().GenerateUniqueId(),
                Lifetime.Scoped);
            builder.Register<Entity>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
