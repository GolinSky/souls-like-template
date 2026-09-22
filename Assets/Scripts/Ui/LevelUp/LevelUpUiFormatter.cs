using System.Globalization;

namespace SoulsLike.Ui.LevelUp
{
    /// <summary>Formats already calculated level-up values for the view.</summary>
    public static class LevelUpUiFormatter
    {
        public const string INCREASED_COLOR_HEX = "7CB5EC";
        public const string UNAFFORDABLE_COLOR_HEX = "E05252";
        public const string DEFAULT_TEXT_COLOR_HEX = "DDD6C8";

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

    }
}
