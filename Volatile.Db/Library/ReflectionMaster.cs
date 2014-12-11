using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Volatile.Db.Workers;

namespace Volatile.Db.Library
{
    public static class ReflectionMaster
    {
        //meant to replace the InputPrac method of saving data like such.
        //will create files that look as such:
        // propertyName&"stringvalue"
        // propertyName2&[firstobjectinarray,true,1,"last"] Will check each line for spaces count until the next closing bracket.
        // propertyNameThatIsAnotherReflectedObject$=>Object.TypeName.InLibrary
        //   secondPropertyName&value
        // <=
        // nextPropertyNameInFirstObject&SomeRandomValue
        //          Checking new object reflections will create an indentation. If the property value equals
        //          "=>" then it'll start scanning the next few lines until "<="
        public static string CreateReflectionMapFromObject(object obj, bool isEmbedded = false)
        {
            var builder = new StringBuilder();

            if (isEmbedded) builder.AppendLine("=>");
            foreach (var property in obj.GetType().GetProperties())
            {
                var valueToPass = property.GetValue(obj, null);

                if (property.PropertyType.BaseType == typeof (Volatile))
                    valueToPass = CreateReflectionMapFromObject(valueToPass, true);  // creates the reflected object

                else if (property.PropertyType == typeof (String)) valueToPass = "\"" + valueToPass + "\""; // creates a string

                else if (property.PropertyType.IsEnumerableType()) valueToPass = valueToPass.ArrayStringFromObject();  // array-like objects

                else if (property.PropertyType.IsEnum) valueToPass = (int)valueToPass; // creates the int the enum references

                builder.AppendLine(String.Format("{0}&{1}", property.Name, valueToPass));
            }

            if (isEmbedded) builder.AppendLine("\r\n<=");

            return builder.ToString();
        }

        public static dynamic GetDynObjectFromLines(string[] input, Type type)
        {
            var output = Activator.CreateInstance(type);
            var i = 0;
            foreach (var line in input)
            {
                if (line.StartsWith("<=")) return output;

                var name = line.Split('&')[0];
                var property = output.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                var value = line.Split('&')[1];

                var eo = value.StartsWith("=>")
                    ? GetDynObjectFromLines(input, GetTypeFromString(value.Replace("=>", "")))
                    : Converter.ChangeType(value, property.PropertyType);
                property.SetValue(output, eo);
                i++;
            }

            return output;
        }

        public static void ReadReflectionMapToObject(string fileName, out dynamic output)
        {
            var className = fileName.Split('\\')[fileName.Split('\\').Length - 1].Split('_')[0];
            var type = GetTypeFromString(className);

            output = Activator.CreateInstance(type);
            var lines = File.ReadAllLines(fileName);
            output = (Volatile) GetDynObjectFromLines(lines, type);
            
            output.OID = Int64.Parse(fileName.Replace(".vdb", "").Split('_')[1]);
        }

        public static Type GetTypeFromString(string input)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(input))
                    .First(t => t != null);
        }
    }
}
