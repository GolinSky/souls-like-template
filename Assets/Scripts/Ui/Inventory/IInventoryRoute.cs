using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Ui.PauseNavigation;

namespace SoulsLike.Ui.Inventory
{
    public interface IInventoryRoute : IPauseNavigationRoute
    {
        void Open(
            IReadOnlyCollection<ItemType> itemTypes,
            Action<InventoryEntryId> itemSelected);
    }
}
