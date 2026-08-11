using System;
using MPUIKIT;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public class InventorySlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
    {
        [Header("MPUIKit Visual Components")]
        [SerializeField] private MPImage backgroundBox;
        [SerializeField] private MPImage focusFrame;
        [SerializeField] private MPImage equippedBadgeBox;
        [SerializeField] private MPImage unmetRequirementOverlay;

        [Header("Content Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private TMP_Text equippedBadgeText;
        [SerializeField] private Image ashOfWarIcon;
        [SerializeField] private Image unmetWarningIcon;

        public InventoryItemSO CurrentItem { get; private set; }
        public int ItemQuantity { get; private set; } = 1;
        public bool IsEquipped { get; private set; }
        public bool MeetsRequirements { get; private set; } = true;

        public event Action<InventorySlotUI> OnSlotSelected;
        public event Action<InventorySlotUI> OnSlotClicked;

        public void Bind(InventoryItemSO item, int quantity = 1, bool isEquipped = false, string equipLabel = "R1", bool meetsReqs = true)
        {
            CurrentItem = item;
            ItemQuantity = quantity;
            IsEquipped = isEquipped;
            MeetsRequirements = meetsReqs;

            if (item == null)
            {
                Clear();
                return;
            }

            if (itemIcon != null)
            {
                itemIcon.sprite = item.itemIcon;
                itemIcon.enabled = item.itemIcon != null;
            }

            if (quantityText != null)
            {
                bool showQty = item.isStackable && quantity > 1;
                quantityText.text = showQty ? $"x{quantity}" : string.Empty;
                quantityText.gameObject.SetActive(showQty);
            }

            if (equippedBadgeBox != null)
            {
                equippedBadgeBox.gameObject.SetActive(isEquipped);
                if (equippedBadgeText != null && isEquipped)
                {
                    equippedBadgeText.text = equipLabel;
                }
            }

            if (unmetRequirementOverlay != null)
            {
                unmetRequirementOverlay.gameObject.SetActive(!meetsReqs);
            }

            if (ashOfWarIcon != null)
            {
                bool hasSkill = item.skillIcon != null;
                ashOfWarIcon.sprite = item.skillIcon;
                ashOfWarIcon.gameObject.SetActive(hasSkill);
            }

            SetFocusState(false);
        }

        public void Clear()
        {
            CurrentItem = null;
            if (itemIcon != null) itemIcon.enabled = false;
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (equippedBadgeBox != null) equippedBadgeBox.gameObject.SetActive(false);
            if (unmetRequirementOverlay != null) unmetRequirementOverlay.gameObject.SetActive(false);
            if (ashOfWarIcon != null) ashOfWarIcon.gameObject.SetActive(false);
            SetFocusState(false);
        }

        public void SetFocusState(bool focused)
        {
            if (focusFrame != null)
            {
                focusFrame.gameObject.SetActive(focused);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetFocusState(true);
            OnSlotSelected?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetFocusState(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}
