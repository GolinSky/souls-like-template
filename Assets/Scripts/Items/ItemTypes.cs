using System;

namespace SoulsLike.Items
{
    public enum ItemId
    {
        None = 0,
        GreatSword = 1,
        WoodenShield = 2,
        CrimsonFlask = 3,
        LightningGrease = 4,
        GoldenRuneSmall = 5
    }

    public enum ItemType
    {
        Weapon = 0,
        Shield = 1,
        Armor = 2,
        Talisman = 3,
        Ammunition = 4,
        Consumable = 5,
        KeyItem = 6,
        Material = 7,
        Currency = 8
    }

    public enum EquipmentGroup
    {
        None = 0,
        Armament = 1,
        Arrow = 2,
        Bolt = 3,
        HeadArmor = 4,
        ChestArmor = 5,
        ArmArmor = 6,
        LegArmor = 7,
        Talisman = 8,
        QuickItem = 9
    }

    public enum ItemUseType
    {
        None = 0,
        Heal = 1,
        GrantCurrency = 2,
        InfuseActiveWeapon = 3
    }

    public enum ScalingGrade
    {
        None = 0,
        E = 1,
        D = 2,
        C = 3,
        B = 4,
        A = 5,
        S = 6
    }

    [Serializable]
    public struct AttributeRequirements
    {
        public int Strength;
        public int Dexterity;
        public int Intelligence;
        public int Faith;
        public int Arcane;
    }

    [Serializable]
    public struct AttributeScaling
    {
        public ScalingGrade Strength;
        public ScalingGrade Dexterity;
        public ScalingGrade Intelligence;
        public ScalingGrade Faith;
        public ScalingGrade Arcane;
    }

    public readonly struct ItemStatSnapshot
    {
        public readonly int PhysicalAttack;
        public readonly int MagicAttack;
        public readonly int FireAttack;
        public readonly int LightningAttack;
        public readonly int HolyAttack;
        public readonly int Critical;
        public readonly float PhysicalGuard;
        public readonly float MagicGuard;
        public readonly float FireGuard;
        public readonly float LightningGuard;
        public readonly float HolyGuard;
        public readonly float GuardBoost;
        public readonly AttributeRequirements Requirements;
        public readonly AttributeScaling Scaling;
        public readonly string SkillName;
        public readonly int SkillFocusCost;

        public ItemStatSnapshot(
            int physicalAttack,
            int magicAttack,
            int fireAttack,
            int lightningAttack,
            int holyAttack,
            int critical,
            float physicalGuard,
            float magicGuard,
            float fireGuard,
            float lightningGuard,
            float holyGuard,
            float guardBoost,
            AttributeRequirements requirements,
            AttributeScaling scaling,
            string skillName,
            int skillFocusCost)
        {
            PhysicalAttack = physicalAttack;
            MagicAttack = magicAttack;
            FireAttack = fireAttack;
            LightningAttack = lightningAttack;
            HolyAttack = holyAttack;
            Critical = critical;
            PhysicalGuard = physicalGuard;
            MagicGuard = magicGuard;
            FireGuard = fireGuard;
            LightningGuard = lightningGuard;
            HolyGuard = holyGuard;
            GuardBoost = guardBoost;
            Requirements = requirements;
            Scaling = scaling;
            SkillName = skillName;
            SkillFocusCost = skillFocusCost;
        }

        public static ItemStatSnapshot Empty => new ItemStatSnapshot(
            0, 0, 0, 0, 0, 0,
            0f, 0f, 0f, 0f, 0f, 0f,
            default, default, string.Empty, 0);
    }
}
