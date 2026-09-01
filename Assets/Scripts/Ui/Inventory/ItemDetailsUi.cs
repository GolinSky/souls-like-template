using SoulsLike.Entities.Character;
using SoulsLike.Items;
using SoulsLike.Ui.Inventory.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Inventory
{
    public sealed class ItemDetailsUi : MonoBehaviour
    {
        public static readonly Color ColorParchmentPrimary = new(0.902f, 0.882f, 0.773f);
        public static readonly Color ColorUnmetRequirement = new(0.898f, 0.224f, 0.208f);

        [SerializeField] private Image detailItemArtwork;
        [SerializeField] private TMP_Text detailItemName;
        [SerializeField] private TMP_Text detailItemType;
        [SerializeField] private TMP_Text detailItemWeight;
        [SerializeField] private TMP_Text detailAttackPhysical;
        [SerializeField] private TMP_Text detailAttackMagic;
        [SerializeField] private TMP_Text detailAttackFire;
        [SerializeField] private TMP_Text detailAttackLightning;
        [SerializeField] private TMP_Text detailAttackHoly;
        [SerializeField] private TMP_Text detailAttackCritical;
        [SerializeField] private TMP_Text detailGuardBoost;
        [SerializeField] private TMP_Text detailScaleStr;
        [SerializeField] private TMP_Text detailScaleDex;
        [SerializeField] private TMP_Text detailScaleInt;
        [SerializeField] private TMP_Text detailScaleFth;
        [SerializeField] private TMP_Text detailScaleArc;
        [SerializeField] private TMP_Text detailReqStr;
        [SerializeField] private TMP_Text detailReqDex;
        [SerializeField] private TMP_Text detailReqInt;
        [SerializeField] private TMP_Text detailReqFth;
        [SerializeField] private TMP_Text detailReqArc;
        [SerializeField] private TMP_Text detailSkillName;
        [SerializeField] private TMP_Text detailSkillFpCost;
        [SerializeField] private TMP_Text detailEffectDescription;

        public void Display(
            InventoryItemViewData item,
            CharacterAttributeStats attributes)
        {
            ItemStatSnapshot stats = item.Stats;
            detailItemArtwork.sprite = item.Icon;
            detailItemArtwork.enabled = item.Icon != null;
            detailItemName.text = item.DisplayName;
            detailItemType.text = item.ItemType.ToString();
            detailItemWeight.text = item.Weight.ToString("F1");
            detailAttackPhysical.text = stats.PhysicalAttack.ToString();
            detailAttackMagic.text = stats.MagicAttack.ToString();
            detailAttackFire.text = stats.FireAttack.ToString();
            detailAttackLightning.text = stats.LightningAttack.ToString();
            detailAttackHoly.text = stats.HolyAttack.ToString();
            detailAttackCritical.text = stats.Critical.ToString();
            detailGuardBoost.text = stats.GuardBoost.ToString("F0");
            detailScaleStr.text = FormatScaling(stats.Scaling.Strength);
            detailScaleDex.text = FormatScaling(stats.Scaling.Dexterity);
            detailScaleInt.text = FormatScaling(stats.Scaling.Intelligence);
            detailScaleFth.text = FormatScaling(stats.Scaling.Faith);
            detailScaleArc.text = FormatScaling(stats.Scaling.Arcane);
            SetRequirementField(detailReqStr, stats.Requirements.Strength, attributes.Strength);
            SetRequirementField(detailReqDex, stats.Requirements.Dexterity, attributes.Dexterity);
            SetRequirementField(detailReqInt, stats.Requirements.Intelligence, attributes.Intelligence);
            SetRequirementField(detailReqFth, stats.Requirements.Faith, attributes.Faith);
            SetRequirementField(detailReqArc, stats.Requirements.Arcane, attributes.Arcane);
            detailSkillName.text = string.IsNullOrWhiteSpace(stats.SkillName) ? "-" : stats.SkillName;
            detailSkillFpCost.text = stats.SkillFocusCost > 0 ? $"FP {stats.SkillFocusCost}" : "-";
            detailEffectDescription.text = item.Description;
        }

        private static string FormatScaling(SoulsLike.Items.ScalingGrade grade)
        {
            return grade == SoulsLike.Items.ScalingGrade.None ? "-" : grade.ToString();
        }

        private static void SetRequirementField(TMP_Text field, int requiredValue, int playerValue)
        {
            field.text = requiredValue > 0 ? requiredValue.ToString() : "-";
            field.color = playerValue < requiredValue
                ? ColorUnmetRequirement
                : ColorParchmentPrimary;
        }
    }
}
