#if UNITY_EDITOR
using System.Globalization;
using NUnit.Framework;
using SoulsLike.Items;
using SoulsLike.Ui.Status;

namespace SoulsLike.Editor.Tests.Status
{
    public sealed class StatusUiFormatterTests
    {
        private CultureInfo _previousCulture;

        [SetUp]
        public void SetUp()
        {
            _previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
        }

        [TearDown]
        public void TearDown()
        {
            CultureInfo.CurrentCulture = _previousCulture;
        }

        [Test]
        public void FormatCurrentAndMaximum_UsesRoundedWholeValues()
        {
            string value = StatusUiFormatter.FormatCurrentAndMaximum(1490f, 1490f);

            Assert.That(value, Is.EqualTo("1,490 / 1,490"));
        }

        [Test]
        public void FormatEquipLoad_PreservesOneDecimalPlace()
        {
            string value = StatusUiFormatter.FormatEquipLoad(13.4f, 65.6f);

            Assert.That(value, Is.EqualTo("13.4 / 65.6"));
        }

        [Test]
        public void FormatWholeNumber_IntPreservesLargeCurrencyValues()
        {
            string value = StatusUiFormatter.FormatWholeNumber(123456789);

            Assert.That(value, Is.EqualTo("123,456,789"));
        }

        [Test]
        public void GetTotalAttack_SumsAllDamageChannelsWithoutCriticalModifier()
        {
            var stats = new ItemStatSnapshot(
                physicalAttack: 100,
                magicAttack: 20,
                fireAttack: 30,
                lightningAttack: 40,
                holyAttack: 50,
                critical: 110,
                physicalGuard: 0f,
                magicGuard: 0f,
                fireGuard: 0f,
                lightningGuard: 0f,
                holyGuard: 0f,
                guardBoost: 0f,
                requirements: default,
                scaling: default,
                skillName: string.Empty,
                skillFocusCost: 0);

            Assert.That(StatusUiFormatter.GetTotalAttack(stats), Is.EqualTo(240));
        }
    }
}
#endif
