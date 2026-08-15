using System;
using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.PlayerHud
{
    [Serializable]
    public class EquipmentSlotHud
    {
        [Header("Container & Visuals")]
        public RectTransform container;
        public MPImage background;
        public MPImage border;
        public Image icon;
        public TMP_Text quantityText;
        public CanvasGroup canvasGroup;

        [Header("Border Colors")]
        public Color normalBorderColor = new Color(0.23f, 0.20f, 0.17f, 1f);
        public Color activeBorderColor = new Color(0.48f, 0.41f, 0.30f, 1f);

        public void SetItem(Sprite itemIcon, int quantity = 0, bool isDimmed = false)
        {
            if (icon != null)
            {
                icon.sprite = itemIcon;
                icon.enabled = itemIcon != null;
            }

            if (quantityText != null)
            {
                bool showQuantity = quantity > 0;
                quantityText.text = showQuantity ? quantity.ToString() : string.Empty;
                quantityText.gameObject.SetActive(showQuantity);
            }

            if (border != null)
            {
                border.OutlineColor = itemIcon != null ? activeBorderColor : normalBorderColor;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isDimmed ? 0.35f : (itemIcon != null ? 1f : 0.65f);
            }
        }

        public void SetEmpty(bool isDimmed = false)
        {
            SetItem(null, 0, isDimmed);
        }
    }
}
