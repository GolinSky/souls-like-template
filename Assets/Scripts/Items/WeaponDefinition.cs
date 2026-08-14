using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Data/Items/Weapon")]
    public sealed class WeaponDefinition : ItemDefinition
    {
        [Header("Runtime")]
        [SerializeField] private AnimationProfile _animationProfile;
        [SerializeField] private CombatProfile _combatProfile;
        [SerializeField] private GameObject _equippedPrefab;
        [SerializeField] private bool _canTwoHand = true;

        [Header("Attack Power")]
        [SerializeField, Min(0)] private int _physicalAttack;
        [SerializeField, Min(0)] private int _magicAttack;
        [SerializeField, Min(0)] private int _fireAttack;
        [SerializeField, Min(0)] private int _lightningAttack;
        [SerializeField, Min(0)] private int _holyAttack;
        [SerializeField, Min(0)] private int _critical = 100;

        [Header("Guard")]
        [SerializeField, Min(0f)] private float _physicalGuard;
        [SerializeField, Min(0f)] private float _magicGuard;
        [SerializeField, Min(0f)] private float _fireGuard;
        [SerializeField, Min(0f)] private float _lightningGuard;
        [SerializeField, Min(0f)] private float _holyGuard;
        [SerializeField, Min(0f)] private float _guardBoost;

        [Header("Requirements and Scaling")]
        [SerializeField] private AttributeRequirements _requirements;
        [SerializeField] private AttributeScaling _scaling;

        [Header("Skill")]
        [SerializeField] private string _skillName;
        [SerializeField] private Sprite _skillIcon;
        [SerializeField, Min(0)] private int _skillFocusCost;

        public override ItemType ItemType => ItemType.Weapon;
        public AnimationProfile AnimationProfile => _animationProfile;
        public CombatProfile CombatProfile => _combatProfile;
        public GameObject EquippedPrefab => _equippedPrefab;
        public bool CanTwoHand => _canTwoHand;
        public Sprite SkillIcon => _skillIcon;

        public override ItemStatSnapshot Stats => new ItemStatSnapshot(
            _physicalAttack,
            _magicAttack,
            _fireAttack,
            _lightningAttack,
            _holyAttack,
            _critical,
            _physicalGuard,
            _magicGuard,
            _fireGuard,
            _lightningGuard,
            _holyGuard,
            _guardBoost,
            _requirements,
            _scaling,
            _skillName,
            _skillFocusCost);
    }
}
