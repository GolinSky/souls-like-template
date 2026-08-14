using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "ShieldDefinition", menuName = "Data/Items/Shield")]
    public sealed class ShieldDefinition : ItemDefinition
    {
        [SerializeField] private GameObject _equippedPrefab;
        [SerializeField, Min(0f)] private float _physicalGuard;
        [SerializeField, Min(0f)] private float _magicGuard;
        [SerializeField, Min(0f)] private float _fireGuard;
        [SerializeField, Min(0f)] private float _lightningGuard;
        [SerializeField, Min(0f)] private float _holyGuard;
        [SerializeField, Min(0f)] private float _guardBoost;
        [SerializeField] private AttributeRequirements _requirements;

        public override ItemType ItemType => ItemType.Shield;
        public GameObject EquippedPrefab => _equippedPrefab;

        public override ItemStatSnapshot Stats => new ItemStatSnapshot(
            0, 0, 0, 0, 0, 0,
            _physicalGuard,
            _magicGuard,
            _fireGuard,
            _lightningGuard,
            _holyGuard,
            _guardBoost,
            _requirements,
            default,
            string.Empty,
            0);
    }
}
