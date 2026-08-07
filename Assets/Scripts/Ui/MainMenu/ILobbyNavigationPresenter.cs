using UnityEngine;

namespace SoulsLike.Ui.MainMenu
{
    public interface ILobbyNavigationPresenter
    {
        void RequestSkinSelection(bool show, Transform parentTransform);
        void RequestDeployment(bool show, Transform parentTransform);
    }
}
