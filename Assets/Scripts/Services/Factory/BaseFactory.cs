using MultiPlayerTemplate.Services.Repository;
using VContainer;
using VContainer.Unity;

namespace MultiPlayerTemplate.Factory
{
    public abstract class BaseFactory
    {
        protected IAssetService AssetService => RootScope.Container.Resolve<IAssetService>();
        protected IObjectResolver Resolver { get; }

        protected LifetimeScope RootScope { get; }

        protected BaseFactory(IObjectResolver resolver)
        {
            RootScope = resolver.Resolve<LifetimeScope>();
            Resolver = resolver;
        }
    }
}