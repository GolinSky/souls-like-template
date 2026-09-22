namespace SoulsLike.Entities.Character.Runtime
{
    /// <summary>Owns pure stamina admission and attack-cost calculations while adapters retain storage.</summary>
    public static class CharacterStaminaPolicy
    {
        public static bool CanStart(
            float currentStamina,
            float maxStamina,
            float cost,
            float startThreshold)
        {
            if (cost <= 0f) return true;
            float threshold = startThreshold < -maxStamina
                ? -maxStamina
                : startThreshold > maxStamina
                    ? maxStamina
                    : startThreshold;
            return currentStamina > threshold;
        }

        public static float CalculateAttackCost(
            bool usesHeavyCost,
            float lightCost,
            float heavyCost,
            float multiplier)
        {
            return (usesHeavyCost ? heavyCost : lightCost) * multiplier;
        }

        public static float GetAttackStartThreshold(
            bool usesHeavyCost,
            float lightThreshold,
            float heavyThreshold)
        {
            return usesHeavyCost ? heavyThreshold : lightThreshold;
        }
    }
}
