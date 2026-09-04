using SoulsLike.Orchestrators.MainMenu;
using SoulsLike.Ui.MainMenu;
using SoulsLike.Ui.Settings;
using VContainer;
using VContainer.Unity;

namespace SoulsLike
{
    public class MainMenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MainMenuUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SettingsUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterEntryPoint<MainMenuOrchestrator>();
        }
    }
}
