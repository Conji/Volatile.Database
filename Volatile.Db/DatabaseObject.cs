using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volatile.Db
{
    public struct DatabaseObject<T>
    {
        public string Key;
        public T Value;

        public DatabaseObject(string key, T value)
        {
            Key = key;
            Value = value;
        }
    }

    public struct DatabaseObject
    {
        public string Key;
        public dynamic Value;

        public DatabaseObject(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
