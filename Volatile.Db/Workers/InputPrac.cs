using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Volatile.Db.Library;

namespace Volatile.Db.Workers
{
    internal static class InputPrac
    {
        public static object FromFile(string fileName, out dynamic output)
        {
            var className = fileName.Split('\\')[fileName.Split('\\').Length - 1].Split('_')[0];
            var type =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(className))
                    .First(t => t != null);

            output = Activator.CreateInstance(type);
            var lines = File.ReadAllLines(fileName);
            foreach (var l in lines)
            {
                var name = l.Split('&')[0];
                var property = output.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                var value = l.Split('&')[1];

                property.SetValue(output, Converter.ChangeType(value, property.PropertyType));
            }
            output.OID = Int64.Parse(fileName.Replace(".vdb", "").Split('_')[1]);
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
                    foreach (PropertyInfo p in input.Value.GetType().GetProperties())
                    {
                        var pass = p.GetValue(input.Value, null);
                        //checks if enum and sets
                        if (p.PropertyType.IsEnum) pass = (int) pass;
                        // TODO: IEnumerables because they're being fgts.
                        if (p.PropertyType.IsEnumerableType() && p.PropertyType != typeof(String) && p.PropertyType != typeof(string)) pass = ((object) pass).ArrayStringFromObject();
                        var s = p.Name + "&" + pass + Environment.NewLine;
                        writer.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
                    }
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
