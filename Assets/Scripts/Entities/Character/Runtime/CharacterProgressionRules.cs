using System;

namespace SoulsLike.Entities.Character.Runtime
{
    /// <summary>Calculates level costs and projected scalar progression without UI dependencies.</summary>
    public static class CharacterProgressionRules
    {
        public static int CalculateLevel(
            int vigor, int mind, int endurance, int strength,
            int dexterity, int intelligence, int faith, int arcane)
        {
            int sum = vigor + mind + endurance + strength + dexterity + intelligence + faith + arcane;
            return Math.Max(1, sum - 79);
        }

        public static int CalculateRuneCost(int currentLevel)
        {
            float x = Math.Max(0f, (currentLevel - 11) * 0.02f);
            float baseCost = (x + 0.1f) * (float)Math.Pow(currentLevel + 81, 2);
            return (int)Math.Floor(baseCost) + 1;
        }

        public static int CalculateTotalRuneCost(int currentLevel, int pointsToAllocate)
        {
            int totalCost = 0;
            for (int i = 0; i < pointsToAllocate; i++)
            {
                totalCost = checked(totalCost + CalculateRuneCost(currentLevel + i));
            }
            return totalCost;
        }

        public static float CalculateProjectedHealth(float baseHealth, int currentVigor, int nextVigor) =>
            baseHealth + (nextVigor - currentVigor) * 20f;

        public static float CalculateProjectedFocus(float baseFocus, int currentMind, int nextMind) =>
            baseFocus + (nextMind - currentMind) * 3f;

        public static float CalculateProjectedStamina(float baseStamina, int currentEndurance, int nextEndurance) =>
            baseStamina + (nextEndurance - currentEndurance) * 1.5f;

        public static float CalculateProjectedEquipLoad(int endurance) => 45f + endurance * 1.5f;
    }
}
