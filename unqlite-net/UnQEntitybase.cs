using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace UnQLiteNet
{
    /// <summary>
    /// Entities
    /// </summary>
    public class UnQEntitybase : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        public UnQEntitybase(string path, UnQMode mode)
        {
            if (mode == UnQMode.Open)
            {
                if (!System.IO.File.Exists(path))
                {
                    throw new Exception($"tsdb config '{path}' not exists");
                }
                unqlite = new UnQLite(path, UnQLiteOpenModel.ReadWrite);
            }
            if (mode == UnQMode.Override)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                unqlite = new UnQLite(path, UnQLiteOpenModel.Create);
            }
            else
            {
                unqlite = new UnQLite(path, UnQLiteOpenModel.Create);
            }
            json = new JavaScriptSerializer();
            this.Entities = new Dictionary<string, UnQEntity>();
        }

        private UnQLite unqlite;

        private JavaScriptSerializer json;

        private Dictionary<string, UnQEntity> Entities;

        /// <summary>
        /// declare an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        public void AddEntity<T>(string entityName)
        {
            var entity = new UnQEntity(entityName, typeof(T));
            this.Entities.Add(entityName, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetEntities<T>()
        {
            var entity = this.Entities.Values.Where( bb=> bb.Type == typeof(T)).First();
            return entity.Objects.Select(value => json.Deserialize<T>(value)).ToList();
        }

        /// <summary>
        /// When they are several entities have the same Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public List<T> GetEntities<T>(string entityName)
        {
            var entity = this.Entities[entityName];
            return entity.Objects.Select(value => json.Deserialize<T>(value)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<object> GetEntities(string name)
        {
            var entity = this.Entities[name];
            return entity.Objects.Select(value => json.Deserialize(value, entity.Type)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetRawValue(string key)
        {
            var data = unqlite.Get(key);
            return data;
        }

        /// <summary>
        /// add a kv
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="objName"></param>
        /// <param name="value"></param>
        public void Add(string entityName, string objName, object value)
        {
            var entity = this.Entities[entityName];
            var data = json.Serialize(value);
            if (String.IsNullOrEmpty(objName))
            {
                throw new Exception($"name must not be null");
            }
            if (objName.Contains('.'))
            {
                throw new Exception($"name should not contain '.'");
            }
            if (entity.Contains(objName))
            {
                throw new Exception($"object {entityName}.{objName} has existed");
            }
            var key = $"{entityName}.{objName}";
            //save to database
            var state = unqlite.TrySave(key, data);
            if (state != UnQLiteResultCode.Ok)
            {
                throw new Exception($"page add exception, {state}");
            }
            //save to cache
            entity.Add(objName, data);
        }

        /// <summary>
        /// remove a kv
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="objName"></param>
        public void Remove(string entityName, string objName)
        {
            var entity = this.Entities[entityName];
            if (!entity.Contains(objName))
            {
                throw new Exception($"object {entityName}.{objName} not exist");
            }
            var key = $"{entityName}.{objName}";
            //remove from database
            var state = unqlite.TryRemove(key);
            if (state != UnQLiteResultCode.Ok)
            {
                throw new Exception($"row remove exception, {state}");
            }
            //remove from cache
            entity.Remove(objName);
        }

        /// <summary>
        /// load data from database
        /// </summary>
        public void Refresh()
        {
            foreach(var table in Entities.Values)
            {
                table.Clear();
            }
            var data = unqlite.GetAll(CursorWalkDirection.LastToFirst);
            foreach (var item in data)
            {
                var temp = item.Item1.Split('.');
                Entities[temp[0]].Add(temp[1], item.Item2);
            }
        }

        /// <summary>
        /// IDisposable
        /// </summary>
        public void Dispose()
        {
            unqlite.Close();
        }
    }

    /// <summary>
    /// Open Mode
    /// </summary>
    public enum UnQMode
    {
        /// <summary>
        /// create a new one
        /// </summary>
        Create,

        /// <summary>
        /// delete if it exists.
        /// </summary>
        Override,

        /// <summary>
        /// Open 
        /// </summary>
        Open
    }
    
    /// <summary>
    /// An Entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnQEntity
    {
        /// <summary>
        /// An Entity
        /// </summary>
        /// <param name="unqlite"></param>
        /// <param name="name"></param>
        internal UnQEntity(string name, Type t)
        {
            Name = name;
            Type = t;
            _Objects = new Dictionary<string, string>();
            Objects = this._Objects.Values;
        }

        /// <summary>
        /// Name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// DataType
        /// </summary>
        public readonly Type Type;

        private Dictionary<string, string> _Objects;

        /// <summary>
        /// 所有的记录
        /// </summary>
        internal IEnumerable<string> Objects;
        
        /// <summary>
        /// Clear the rows
        /// </summary>
        public void Clear()
        {
            this._Objects.Clear();
        }

        /// <summary>
        /// return true if object has exists in database
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return _Objects.ContainsKey(name);
        }

        /// <summary>
        /// add an kv to cache
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public void Add(string name, string data)
        {
            this._Objects.Add(name, data);
        }

        /// <summary>
        /// remove a kv from cache
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            this._Objects.Remove(name);
        }
    }
}
