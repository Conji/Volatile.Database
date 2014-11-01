using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Volatile.Db.Workers
{
    internal static class InputPrac
    {
        public static List<Type> Types { get; set; }

        public static void Initialize()
        {
            
            Types = Directory.GetFiles(Engine.Instance.Location + "\\skeletons")
                .Select(file => Type.GetType(file.Replace(".vdbs", ""), true, true))
                .ToList();
        }

        public static object FromFile(string fileName, out object output)
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
                var name = l.Split("::".ToCharArray())[0];

                var property = output.GetType().GetProperty(name, BindingFlags.CreateInstance);
                var value = l.Split("::".ToCharArray())[1];
                property.SetValue(output, Convert.ChangeType(value, property.PropertyType), null);
            }
            return output;
        }

        public static void ToFile(DatabaseObject input)
        {
            var directory = Engine.Instance.Location;
            var thread = new Thread(() =>
            {
                if (!File.Exists(String.Format(@"{0}\skeletons\{1}.vdbs", Engine.Instance.Location,
                    input.Value.GetType().FullName))) CreateSkeleton(input.Value.GetType());
                using (var writer = File.OpenWrite(String.Format(@"{0}\{1}_{2}.vdb", directory, input.Value.GetType().FullName,input.Key)))
                {
                    foreach (
                        var s in
                            input.Value.GetType()
                                .GetProperties()
                                .Select(
                                    property =>
                                        property.Name + "::" + property.GetValue(input.Value) + Environment.NewLine))
                        writer.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
                }
            });
            thread.Start();
        }

        public static void CreateSkeleton(Type type)
        {
            using (var writer = File.OpenWrite(String.Format(@"{0}\skeletons\{1}.vdbs", Engine.Instance.Location, type.FullName)))
            {
                foreach (
                    var s in
                        type.GetProperties().Select(p => p.Name + "::" + p.PropertyType.FullName + Environment.NewLine))
                    writer.Write(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s));
            }
        }

        public static Type GetPropertyTypeFromSkeleton(string typeName, string propertyName)
        {
            return
                (from line in
                    File.ReadAllLines(String.Format(@"{0}\skeletons\{1}.vdbs".ToLower(), Engine.Instance.Location,
                        typeName.ToLower()))
                    let p = line.Split("::".ToCharArray())[0].ToLower()
                    let v = line.Split("::".ToCharArray())[1].ToLower()
                    where p == propertyName.ToLower()
                    select Type.GetType(v, true, true)).FirstOrDefault();
        }

        public static bool DoesKeyExist(string directoryLocation, string key)
        {
            return
                Directory.GetFiles(directoryLocation, "*.vdb")
                    .Any(f => f.Split('_')[f.Split('_').Length - 1].EndsWith(key + ".vdb"));
        }
    }
}
