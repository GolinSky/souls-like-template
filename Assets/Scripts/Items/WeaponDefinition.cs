using System;
using UnityEngine;

namespace SoulsLike.Items
{
    [Serializable]
    public sealed class WeaponDefinition
    {
        [SerializeField] private ItemId itemId;

        [Header("Runtime")]
        [SerializeField] private AnimationProfile animationProfile;
        [SerializeField] private CombatProfile combatProfile;
        [SerializeField] private GameObject equippedPrefab;
        [SerializeField] private bool canTwoHand = true;

        [Header("Attack Power")]
        [SerializeField, Min(0)] private int physicalAttack;
        [SerializeField, Min(0)] private int magicAttack;
        [SerializeField, Min(0)] private int fireAttack;
        [SerializeField, Min(0)] private int lightningAttack;
        [SerializeField, Min(0)] private int holyAttack;
        [SerializeField, Min(0)] private int critical = 100;

        [Header("Guard")]
        [SerializeField, Min(0f)] private float physicalGuard;
        [SerializeField, Min(0f)] private float magicGuard;
        [SerializeField, Min(0f)] private float fireGuard;
        [SerializeField, Min(0f)] private float lightningGuard;
        [SerializeField, Min(0f)] private float holyGuard;
        [SerializeField, Min(0f)] private float guardBoost;

        [Header("Requirements and Scaling")]
        [SerializeField] private AttributeRequirements requirements;
        [SerializeField] private AttributeScaling scaling;

        [Header("Skill")]
        [SerializeField] private string skillName;
        [SerializeField] private Sprite skillIcon;
        [SerializeField, Min(0)] private int skillFocusCost;

        public ItemId ItemId => itemId;
        public AnimationProfile AnimationProfile => animationProfile;
        public CombatProfile CombatProfile => combatProfile;
        public GameObject EquippedPrefab => equippedPrefab;
        public bool CanTwoHand => canTwoHand;
        public Sprite SkillIcon => skillIcon;

        public ItemStatSnapshot Stats => new(
            physicalAttack,
            magicAttack,
            fireAttack,
            lightningAttack,
            holyAttack,
            critical,
            physicalGuard,
            magicGuard,
            fireGuard,
            lightningGuard,
            holyGuard,
            guardBoost,
            requirements,
            scaling,
            skillName,
            skillFocusCost);

        public void ValidateDefinition()
        {
            if (itemId == ItemId.None)
            {
                throw new InvalidOperationException("Weapon definition requires a non-None ItemId.");
            }
        }
    }
}
