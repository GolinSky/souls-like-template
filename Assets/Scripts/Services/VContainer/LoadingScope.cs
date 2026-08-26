using SoulsLike.Ui.Loading;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public sealed class LoadingScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LoadingUiController>().AsSelf();
        }
    }
}
