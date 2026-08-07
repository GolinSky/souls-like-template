using System.Ui.Base;
using SoulsLike.Ui.Base;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.MainMenu
{
    public class MainMenuUi: BaseUi, IStartable
    {
        [SerializeField] private CustomButton playButton;
        [SerializeField] private CustomButton optionsButton;
        [SerializeField] private CustomButton exitButton;
     
        private IMainMenuPresenter Presenter { get; set; }


        void IStartable.Start()
        {
            playButton.onClick.AddListener(Presenter.PlayGame);
            optionsButton.onClick.AddListener(Presenter.OpenOptions);
            exitButton.onClick.AddListener(Presenter.ExitGame);
        }

        public void OnDestroy()
        {
            playButton.onClick.RemoveListener(Presenter.PlayGame);
            optionsButton.onClick.RemoveListener(Presenter.OpenOptions);
            exitButton.onClick.RemoveListener(Presenter.ExitGame);
        }

        public void AssignPresenter(IMainMenuPresenter presenter)
        {
            Presenter = presenter;
        }
    }
}