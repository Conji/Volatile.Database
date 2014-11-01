using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Volatile.Db.Workers
{
    public static class HashGenerator
    {
        public static string Next(string seed = null)
        {
            return _softGenerateId(10);
        }
        private static Random _random = new Random(DateTime.Now.Millisecond / (int)DateTime.Now.Ticks);

        private static string _softGenerateId(int size)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65));
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
