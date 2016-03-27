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

        public static string Serialize(object value, EntityDataType dataType)
        {
            if (value == null)
                return "";
            if (value is string)
                return (string)value;
            if (value is DateTime)
            {
                switch (dataType)
                {
                    case EntityDataType.Date:
                        return ApiUtils.PrintDate((DateTime)value);
                    case EntityDataType.DateTime:
                        return ApiUtils.PrintDateTime((DateTime)value);
                    case EntityDataType.DateTimeTz:
                        return ApiUtils.PrintDateTimeOffset((DateTime)value);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
            if (value is DateTimeOffset)
            {
                switch (dataType)
                {
                    case EntityDataType.Date:
                        return ApiUtils.PrintDate((DateTimeOffset)value);
                    case EntityDataType.DateTime:
                        return ApiUtils.PrintDateTime((DateTimeOffset)value);
                    case EntityDataType.DateTimeTz:
                        return ApiUtils.PrintDateTimeOffset((DateTimeOffset)value);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
            if (value is int)
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            if (value is long)
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            if (value is float)
                return ((float)value).ToString(CultureInfo.InvariantCulture);
            if (value is double)
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            if (value is decimal)
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
