using UnityEngine;

namespace MultiPlayerTemplate.Ui.MainMenu
{
    public interface ILobbyNavigationPresenter
    {
        void RequestSkinSelection(bool show, Transform parentTransform);
        void RequestDeployment(bool show, Transform parentTransform);
    }
}
