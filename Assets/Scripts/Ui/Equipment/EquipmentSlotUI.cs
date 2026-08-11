using System;
using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulsLike.Ui.Equipment
{
    public class EquipmentSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private MPImage borderImage;
        [SerializeField] private MPImage selectionHighlight;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private TMP_Text slotNameText;

        [Header("Visual Colors")]
        [SerializeField] private Color normalBorderColor = new Color(0.23f, 0.20f, 0.17f, 1f); // #3A342B
        [SerializeField] private Color selectedBorderColor = new Color(0.77f, 0.63f, 0.35f, 1f); // #C5A059 (Gold)

        public string SlotId { get; private set; }
        public string SlotCategory { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsEmpty => iconImage == null || !iconImage.enabled || iconImage.sprite == null;

        public event Action<EquipmentSlotUI> OnSlotHighlighted;
        public event Action<EquipmentSlotUI> OnSlotClicked;

        public void SetupSlot(string slotId, string categoryName, Sprite icon = null, bool isLocked = false, int quantity = 0)
        {
            SlotId = slotId;
            SlotCategory = categoryName;
            IsLocked = isLocked;

            if (slotNameText != null)
            {
                slotNameText.text = categoryName;
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(isLocked);
            }

            if (!isLocked && icon != null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = icon;
                    iconImage.enabled = true;
                }
            }
            else
            {
                if (iconImage != null)
                {
                    iconImage.enabled = false;
                }
            }

            if (quantityText != null)
            {
                if (quantity > 1)
                {
                    quantityText.text = quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }

            SetHighlight(false);
        }

        public void SetHighlight(bool highlighted)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(highlighted);
            }

            if (borderImage != null)
            {
                borderImage.color = highlighted ? selectedBorderColor : normalBorderColor;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetHighlight(true);
            OnSlotHighlighted?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
            OnSlotHighlighted?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}
