using UnityEngine;
using SoulsLike.Ui.Base;
using System.Ui.Base;

namespace SoulsLike
{
    public class PauseMenuUi : BaseUi
    {
        [SerializeField] private CustomButton resumeButton;
        [SerializeField] private CustomButton optionsButton;
        [SerializeField] private CustomButton exitButton;

        private IPauseMenuPresenter _presenter;

        public void Initialize(IPauseMenuPresenter presenter)
        {
            _presenter = presenter;
            
            resumeButton.onClick.AddListener(_presenter.ResumeGame);
            optionsButton.onClick.AddListener(_presenter.OpenOptions);
            exitButton.onClick.AddListener(_presenter.QuitGame);
        }
        
        private void OnDestroy()
        {
            resumeButton.onClick.RemoveListener(_presenter.ResumeGame);
            optionsButton.onClick.RemoveListener(_presenter.OpenOptions);
            exitButton.onClick.RemoveListener(_presenter.QuitGame);
        }
    }
}
