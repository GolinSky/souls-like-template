using System;

namespace SoulsLike.Ui.PauseNavigation
{
    public interface IPauseNavigationRoute
    {
        event Action CloseRequested;

        void Show();
        void Hide();
    }
}
