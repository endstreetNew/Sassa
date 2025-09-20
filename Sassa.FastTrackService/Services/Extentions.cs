using System.Globalization;

namespace Sassa.FastTrackService.Services
{
    public static class Extentions
    {
        public static bool IsNumeric(this string text)
        {
            double result;
            return double.TryParse(text, out result);
        }
        public static DateTime ToDate(this string date, string fromFormat)
        {
            if (string.IsNullOrEmpty(date)) return DateTime.Now;
            return DateTime.ParseExact(date, fromFormat, CultureInfo.InvariantCulture);
        }
    }
}
