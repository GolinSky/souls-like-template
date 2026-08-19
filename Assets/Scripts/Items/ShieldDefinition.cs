using System;
using UnityEngine;

namespace SoulsLike.Items
{
    [Serializable]
    public sealed class ShieldDefinition
    {
        [SerializeField] private ItemId itemId;
        [SerializeField] private GameObject equippedPrefab;
        [SerializeField, Min(0f)] private float physicalGuard;
        [SerializeField, Min(0f)] private float magicGuard;
        [SerializeField, Min(0f)] private float fireGuard;
        [SerializeField, Min(0f)] private float lightningGuard;
        [SerializeField, Min(0f)] private float holyGuard;
        [SerializeField, Min(0f)] private float guardBoost;
        [SerializeField] private AttributeRequirements requirements;

        public ItemId ItemId => itemId;
        public GameObject EquippedPrefab => equippedPrefab;

        public ItemStatSnapshot Stats => new(
            0, 0, 0, 0, 0, 0,
            physicalGuard,
            magicGuard,
            fireGuard,
            lightningGuard,
            holyGuard,
            guardBoost,
            requirements,
            default,
            string.Empty,
            0);

        public void ValidateDefinition()
        {
            if (itemId == ItemId.None)
            {
                throw new InvalidOperationException("Shield definition requires a non-None ItemId.");
            }
        }
    }
}
