using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volatile.Db.Workers
{
    internal static class ObjectHelpers
    {
        public static object Parse(this string input)
        {
            var i = input.ToLower().Trim();
            if (i == "true" || i == "false")
            {
                if (i == "true") return true;
                if (i == "false") return false;
            }
            try
            {
                return Convert.ToInt32(i);
            }
            catch
            {
                
            }
            if (i.StartsWith("[") && i.EndsWith("]"))
            {
                var l = new ArrayList();
                foreach (var obj in i.Substring(1, i.Length - 2).Split(','))
                {
                    l.Add(obj.Trim().Parse());
                }
                return l.ToArray();
            }

            return input;
        }
    }
}
