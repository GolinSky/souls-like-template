using System.Globalization;
using SoulsLike.Items;

namespace SoulsLike.Ui.Status
{
    public static class StatusUiFormatter
    {
        public const string UNAVAILABLE_VALUE = "—";

        public static string FormatCurrentAndMaximum(float current, float maximum)
        {
            return $"{FormatWholeNumber(current)} / {FormatWholeNumber(maximum)}";
        }

        public static string FormatEquipLoad(float current, float maximum)
        {
            return $"{current.ToString("F1", CultureInfo.CurrentCulture)} / "
                + maximum.ToString("F1", CultureInfo.CurrentCulture);
        }

        public static string FormatWholeNumber(float value)
        {
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static string FormatWholeNumber(int value)
        {
            return value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public static int GetTotalAttack(ItemStatSnapshot stats)
        {
            return stats.PhysicalAttack
                + stats.MagicAttack
                + stats.FireAttack
                + stats.LightningAttack
                + stats.HolyAttack;
        }
    }
}
