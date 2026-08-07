using SoulsLike.Services.Repository;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Factory
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