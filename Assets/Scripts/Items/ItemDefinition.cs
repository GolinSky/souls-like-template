using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Items
{
    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private ItemId _itemId;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 5)] private string _description;
        [SerializeField, TextArea(4, 10)] private string _loreDescription;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0f)] private float _weight;
        [SerializeField, Min(1)] private int _maxStack = 1;
        [SerializeField] private List<EquipmentGroup> _equipmentGroups = new();

        public ItemId ItemId => _itemId;
        public abstract ItemType ItemType { get; }
        public string DisplayName => _displayName;
        public string Description => _description;
        public string LoreDescription => _loreDescription;
        public Sprite Icon => _icon;
        public float Weight => _weight;
        public int MaxStack => _maxStack;
        public bool IsStackable => _maxStack > 1;
        public IReadOnlyList<EquipmentGroup> EquipmentGroups => _equipmentGroups;
        public virtual ItemStatSnapshot Stats => ItemStatSnapshot.Empty;

        public bool CanEquipIn(EquipmentGroup group)
        {
            if (group == EquipmentGroup.None)
            {
                throw new ArgumentOutOfRangeException(nameof(group), group, "An equipment group is required.");
            }

            return _equipmentGroups.Contains(group);
        }

        public void ValidateDefinition()
        {
            if (_itemId == ItemId.None)
            {
                throw new InvalidOperationException($"Item definition '{name}' requires a non-None ItemId.");
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                throw new InvalidOperationException($"Item definition '{name}' requires a display name.");
            }

            if (_maxStack < 1)
            {
                throw new InvalidOperationException($"Item definition '{name}' requires MaxStack >= 1.");
            }
        }
    }
}
