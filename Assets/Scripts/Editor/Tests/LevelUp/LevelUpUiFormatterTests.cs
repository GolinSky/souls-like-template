#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Entities.Character;
using SoulsLike.Ui.LevelUp;

namespace SoulsLike.Editor.Tests.LevelUp
{
    public sealed class LevelUpUiFormatterTests
    {
        [Test]
        public void CalculateLevel_MatchesStartingClassAndScreenshotValues()
        {
            var startingStats = new CharacterAttributeStats(10, 10, 10, 10, 10, 10, 10, 10);
            int startingLevel = LevelUpUiFormatter.CalculateLevel(startingStats);
            Assert.That(startingLevel, Is.EqualTo(1));

            // Reference screenshot stats: 38, 10, 21, 18, 18, 23, 9, 7
            var screenshotStats = new CharacterAttributeStats(38, 10, 21, 18, 18, 23, 9, 7);
            int screenshotLevel = LevelUpUiFormatter.CalculateLevel(screenshotStats);
            Assert.That(screenshotLevel, Is.EqualTo(65));
        }

        [Test]
        public void CalculateRuneCost_MatchesExactEldenRingScreenshot()
        {
            // Level 65 in screenshot requires 25,153 runes to reach level 66
            int costAt65 = LevelUpUiFormatter.CalculateRuneCost(65);
            Assert.That(costAt65, Is.EqualTo(25153));
        }

        [Test]
        public void CalculateTotalRuneCost_CalculatesCumulativeCostsCorrectly()
        {
            int zeroCost = LevelUpUiFormatter.CalculateTotalRuneCost(65, 0);
            Assert.That(zeroCost, Is.EqualTo(0));

            int singleLevelCost = LevelUpUiFormatter.CalculateTotalRuneCost(65, 1);
            Assert.That(singleLevelCost, Is.EqualTo(25153));

            int twoLevelCost = LevelUpUiFormatter.CalculateTotalRuneCost(65, 2);
            int expectedTwoLevelCost = LevelUpUiFormatter.CalculateRuneCost(65) + LevelUpUiFormatter.CalculateRuneCost(66);
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
            float hp = LevelUpUiFormatter.CalculateProjectedHealth(1000f, 10, 20);
            Assert.That(hp, Is.EqualTo(1200f));

            float fp = LevelUpUiFormatter.CalculateProjectedFocus(100f, 10, 15);
            Assert.That(fp, Is.EqualTo(115f));

            float stamina = LevelUpUiFormatter.CalculateProjectedStamina(100f, 10, 14);
            Assert.That(stamina, Is.EqualTo(106f));

            float equipLoad = LevelUpUiFormatter.CalculateProjectedEquipLoad(20);
            Assert.That(equipLoad, Is.EqualTo(75f));
        }
    }
}
#endif
