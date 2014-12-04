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
        internal List<DatabaseObject> Stack { get; set; }
        internal string Location { get; set; }

        public static Engine Instance
        {
            get { return _instance ?? (_instance = new Engine(AppDomain.CurrentDomain.BaseDirectory + "\\Volatile")); }
        }

        public Engine(string directory)
        {
            Stack = new List<DatabaseObject>();
            Location = directory ?? AppDomain.CurrentDomain.BaseDirectory + "\\Volatile";
            if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);
            foreach (var file in Directory.EnumerateFiles(Location, "*.vdb"))
            {
                object obj;
                InputPrac.FromFile(file, out obj);
                Stack.Add(new DatabaseObject(file.Replace(".vdb", "").Split('_')[1], obj));
            }
        }

        /// <summary>
        /// Returns the stack inside the database.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> Get()
        {
            return Stack.Select(s => s.Value).ToArray();
        }

        /// <summary>
        /// Returns items of the specific type inside the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> Get<T>()
        {
            return Stack.Select(s => s.Value).Where(i => i.GetType() == typeof (T)).Cast<T>();
        }

        public IEnumerable<dynamic> Get(Predicate<dynamic> func)
        {
            return Stack.Select(s => s.Value).Where(i => func(i));
        }

        public IEnumerable<T> Get<T>(Predicate<dynamic> func)
        {
            return Stack.Select(s => s.Value).Where(i => func(i) && i.GetType() == typeof (T)).Cast<T>();
        } 

        /// <summary>
        /// Commits all items from the database to file.
        /// </summary>
        public void Commit()
        {
            foreach (var item in Stack) InputPrac.ToFile(item);
        }

        /// <summary>
        /// Commits the item in the stack with the OID provided.
        /// </summary>
        /// <param name="oid">OID of the committing object</param>
        /// <param name="method">use "put" or "delete"</param>
        public void Commit(long oid, string method = "put")
        {
            Commit(Stack.Find(s => s.Key == oid.ToString()));
        }

        /// <summary>
        /// Commits the specific item from the database.
        /// </summary>
        /// <param name="toBeCommitted"></param>
        /// <param name="method">use "put" or "delete"</param>
        public void Commit(dynamic toBeCommitted, string method = "put")
        {
            switch (method.ToLower())
            {
                case "put":
                    foreach (var item in Stack.Where(item => toBeCommitted.OID.ToString() == item.Key)) InputPrac.ToFile(item);
                    break;
                case "delete":
                    foreach (var item in Stack.Where(item => toBeCommitted == item.Value))
                        File.Delete(Directory.EnumerateFiles(Location).First(i => i.Contains(item.Key)));
                    break;
            }
        }

        /// <summary>
        /// Commits the specific DatabaseObject from the database.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="method">use "put" or "delete"</param>
        public void Commit(DatabaseObject db, string method = "put")
        {
            switch (method.ToLower())
            {
                case "put":
                    InputPrac.ToFile(db);
                    break;
                case "delete":
                    Commit(db.Value, method);
                    break;
            }
        }

        /// <summary>
        /// Adds the item to the database then returns the item with it's OID.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public Volatile Add(dynamic item, bool commit = true)
        {
            dynamic n = (Volatile)item;
            var it = long.Parse(HashGenerator.Next());
            n.OID = it;
            var db = new DatabaseObject(it.ToString(), n);
            Stack.Add(db);
            Commit(db);

            return db.Value;
        }

        /// <summary>
        /// Removes the item from the database.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="commit"></param>
        public void Remove(dynamic item, bool commit = true)
        {
            foreach (var file in Directory.EnumerateFiles(Location).Where(file => file.Contains(item.OID.ToString())))
            {
                File.Delete(file);
            }
            Stack.RemoveAll(i => ((Volatile) i.Value).OID == item.OID);
        }

        /// <summary>
        /// Removes the item that matches the function from the database.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="commit"></param>
        public void Remove(Predicate<dynamic> func, bool commit = true)
        {
            foreach (var file in Directory.EnumerateFiles(Location).Where(file => func(file)))
            {
                File.Delete(file);
            } 
            var i = Stack.First(ind => func(ind));
            Stack.RemoveAll(ind => i.Equals(ind));
        }

        /// <summary>
        /// Updates the specific item in the database using matching OIDs.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="commit"></param>
        public void Update(dynamic obj, bool commit = true)
        {
            var i = 0;
            while (i < Stack.Count)
            {
                if (Stack[i].Value.OID.ToString() == obj.OID.ToString())
                {
                    var item = Stack[i];
                    item.Value = obj;
                    Stack[i] = item;
                    break;
                }
                i++;
            }
            if (commit) Commit(obj);
        }

        public void Update(string oid, dynamic obj, bool commit = true)
        {
            var i = 0;
            while (i < Stack.Count)
            {
                if (Stack[i].Key == oid)
                {
                    var item = Stack[i];
                    item.Value = obj;
                    Stack[i] = item;
                    break;
                }
                i++;
            }
            if (commit) Commit(obj);
        }
    }
}
