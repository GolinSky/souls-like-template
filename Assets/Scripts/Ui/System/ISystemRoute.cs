using System;
using SoulsLike.Ui.PauseNavigation;

namespace SoulsLike
{
    public interface ISystemRoute : IPauseNavigationRoute
    {
        event Action ResumeRequested;
        event Action OptionsRequested;
    }
}
