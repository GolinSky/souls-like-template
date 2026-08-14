using System;
using MPUIKIT;
using SoulsLike.Items;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public sealed class InventorySlotUI : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerClickHandler,
        IPointerEnterHandler,
        ISubmitHandler,
        IMoveHandler
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

        private InventorySlotUI _up;
        private InventorySlotUI _down;
        private InventorySlotUI _left;
        private InventorySlotUI _right;

        public InventoryItemViewData CurrentItem { get; private set; }

        public event Action<InventorySlotUI> SlotSelected;
        public event Action<InventorySlotUI> SlotSubmitted;

        private void Awake()
        {
            if (backgroundBox == null
                || focusFrame == null
                || equippedBadgeBox == null
                || unmetRequirementOverlay == null
                || itemIcon == null
                || quantityText == null
                || equippedBadgeText == null
                || ashOfWarIcon == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InventorySlotUI)} '{name}' has missing serialized references.");
            }
        }

        public void Bind(InventoryItemViewData item)
        {
            CurrentItem = item ?? throw new ArgumentNullException(nameof(item));
            ItemDefinition definition = item.Definition;

            itemIcon.sprite = definition.Icon;
            itemIcon.enabled = definition.Icon != null;

            bool showQuantity = definition.IsStackable && item.Quantity > 1;
            quantityText.text = showQuantity ? $"x{item.Quantity}" : string.Empty;
            quantityText.gameObject.SetActive(showQuantity);

            equippedBadgeBox.gameObject.SetActive(item.IsEquipped);
            equippedBadgeText.text = item.IsEquipped ? item.EquipmentLabel : string.Empty;
            unmetRequirementOverlay.gameObject.SetActive(!item.MeetsRequirements);

            Sprite skillIcon = definition is WeaponDefinition weapon ? weapon.SkillIcon : null;
            ashOfWarIcon.sprite = skillIcon;
            ashOfWarIcon.gameObject.SetActive(skillIcon != null);
            SetFocusState(false);
        }

        public void ConfigureNavigation(
            InventorySlotUI up,
            InventorySlotUI down,
            InventorySlotUI left,
            InventorySlotUI right)
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
                throw new InvalidOperationException("Inventory UI requires an active EventSystem.");
            }

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetFocusState(true);
            SlotSelected?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetFocusState(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SlotSubmitted?.Invoke(this);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            SlotSubmitted?.Invoke(this);
        }

        public void OnMove(AxisEventData eventData)
        {
            InventorySlotUI target = eventData.moveDir switch
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

        private void SetFocusState(bool focused)
        {
            focusFrame.gameObject.SetActive(focused);
        }
    }
}
