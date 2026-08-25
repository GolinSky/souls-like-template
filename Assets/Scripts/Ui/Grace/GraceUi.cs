using System.Ui.Base;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Grace
{
    public sealed class GraceUi : BaseUi
    {
        [SerializeField] private CustomButton leaveButton;

        private IGraceUiPresenter _presenter;

        public void AssignPresenter(IGraceUiPresenter presenter)
        {
            _presenter = presenter;
            leaveButton.onClick.AddListener(_presenter.Leave);
        }

        private void OnDestroy()
        {
            leaveButton.onClick.RemoveListener(_presenter.Leave);
        }
    }
}
