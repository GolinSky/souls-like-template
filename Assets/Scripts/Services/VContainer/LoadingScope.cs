using SoulsLike.Services;
using SoulsLike.Ui.Loading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public sealed class LoadingScope : LifetimeScope
    {
        [SerializeField] private UiService uiService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<UiFactory>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponent(uiService).As<IUiService>();
            builder.RegisterEntryPoint<LoadingUiController>().AsSelf();
        }
    }
}
