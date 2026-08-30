using System;
using SoulsLike.Ui.Navigation;

namespace SoulsLike.Ui.Grace
{
    public interface IGraceRoute : IUiRoute
    {
        event Action CloseRequested;
    }
}
