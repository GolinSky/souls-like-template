using System;

namespace SoulsLike.Ui.Grace
{
    public interface IGraceRoute
    {
        event Action CloseRequested;

        void Show();
        void Hide();
    }
}
