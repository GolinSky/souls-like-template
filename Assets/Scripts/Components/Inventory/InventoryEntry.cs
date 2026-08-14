using System;
using SoulsLike.Items;

namespace SoulsLike.Entities.Character.Components.Inventory
{
    [Serializable]
    public readonly struct InventoryEntryId : IEquatable<InventoryEntryId>
    {
        public readonly string Value;

        public InventoryEntryId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Inventory entry ID cannot be empty.", nameof(value));
            }

            Value = value;
        }

        public static InventoryEntryId Create()
        {
            return new InventoryEntryId(Guid.NewGuid().ToString("N"));
        }

        public bool Equals(InventoryEntryId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is InventoryEntryId other && Equals(other);
        public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(InventoryEntryId left, InventoryEntryId right) => left.Equals(right);
        public static bool operator !=(InventoryEntryId left, InventoryEntryId right) => !left.Equals(right);
    }

    [Serializable]
    public sealed class InventoryInstanceState
    {
        public int UpgradeLevel { get; private set; }
        public string AffinityId { get; private set; } = string.Empty;

        public void SetUpgradeLevel(int upgradeLevel)
        {
            if (upgradeLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(upgradeLevel), upgradeLevel, null);
            }

            UpgradeLevel = upgradeLevel;
        }

        public void SetAffinity(string affinityId)
        {
            AffinityId = affinityId ?? throw new ArgumentNullException(nameof(affinityId));
        }
    }

    [Serializable]
    public sealed class InventoryEntry
    {
        public InventoryEntryId EntryId { get; }
        public ItemId ItemId { get; }
        public int Quantity { get; private set; }
        public InventoryInstanceState InstanceState { get; }

        public InventoryEntry(
            InventoryEntryId entryId,
            ItemId itemId,
            int quantity,
            InventoryInstanceState instanceState = null)
        {
            if (itemId == ItemId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(itemId), itemId, null);
            }

            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null);
            }

            EntryId = entryId;
            ItemId = itemId;
            Quantity = quantity;
            InstanceState = instanceState ?? new InventoryInstanceState();
        }

        internal void SetQuantity(int quantity)
        {
            if (quantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null);
            }

            Quantity = quantity;
        }
    }
}
