using UnityEngine;

namespace SoulsLike.Ui.Inventory.Data
{
    public enum ScalingGrade
    {
        None,
        E,
        D,
        C,
        B,
        A,
        S
    }

    [CreateAssetMenu(fileName = "NewInventoryItem", menuName = "SoulsLike/UI/Inventory Item")]
    public class InventoryItemSO : ScriptableObject
    {
        [Header("Basic Information")]
        public string itemId;
        public string itemName;
        public Sprite itemIcon;
        public InventoryPrimaryCategory primaryCategory;
        public InventorySubCategory subCategory;
        public string itemTypeLabel = "Colossal Sword";
        public float weight = 15.5f;
        public bool isStackable = false;
        public int maxStackSize = 99;

        [Header("Offensive Power")]
        public int physicalAttack = 145;
        public int magicAttack = 0;
        public int fireAttack = 0;
        public int lightningAttack = 0;
        public int holyAttack = 0;
        public int critical = 100;

        [Header("Defensive Negation & Guard")]
        public float physicalGuard = 65.0f;
        public float magicGuard = 40.0f;
        public float fireGuard = 40.0f;
        public float lightningGuard = 35.0f;
        public float holyGuard = 40.0f;
        public float guardBoost = 48.0f;

        [Header("Attribute Requirements")]
        public int reqStrength = 31;
        public int reqDexterity = 12;
        public int reqIntelligence = 0;
        public int reqFaith = 0;
        public int reqArcane = 0;

        [Header("Attribute Scaling")]
        public ScalingGrade scaleStrength = ScalingGrade.C;
        public ScalingGrade scaleDexterity = ScalingGrade.E;
        public ScalingGrade scaleIntelligence = ScalingGrade.None;
        public ScalingGrade scaleFaith = ScalingGrade.None;
        public ScalingGrade scaleArcane = ScalingGrade.None;

        [Header("Skill / Ash of War")]
        public string skillName = "Stamp (Upward Cut)";
        public Sprite skillIcon;
        public int fpCost = 15;

        [Header("Lore & Description")]
        [TextArea(3, 6)]
        public string effectDescription = "A massive iron broadsword with a heavy blade designed to slice down armored beasts.";
        
        [TextArea(5, 10)]
        public string loreDescription = "One of the great weapons forged in the era of conflict. Designed to be wielded only by champions of extraordinary physical prowess.";
    }
}
