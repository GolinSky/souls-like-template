using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ShieldDefinition", menuName = "Data/Items/Shield")]
    public sealed class ShieldDefinition : ItemDefinition
    {
        [SerializeField] private GameObject equippedPrefab;
        [SerializeField, Min(0f)] private float physicalGuard;
        [SerializeField, Min(0f)] private float magicGuard;
        [SerializeField, Min(0f)] private float fireGuard;
        [SerializeField, Min(0f)] private float lightningGuard;
        [SerializeField, Min(0f)] private float holyGuard;
        [SerializeField, Min(0f)] private float guardBoost;
        [SerializeField] private AttributeRequirements requirements;

        public override ItemType ItemType => ItemType.Shield;
        public GameObject EquippedPrefab => equippedPrefab;

        public override ItemStatSnapshot Stats => new ItemStatSnapshot(
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
    }
}
