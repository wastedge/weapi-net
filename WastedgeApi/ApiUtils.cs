using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    internal static class ApiUtils
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = DateFormat + "'T'HH:mm:ss.fff";
        private const string DateTimeTzFormat = DateTimeFormat + "zzz";

        public static DateTime? ParseDate(string value)
        {
            if (value == null)
                return null;

            return DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime? ParseDateTime(string value)
        {
            if (value == null)
                return null;

            return DateTime.ParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset? ParseDateTimeOffset(string value)
        {
            if (value == null)
                return null;

            return DateTimeOffset.ParseExact(value, DateTimeTzFormat, CultureInfo.InvariantCulture);
        }

        public static string PrintDate(DateTime? value)
        {
            return value?.ToString(DateFormat);
        }

        public static string PrintDateTime(DateTime? value)
        {
            return value?.ToString(DateTimeFormat);
        }

        public static string PrintDateTimeOffset(DateTime? value)
        {
            return value?.ToString(DateTimeTzFormat);
        }

        public static string PrintDate(DateTimeOffset? value)
        {
            return value?.ToString(DateFormat);
        }

        public static string PrintDateTime(DateTimeOffset? value)
        {
            return value?.ToString(DateTimeFormat);
        }

        public static string PrintDateTimeOffset(DateTimeOffset? value)
        {
            return value?.ToString(DateTimeTzFormat);
        }
    }
}
