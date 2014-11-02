using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volatile.Db.Workers;

namespace Volatile.Db
{
    public class Engine
    {
        private static Engine _instance { get; set; }
        internal Stack<DatabaseObject> Stack { get; set; }
        internal string Location { get; set; }

        public static Engine Instance
        {
            get { return _instance ?? (_instance = new Engine(AppDomain.CurrentDomain.BaseDirectory + "\\Volatile")); }
        }

        public Engine(string directory)
        {
            Stack = new Stack<DatabaseObject>();
            Location = directory ?? AppDomain.CurrentDomain.BaseDirectory + "\\Volatile";
            if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);
            if (!Directory.Exists(Location + "\\skeletons")) Directory.CreateDirectory(Location + "\\skeletons");
            //InputPrac.Initialize();
            foreach (var file in Directory.EnumerateFiles(Location, "*.vdb"))
            {
                object obj;
                InputPrac.FromFile(file, out obj);
                Stack.Push(new DatabaseObject(file.Replace(".vdb", "").Split('_')[1], obj));
            }
        }

        public IEnumerable<object> Get()
        {
            return Stack.Select(s => s.Value).ToArray();
        }

        public IEnumerable<T> Get<T>()
        {
            return Stack.Select(s => s.Value).Where(i => i.GetType() == typeof (T)).Cast<T>();
        }

        public IEnumerable<object> Get(Func<dynamic, bool> func)
        {
            return Stack.Where(i => func(i.Value)).Select(i => i.Value);
        }

        public IEnumerable<T> Get<T>(Func<object, bool> func)
        {
            return Stack.Where(i => func(i.Value) && i.Value.GetType() == typeof (T)).Cast<T>();
        } 

        public void Commit()
        {
            foreach (var item in Stack) InputPrac.ToFile(item);
        }

        public void CommitTop()
        {
            InputPrac.ToFile(Stack.Peek());
        }

        public void Commit(Func<object, bool> function)
        {
            foreach (var item in Stack.Where(item => function(item.Value))) InputPrac.ToFile(item);
        }

        public void Add(dynamic item, bool commit = true)
        {
            //Stack.Push(new DatabaseObject(HashGenerator.Next(), item));
            if (Stack.Any(i => (i.Value as dynamic).OID == item.OI)) Update(i => i == item, commit);
            else Stack.Push(new DatabaseObject(HashGenerator.Next(), item));
        }

        public void Remove(dynamic item, bool commit = true)
        {
            var s = new Stack<DatabaseObject>();
            while (Stack.Count > 0)
            {
                dynamic obj = Stack.Pop();
                if (item.OID != obj.Value.OID) s.Push(obj);
                else
                {
                    foreach (var n in s) Stack.Push(n);
                    return;
                }
                foreach (var n in s) Stack.Push(n);
            }
            if (commit) Commit();
        }

        public void Remove(Func<dynamic, bool> func, bool commit = true)
        {
            var s = new Stack<DatabaseObject>();
            while (Stack.Count > 0)
            {
                dynamic obj = Stack.Pop();
                if (!func(obj.Value.OID)) s.Push(obj);
                else
                {
                    foreach (var n in s) Stack.Push(n);
                    return;
                }
                foreach (var n in s) Stack.Push(n);
            }
            if (commit) Commit();
        }

        public void Update(Func<dynamic, bool> func, bool commit = true)
        {
            var item = Get(func);
            Remove(item, commit);
            Add(item, commit);
        }
    }
}
