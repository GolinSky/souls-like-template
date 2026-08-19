using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Popups
{
    public class AlertPopup : BasePopup
    {
        [SerializeField] private Button closeButton;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            closeButton.onClick.AddListener(HandleNoButtonClick);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            closeButton.onClick.RemoveListener(HandleNoButtonClick);
        }
    }
}
