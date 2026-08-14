using System;
using System.Collections.Generic;
using SoulsLike.Items;

namespace SoulsLike.Entities.Character.Components.Inventory
{
    public enum InventoryChangeType
    {
        Added = 0,
        QuantityChanged = 1,
        Removed = 2
    }

    public readonly struct InventoryChange
    {
        public readonly InventoryChangeType Type;
        public readonly InventoryEntry Entry;

        public InventoryChange(InventoryChangeType type, InventoryEntry entry)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Type = type;
        }
    }

    public sealed class InventoryModel
    {
        private readonly List<InventoryEntry> _entries = new();
        private readonly Dictionary<InventoryEntryId, InventoryEntry> _entriesById = new();

        public event Action<InventoryChange> Changed;

        public IReadOnlyList<InventoryEntry> Entries => _entries;

        internal void AddEntry(InventoryEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (!_entriesById.TryAdd(entry.EntryId, entry))
            {
                throw new InvalidOperationException($"Inventory already contains entry '{entry.EntryId}'.");
            }

            _entries.Add(entry);
            Changed?.Invoke(new InventoryChange(InventoryChangeType.Added, entry));
        }

        internal void UpdateQuantity(InventoryEntry entry, int quantity)
        {
            RequireOwnedEntry(entry);
            entry.SetQuantity(quantity);
            Changed?.Invoke(new InventoryChange(InventoryChangeType.QuantityChanged, entry));
        }

        internal void RemoveEntry(InventoryEntry entry)
        {
            RequireOwnedEntry(entry);
            _entriesById.Remove(entry.EntryId);
            _entries.Remove(entry);
            Changed?.Invoke(new InventoryChange(InventoryChangeType.Removed, entry));
        }

        public bool TryGetEntry(InventoryEntryId entryId, out InventoryEntry entry)
        {
            return _entriesById.TryGetValue(entryId, out entry);
        }

        public InventoryEntry GetRequiredEntry(InventoryEntryId entryId)
        {
            if (!_entriesById.TryGetValue(entryId, out InventoryEntry entry))
            {
                throw new KeyNotFoundException($"Inventory entry '{entryId}' does not exist.");
            }

            return entry;
        }

        private void RequireOwnedEntry(InventoryEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (!_entriesById.TryGetValue(entry.EntryId, out InventoryEntry ownedEntry)
                || !ReferenceEquals(ownedEntry, entry))
            {
                throw new InvalidOperationException($"Inventory does not own entry '{entry.EntryId}'.");
            }
        }
    }
}
