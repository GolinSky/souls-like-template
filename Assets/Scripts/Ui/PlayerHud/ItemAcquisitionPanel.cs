using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.PlayerHud
{
    public sealed class ItemAcquisitionPanel : MonoBehaviour
    {
        private const float VISIBLE_DURATION = 2.4f;
        private const float FADE_DURATION = 0.3f;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI quantityText;

        private float _remainingTime;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (_remainingTime <= 0f)
            {
                return;
            }

            _remainingTime -= Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(_remainingTime / FADE_DURATION);
        }

        public void ShowAcquisition(string itemName, Sprite itemIcon, int quantity)
        {
            itemNameText.text = itemName;
            quantityText.text = quantity > 1 ? $"x{quantity:N0}" : string.Empty;
            icon.sprite = itemIcon;
            icon.enabled = itemIcon != null;
            canvasGroup.alpha = 1f;
            _remainingTime = VISIBLE_DURATION;
        }

        public void ShowMessage(string message)
        {
            ShowAcquisition(message, null, 1);
        }
    }
}
