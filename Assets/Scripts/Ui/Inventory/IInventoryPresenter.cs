using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Ui.Inventory.Data;

namespace SoulsLike.Ui.Inventory
{
    public interface IInventoryPresenter
    {
        void SelectPrimaryCategory(InventoryPrimaryCategory category);
        void SelectSubCategory(InventorySubCategory subCategory);
        void OnItemFocused(InventoryEntryId entryId);
        void OnItemSubmitted(InventoryEntryId entryId);
        void CloseInventory();
        void ToggleLoreView();
        void ToggleSimpleView();
    }
}
