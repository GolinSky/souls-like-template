using System.Ui.Base;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.PauseNavigation
{
    public sealed class PauseNavigationUi : BaseUi
    {
        [SerializeField] private CustomButton openStatusButton;
        [SerializeField] private CustomButton openEquipmentButton;
        [SerializeField] private CustomButton openInventoryButton;
        [SerializeField] private CustomButton openSystemButton;

        private IPauseNavigationPresenter _presenter;

        public void AssignPresenter(IPauseNavigationPresenter presenter)
        {
            _presenter = presenter;
            openStatusButton.onClick.AddListener(_presenter.OpenStatus);
            openEquipmentButton.onClick.AddListener(_presenter.OpenEquipment);
            openInventoryButton.onClick.AddListener(_presenter.OpenInventory);
            openSystemButton.onClick.AddListener(_presenter.OpenSystem);
        }

        public override void Show()
        {
            base.Show();
            openStatusButton.Select();
        }

        private void OnDestroy()
        {
            if (_presenter == null)
            {
                return;
            }

            openStatusButton.onClick.RemoveListener(_presenter.OpenStatus);
            openEquipmentButton.onClick.RemoveListener(_presenter.OpenEquipment);
            openInventoryButton.onClick.RemoveListener(_presenter.OpenInventory);
            openSystemButton.onClick.RemoveListener(_presenter.OpenSystem);
        }

    }
}
