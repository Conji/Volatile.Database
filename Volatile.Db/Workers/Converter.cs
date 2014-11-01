using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Volatile.Db.Workers
{
    public static class Converter
    {
        public static object ChangeType(string value, Type targetType)
        {
            if (targetType == typeof (bool)) return value.ToLower() == "true";
            else if (targetType == typeof (int)) return value.FromString();
            else if (targetType == typeof (string)) return value;
            else if (targetType == typeof (Array)) return value;
            // TODO: GET FUCKING ENUMS WORKING BECAUSE BULLSHIT

            return value;
        }

        public static int FromString(this string input)
        {
            return !input.Any() ? 0 : int.Parse(new StringBuilder().Append(input.Select(Char.IsNumber)).ToString());
        }

    }
}
