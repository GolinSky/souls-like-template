using SoulsLike.Entities.Character;
using TMPro;
using UnityEngine;

namespace SoulsLike.Ui.Inventory
{
    public sealed class CharacterStatsUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text vigorText;
        [SerializeField] private TMP_Text mindText;
        [SerializeField] private TMP_Text enduranceText;
        [SerializeField] private TMP_Text strengthText;
        [SerializeField] private TMP_Text dexterityText;
        [SerializeField] private TMP_Text intelligenceText;
        [SerializeField] private TMP_Text faithText;
        [SerializeField] private TMP_Text arcaneText;
        [SerializeField] private TMP_Text rightAttackText;
        [SerializeField] private TMP_Text leftAttackText;
        [SerializeField] private TMP_Text equipLoadText;
        [SerializeField] private TMP_Text poiseText;

        private static readonly Color ColorParchmentPrimary = new(0.902f, 0.882f, 0.773f);
        private static readonly Color ColorStatBuff = new(0.384f, 0.710f, 0.965f);
        private static readonly Color ColorStatNerf = new(0.937f, 0.325f, 0.314f);

        public void Display(
            Character character,
            float equipWeight,
            float maxEquipWeight,
            int rightAttack,
            int leftAttack)
        {
            CharacterAttributeStats attributes = character.Attributes;
            vigorText.text = attributes.Vigor.ToString();
            mindText.text = attributes.Mind.ToString();
            enduranceText.text = attributes.Endurance.ToString();
            strengthText.text = attributes.Strength.ToString();
            dexterityText.text = attributes.Dexterity.ToString();
            intelligenceText.text = attributes.Intelligence.ToString();
            faithText.text = attributes.Faith.ToString();
            arcaneText.text = attributes.Arcane.ToString();
            rightAttackText.text = rightAttack.ToString();
            rightAttackText.color = ColorParchmentPrimary;
            leftAttackText.text = leftAttack.ToString();
            equipLoadText.text = $"{equipWeight:F1} / {maxEquipWeight:F1}";
            poiseText.text = "0";
        }

        public void UpdateRightAttackComparison(int currentAttack, int candidateAttack)
        {
            int delta = candidateAttack - currentAttack;
            rightAttackText.text = delta switch
            {
                > 0 => $"{candidateAttack} (+{delta})",
                < 0 => $"{candidateAttack} ({delta})",
                _ => candidateAttack.ToString()
            };
            rightAttackText.color = delta switch
            {
                > 0 => ColorStatBuff,
                < 0 => ColorStatNerf,
                _ => ColorParchmentPrimary
            };
        }
    }
}
