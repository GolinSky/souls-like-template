using SoulsLike.Ui.Inventory.Data;

namespace SoulsLike.Ui.Inventory
{
    public interface IInventoryPresenter
    {
        void SelectPrimaryCategory(InventoryPrimaryCategory category);
        void SelectSubCategory(InventorySubCategory subCategory);
        void OnItemFocused(InventoryItemSO item);
        void OnItemSubmitted(InventoryItemSO item);
        void CloseInventory();
        void ToggleLoreView();
        void ToggleSimpleView();
    }
}
