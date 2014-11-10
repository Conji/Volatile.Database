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
            return _softGenerateId(8);
        }
        private static Random _random = new Random(DateTime.Now.Millisecond / (int)DateTime.Now.Ticks);

        private static string _softGenerateId(int size)
        {
            var rand = new Random(DateTime.Now.Millisecond);
            return rand.Next(0, 999999999).ToString().Substring(0, size);
        }
    }
}
