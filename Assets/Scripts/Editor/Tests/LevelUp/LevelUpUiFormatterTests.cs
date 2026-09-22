#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Ui.LevelUp;

namespace SoulsLike.Editor.Tests.LevelUp
{
    /// <summary>Verifies presentation formatting and pure progression calculations.</summary>
    public sealed class LevelUpUiFormatterTests
    {
        [Test]
        public void CalculateLevel_MatchesStartingClassAndScreenshotValues()
        {
            var startingStats = new CharacterAttributeStats(10, 10, 10, 10, 10, 10, 10, 10);
            int startingLevel = CalculateLevel(startingStats);
            Assert.That(startingLevel, Is.EqualTo(1));

            // Reference screenshot stats: 38, 10, 21, 18, 18, 23, 9, 7
            var screenshotStats = new CharacterAttributeStats(38, 10, 21, 18, 18, 23, 9, 7);
            int screenshotLevel = CalculateLevel(screenshotStats);
            Assert.That(screenshotLevel, Is.EqualTo(65));
        }

        [Test]
        public void CalculateRuneCost_MatchesExactEldenRingScreenshot()
        {
            // Level 65 in screenshot requires 25,153 runes to reach level 66
            int costAt65 = CharacterProgressionRules.CalculateRuneCost(65);
            Assert.That(costAt65, Is.EqualTo(25153));
        }

        [Test]
        public void CalculateTotalRuneCost_CalculatesCumulativeCostsCorrectly()
        {
            int zeroCost = CharacterProgressionRules.CalculateTotalRuneCost(65, 0);
            Assert.That(zeroCost, Is.EqualTo(0));

            int singleLevelCost = CharacterProgressionRules.CalculateTotalRuneCost(65, 1);
            Assert.That(singleLevelCost, Is.EqualTo(25153));

            int twoLevelCost = CharacterProgressionRules.CalculateTotalRuneCost(65, 2);
            int expectedTwoLevelCost = CharacterProgressionRules.CalculateRuneCost(65) + CharacterProgressionRules.CalculateRuneCost(66);
            Assert.That(twoLevelCost, Is.EqualTo(expectedTwoLevelCost));
        }

        [Test]
        public void FormatNextValue_AppliesColorMarkupOnlyWhenIncreased()
        {
            string unchanged = LevelUpUiFormatter.FormatNextValue(10, 10);
            Assert.That(unchanged, Does.Not.Contain("<color"));
            Assert.That(unchanged, Is.EqualTo("10"));

            string increased = LevelUpUiFormatter.FormatNextValue(10, 15);
            Assert.That(increased, Does.Contain($"<color=#{LevelUpUiFormatter.INCREASED_COLOR_HEX}>15</color>"));
        }

        [Test]
        public void FormatRunesNeeded_AppliesRedColorWhenUnaffordable()
        {
            string affordable = LevelUpUiFormatter.FormatRunesNeeded(5000, true);
            Assert.That(affordable, Does.Not.Contain("<color"));

            string unaffordable = LevelUpUiFormatter.FormatRunesNeeded(5000, false);
            Assert.That(unaffordable, Does.Contain($"<color=#{LevelUpUiFormatter.UNAFFORDABLE_COLOR_HEX}>"));
        }

        [Test]
        public void ProjectedStats_CalculateExpectedValues()
        {
            float hp = CharacterProgressionRules.CalculateProjectedHealth(1000f, 10, 20);
            Assert.That(hp, Is.EqualTo(1200f));

            float fp = CharacterProgressionRules.CalculateProjectedFocus(100f, 10, 15);
            Assert.That(fp, Is.EqualTo(115f));

            float stamina = CharacterProgressionRules.CalculateProjectedStamina(100f, 10, 14);
            Assert.That(stamina, Is.EqualTo(106f));

            float equipLoad = CharacterProgressionRules.CalculateProjectedEquipLoad(20);
            Assert.That(equipLoad, Is.EqualTo(75f));
        }

        private static int CalculateLevel(CharacterAttributeStats stats)
        {
            return CharacterProgressionRules.CalculateLevel(
                stats.Vigor, stats.Mind, stats.Endurance, stats.Strength,
                stats.Dexterity, stats.Intelligence, stats.Faith, stats.Arcane);
        }
    }
}
#endif
