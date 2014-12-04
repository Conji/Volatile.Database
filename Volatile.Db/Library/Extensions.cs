using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volatile.Db.Library
{
    public static class Extensions
    {
        public static bool IsEnumerableType(this Type type)
        {
            return 
                type.GetInterfaces().Contains(typeof (IEnumerable<>)) ||
                type.GetInterfaces().Contains(typeof (IEnumerable));
        }
    }
}
