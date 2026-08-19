using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Inventory
{
    public sealed class InventoryComponent : BaseComponent<InventoryModel>, IInitializable
    {
        private ItemCatalog _itemCatalog;
        private InventoryData _inventoryData;

        [Inject]
        public void InjectDependencies(ItemCatalog itemCatalog, InventoryData inventoryData)
        {
            _itemCatalog = itemCatalog;
            _inventoryData = inventoryData;
        }

        public void Initialize()
        {
            foreach (InitialInventoryEntry initialEntry in _inventoryData.InitialEntries)
            {
                Add(initialEntry.ItemId, initialEntry.Quantity);
            }
        }

        public IReadOnlyList<InventoryEntry> Entries => Model.Entries;

        public IReadOnlyList<InventoryEntry> Add(ItemId itemId, int quantity)
        {
            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null);
            }

            ItemDefinition definition = _itemCatalog.GetItem(itemId);
            var affectedEntries = new List<InventoryEntry>();
            int remaining = quantity;

            if (definition.IsStackable)
            {
                foreach (InventoryEntry entry in Model.Entries)
                {
                    if (entry.ItemId != itemId || entry.Quantity >= definition.MaxStack)
                    {
                        continue;
                    }

                    int added = Math.Min(remaining, definition.MaxStack - entry.Quantity);
                    Model.UpdateQuantity(entry, entry.Quantity + added);
                    affectedEntries.Add(entry);
                    remaining -= added;
                    if (remaining == 0)
                    {
                        return affectedEntries;
                    }
                }
            }

            while (remaining > 0)
            {
                int entryQuantity = definition.IsStackable
                    ? Math.Min(remaining, definition.MaxStack)
                    : 1;
                var entry = new InventoryEntry(
                    InventoryEntryId.Create(),
                    itemId,
                    entryQuantity);
                Model.AddEntry(entry);
                affectedEntries.Add(entry);
                remaining -= entryQuantity;
            }

            return affectedEntries;
        }

        public void Remove(InventoryEntryId entryId, int quantity)
        {
            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null);
            }

            InventoryEntry entry = Model.GetRequiredEntry(entryId);
            if (quantity > entry.Quantity)
            {
                throw new InvalidOperationException(
                    $"Cannot remove {quantity} from inventory entry '{entryId}' with quantity {entry.Quantity}.");
            }

            if (quantity == entry.Quantity)
            {
                Model.RemoveEntry(entry);
                return;
            }

            Model.UpdateQuantity(entry, entry.Quantity - quantity);
        }

        public void Consume(InventoryEntryId entryId, int quantity = 1)
        {
            InventoryEntry entry = Model.GetRequiredEntry(entryId);
            ItemDefinition definition = _itemCatalog.GetItem(entry.ItemId);
            if (definition.ItemType != ItemType.Consumable)
            {
                throw new InvalidOperationException($"Item '{definition.DisplayName}' is not consumable.");
            }

            Remove(entryId, quantity);
        }

        public bool TryGetEntry(InventoryEntryId entryId, out InventoryEntry entry)
        {
            return Model.TryGetEntry(entryId, out entry);
        }

        public InventoryEntry GetRequiredEntry(InventoryEntryId entryId)
        {
            return Model.GetRequiredEntry(entryId);
        }
    }
}
