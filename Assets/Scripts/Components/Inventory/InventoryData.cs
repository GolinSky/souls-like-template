using System;
using System.Collections.Generic;
using SoulsLike.Items;
using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Inventory
{
    [Serializable]
    public struct InitialInventoryEntry
    {
        [field: SerializeField] public ItemId ItemId { get; private set; }
        [field: SerializeField, Min(1)] public int Quantity { get; private set; }
    }

    [CreateAssetMenu(fileName = "InventoryData", menuName = "Data/InventoryData")]
    public sealed class InventoryData : Data
    {
        [SerializeField] private List<InitialInventoryEntry> _initialEntries = new();

        public IReadOnlyList<InitialInventoryEntry> InitialEntries => _initialEntries;
    }
}
