using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Volatile.Db.Library;

namespace Volatile.Db.Workers
{
    internal static class InputPrac
    {
        public static object FromFile(string fileName, out dynamic output)
        {
            ReflectionMaster.ReadReflectionMapToObject(fileName, out output);
            return output;
        }

        public static void ToFile(DatabaseObject input)
        {
            var directory = Engine.Instance.Location;
            var thread = new Thread(() =>
            {
                File.Delete(String.Format(@"{0}\{1}_{2}.vdb", directory, input.Value.GetType().FullName,input.Key));
                using (var writer = File.OpenWrite(String.Format(@"{0}\{1}_{2}.vdb", directory, input.Value.GetType().FullName,input.Key)))
                {
                    writer.Write(Encoding.UTF8.GetBytes(ReflectionMaster.CreateReflectionMapFromObject(input.Value)), 0,
                        Encoding.UTF8.GetByteCount(ReflectionMaster.CreateReflectionMapFromObject(input.Value)));
                }
            });
            thread.Start();
        }

        public static bool DoesKeyExist(string directoryLocation, string key)
        {
            return
                Directory.GetFiles(directoryLocation, "*.vdb")
                    .Any(f => f.Split('_')[f.Split('_').Length - 1].EndsWith(key + ".vdb"));
        }
    }
}
