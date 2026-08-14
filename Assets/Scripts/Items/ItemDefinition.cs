using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Items
{
    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private ItemId itemId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField, TextArea(4, 10)] private string loreDescription;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(0f)] private float weight;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField] private List<EquipmentGroup> equipmentGroups = new();

        public ItemId ItemId => itemId;
        public abstract ItemType ItemType { get; }
        public string DisplayName => displayName;
        public string Description => description;
        public string LoreDescription => loreDescription;
        public Sprite Icon => icon;
        public float Weight => weight;
        public int MaxStack => maxStack;
        public bool IsStackable => maxStack > 1;
        public IReadOnlyList<EquipmentGroup> EquipmentGroups => equipmentGroups;
        public virtual ItemStatSnapshot Stats => ItemStatSnapshot.Empty;

        public bool CanEquipIn(EquipmentGroup group)
        {
            if (group == EquipmentGroup.None)
            {
                throw new ArgumentOutOfRangeException(nameof(group), group, "An equipment group is required.");
            }

            return equipmentGroups.Contains(group);
        }

        public void ValidateDefinition()
        {
            if (itemId == ItemId.None)
            {
                throw new InvalidOperationException($"Item definition '{name}' requires a non-None ItemId.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new InvalidOperationException($"Item definition '{name}' requires a display name.");
            }

            if (maxStack < 1)
            {
                throw new InvalidOperationException($"Item definition '{name}' requires MaxStack >= 1.");
            }
        }
    }
}
