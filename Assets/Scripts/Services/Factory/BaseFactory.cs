using SoulsLike.Services.Repository;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Factory
{
    public abstract class BaseFactory
    {
        protected IAssetService AssetService { get; }
        protected IObjectResolver Resolver { get; }

        protected LifetimeScope RootScope { get; }

        protected BaseFactory(IObjectResolver resolver, IAssetService assetService)
        {
            RootScope = resolver.Resolve<LifetimeScope>();
            Resolver = resolver;
            AssetService = assetService;
            
            
        }
    }
}