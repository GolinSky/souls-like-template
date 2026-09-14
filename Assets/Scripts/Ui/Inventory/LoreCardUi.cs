using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public sealed class LoreCardUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text loreItemName;
        [SerializeField] private Image loreItemArtwork;
        [SerializeField] private TMP_Text loreFullText;

        public void Display(InventoryItemViewData item)
        {
            loreItemName.text = item.DisplayName;
            loreItemArtwork.sprite = item.Icon;
            loreItemArtwork.enabled = item.Icon != null;
            loreFullText.text = $"{item.Description}\n\n{item.LoreDescription}";
        }

        public void DisplayEmpty(string slotName, Sprite slotIcon)
        {
            loreItemName.text = "Empty slot";
            loreItemArtwork.sprite = slotIcon;
            loreItemArtwork.enabled = slotIcon != null;
            loreFullText.text = $"No {slotName} equipped. Select this slot to choose equipment from your inventory.";
        }
    }
}
