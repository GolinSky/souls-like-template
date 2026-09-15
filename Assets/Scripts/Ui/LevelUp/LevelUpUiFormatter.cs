using System;
using System.Globalization;
using SoulsLike.Entities.Character;
using UnityEngine;

namespace SoulsLike.Ui.LevelUp
{
    public static class LevelUpUiFormatter
    {
        public const string INCREASED_COLOR_HEX = "7CB5EC";
        public const string UNAFFORDABLE_COLOR_HEX = "E05252";
        public const string DEFAULT_TEXT_COLOR_HEX = "DDD6C8";

        public static int CalculateLevel(CharacterAttributeStats stats)
        {
            int sum = stats.Vigor + stats.Mind + stats.Endurance + stats.Strength
                + stats.Dexterity + stats.Intelligence + stats.Faith + stats.Arcane;
            return Mathf.Max(1, sum - 79);
        }

        public static int CalculateRuneCost(int currentLevel)
        {
            float x = Mathf.Max(0f, (currentLevel + 81 - 92) * 0.02f);
            float baseCost = (x + 0.1f) * Mathf.Pow(currentLevel + 81, 2);
            return Mathf.FloorToInt(baseCost) + 1;
        }

        public static int CalculateTotalRuneCost(int currentLevel, int pointsToAllocate)
        {
            if (pointsToAllocate <= 0)
            {
                return 0;
            }

            int totalCost = 0;
            for (int i = 0; i < pointsToAllocate; i++)
            {
                totalCost = checked(totalCost + CalculateRuneCost(currentLevel + i));
            }

            return totalCost;
        }

        public static string FormatWholeNumber(int value)
        {
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static string FormatWholeNumber(float value)
        {
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static string FormatDecimal(float value, int decimalPlaces = 1)
        {
            string format = decimalPlaces == 1 ? "F1" : "F0";
            return value.ToString(format, CultureInfo.CurrentCulture);
        }

        public static string FormatNextValue(int current, int next)
        {
            if (next > current)
            {
                return $"<color=#{INCREASED_COLOR_HEX}>{next.ToString("N0", CultureInfo.CurrentCulture)}</color>";
            }

            return next.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static string FormatNextValue(float current, float next, int decimalPlaces = 0)
        {
            string format = decimalPlaces == 1 ? "F1" : "N0";
            if (next > current)
            {
                return $"<color=#{INCREASED_COLOR_HEX}>{next.ToString(format, CultureInfo.CurrentCulture)}</color>";
            }

            return next.ToString(format, CultureInfo.CurrentCulture);
        }

        public static string FormatRunesNeeded(int runesNeeded, bool isAffordable)
        {
            string formatted = runesNeeded.ToString("N0", CultureInfo.CurrentCulture);
            if (!isAffordable && runesNeeded > 0)
            {
                return $"<color=#{UNAFFORDABLE_COLOR_HEX}>{formatted}</color>";
            }

            return formatted;
        }

        public static float CalculateProjectedHealth(float baseHealth, int currentVigor, int nextVigor)
        {
            const float healthPerVigor = 20f;
            return baseHealth + (nextVigor - currentVigor) * healthPerVigor;
        }

        public static float CalculateProjectedFocus(float baseFocus, int currentMind, int nextMind)
        {
            const float focusPerMind = 3f;
            return baseFocus + (nextMind - currentMind) * focusPerMind;
        }

        public static float CalculateProjectedStamina(float baseStamina, int currentEndurance, int nextEndurance)
        {
            const float staminaPerEndurance = 1.5f;
            return baseStamina + (nextEndurance - currentEndurance) * staminaPerEndurance;
        }

        public static float CalculateProjectedEquipLoad(int endurance)
        {
            return 45f + endurance * 1.5f;
        }
    }
}
