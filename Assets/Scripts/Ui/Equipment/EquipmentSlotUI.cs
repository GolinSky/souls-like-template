using System;
using MPUIKIT;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulsLike.Ui.Equipment
{
    public sealed class EquipmentSlotUI : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerClickHandler,
        IPointerEnterHandler,
        ISubmitHandler,
        IMoveHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private MPImage borderImage;
        [SerializeField] private MPImage selectionHighlight;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private TMP_Text quantityText;

        [Header("Visual Colors")]
        [SerializeField] private Color normalBorderColor = new(0.102f, 0.102f, 0.094f, 1f);
        [SerializeField] private Color selectedBorderColor = new(0.102f, 0.102f, 0.094f, 1f);

        private EquipmentSlotUI _up;
        private EquipmentSlotUI _down;
        private EquipmentSlotUI _left;
        private EquipmentSlotUI _right;

        public EquipmentSlotId SlotId { get; private set; }
        public InventoryItemViewData CurrentItem { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsEmpty => CurrentItem == null;

        public event Action<EquipmentSlotUI> SlotFocused;
        public event Action<EquipmentSlotUI> SlotSubmitted;

        private void Awake()
        {
            if (iconImage == null
                || borderImage == null
                || selectionHighlight == null
                || lockOverlay == null
                || quantityText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentSlotUI)} '{name}' has missing serialized references.");
            }
        }

        public void Bind(
            EquipmentSlotId slotId,
            InventoryItemViewData item,
            bool isLocked = false)
        {
            SlotId = slotId;
            CurrentItem = item;
            IsLocked = isLocked;
            lockOverlay.SetActive(isLocked);

            iconImage.sprite = item == null ? null : item.Icon;
            iconImage.enabled = !isLocked && item?.Icon != null;
            bool showQuantity = !isLocked
                && item != null
                && item.IsStackable
                && item.Quantity > 1;
            quantityText.text = showQuantity ? item.Quantity.ToString() : string.Empty;
            quantityText.gameObject.SetActive(showQuantity);
            SetHighlight(false);
        }

        public void ConfigureNavigation(
            EquipmentSlotUI up,
            EquipmentSlotUI down,
            EquipmentSlotUI left,
            EquipmentSlotUI right)
        {
            _up = up;
            _down = down;
            _left = left;
            _right = right;
        }

        public void Select()
        {
            if (EventSystem.current == null)
            {
                throw new InvalidOperationException("Equipment UI requires an active EventSystem.");
            }

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetHighlight(true);
            SlotFocused?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Submit();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Submit();
        }

        public void OnMove(AxisEventData eventData)
        {
            EquipmentSlotUI target = eventData.moveDir switch
            {
                MoveDirection.Up => _up,
                MoveDirection.Down => _down,
                MoveDirection.Left => _left,
                MoveDirection.Right => _right,
                _ => null
            };

            if (target != null)
            {
                target.Select();
                eventData.Use();
            }
        }

        private void Submit()
        {
            if (!IsLocked)
            {
                SlotSubmitted?.Invoke(this);
            }
        }

        private void SetHighlight(bool highlighted)
        {
            selectionHighlight.gameObject.SetActive(highlighted);
            borderImage.color = highlighted ? selectedBorderColor : normalBorderColor;
        }
    }
}
