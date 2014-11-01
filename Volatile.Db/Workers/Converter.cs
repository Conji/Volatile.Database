using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Volatile.Db.Workers
{
    public static class Converter
    {
        public static object ChangeType(string value, Type targetType)
        {
            if (String.IsNullOrEmpty(value)) return null;
            if (targetType == typeof (bool)) return value.ToLower() == "true";
            if (targetType == typeof (int)) return int.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (Array)) return new object[] {};
            if (targetType.IsEnum) return Enum.ToObject(targetType, String.IsNullOrEmpty(value) ? 0 : Int32.Parse(value.Trim(), NumberStyles.Any));
           
            return value;
        }

        public static int FromString(this string input)
        {
            return !input.Any() ? 0 : int.Parse(new StringBuilder().Append(input.Select(Char.IsNumber)).ToString());
        }

        public static T EnumParse<T>(this string value, bool ignoreCase)
        {
            if (value == null) throw new ArgumentNullException("value");
            value.Trim();
            if (value.Length == 0) throw new ArgumentException("Must specify valid information for parsing in the string.", "value");
            var type = typeof (T);
            if (!type.IsEnum) throw new ArgumentException("Type provided must be an Enum.", "T");
            return (T) Enum.Parse(type, value, ignoreCase);
        }
    }
}
