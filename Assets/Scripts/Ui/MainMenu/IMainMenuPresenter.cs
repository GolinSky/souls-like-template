using UnityEngine;

namespace SoulsLike.Ui.MainMenu
{
    public interface IMainMenuPresenter
    {
        void PlayGame();
        void OpenOptions();
        void ExitGame();
    }
}
