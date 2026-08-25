using System;
using System.Ui.Base;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.PauseNavigation
{
    public sealed class PauseNavigationUi : BaseUi
    {
        [SerializeField] private CustomButton openEquipmentButton;
        [SerializeField] private CustomButton openInventoryButton;
        [SerializeField] private CustomButton openSystemButton;

        private IPauseNavigationPresenter _presenter;

        public void AssignPresenter(IPauseNavigationPresenter presenter)
        {
            _presenter = presenter;
            openEquipmentButton.onClick.AddListener(_presenter.OpenEquipment);
            openInventoryButton.onClick.AddListener(_presenter.OpenInventory);
            openSystemButton.onClick.AddListener(_presenter.OpenSystem);
        }

        private void OnDestroy()
        {
            if (_presenter == null)
            {
                return;
            }

            openEquipmentButton.onClick.RemoveListener(_presenter.OpenEquipment);
            openInventoryButton.onClick.RemoveListener(_presenter.OpenInventory);
            openSystemButton.onClick.RemoveListener(_presenter.OpenSystem);
        }

        protected override void Awake()
        {
            if (openEquipmentButton == null
                || openInventoryButton == null
                || openSystemButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PauseNavigationUi)} '{name}' has missing button references.");
            }

            base.Awake();
        }
    }
}
