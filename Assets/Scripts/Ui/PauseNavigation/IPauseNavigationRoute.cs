using System;
using SoulsLike.Ui.Navigation;

namespace SoulsLike.Ui.PauseNavigation
{
    public interface IPauseNavigationRoute : IUiRoute
    {
        event Action CloseRequested;
    }
}
