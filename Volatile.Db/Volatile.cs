using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volatile.Db
{
    public class Volatile : DynamicObject
    {
        public Int64 OID { get; set; }
    }
}
