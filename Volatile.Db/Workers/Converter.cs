using System;
using System.Collections;
using System.Collections.Generic;
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
            if (targetType == typeof (Int16)) return Int16.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (int)) return int.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (Int64)) return Int64.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (double)) return double.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (UInt16)) return UInt16.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (UInt32)) return UInt32.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (UInt64)) return UInt64.Parse(value.Trim(), NumberStyles.Any);
            if (targetType == typeof (IEnumerable)) return value.ToEnumerable();
            if (targetType.IsEnum) return Enum.ToObject(targetType, String.IsNullOrEmpty(value) ? 0 : Int32.Parse(value.Trim(), NumberStyles.Any));
           
            return value;
        }

        public static int FromString(this string input)
        {
            return !input.Any() ? 0 : int.Parse(new StringBuilder().Append(input.Select(Char.IsNumber)).ToString());
        }

        public static IEnumerable<string> ToEnumerable(this string value)
        {
            return value.Substring(1, value.Length - 2).Split(',');
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
