using System;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Ui.PauseNavigation;

namespace SoulsLike.Ui.Equipment
{
    public interface IEquipmentRoute : IPauseNavigationRoute
    {
        event Action<EquipmentSlotId> InventoryRequested;

        void SelectItem(InventoryEntryId entryId);
    }
}
