using System.Ui.Base;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Grace
{
    public sealed class GraceUi : BaseUi
    {
        [SerializeField] private CustomButton travelButton;
        [SerializeField] private CustomButton leaveButton;

        private IGraceUiPresenter _presenter;

        public void AssignPresenter(IGraceUiPresenter presenter)
        {
            _presenter = presenter;
            travelButton.onClick.AddListener(_presenter.OpenTravel);
            leaveButton.onClick.AddListener(_presenter.Leave);
        }

        private void OnDestroy()
        {
            travelButton.onClick.RemoveListener(_presenter.OpenTravel);
            leaveButton.onClick.RemoveListener(_presenter.Leave);
        }
    }
}
