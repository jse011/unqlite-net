using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnQLiteNet
{
    /// <summary>
    /// Entities
    /// </summary>
    public class UnQEntitybase : IDisposable
    {
        /// <summary>
        /// create a database in memory
        /// </summary>
        public UnQEntitybase()
        {
            unqlite = new UnQLite(inmemory, UnQLiteOpenModel.Create);
            this.Entities = new Dictionary<string, UnQRecord>();
        }

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
            this.Entities = new Dictionary<string, UnQRecord>();
        }

        private const string inmemory = ":mem:";

        private UnQLite unqlite;

        private Dictionary<string, UnQRecord> Entities;

        private BinaryFormatter formatter = new BinaryFormatter();//is not cross platform

        /// <summary>
        /// declare an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="uniqueKey"></param>
        public void AddEntity<T>(string entityName, string uniqueKey = null)
        {
            var entity = new UnQRecord(entityName, typeof(T), uniqueKey);
            this.Entities.Add(entityName, entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<Tuple<long, T>> GetRecords<T>()
        {
            var entity = this.Entities.Values.Where( bb=> bb.Type == typeof(T)).First();
            return entity.GetRecords<T>();
        }

        /// <summary>
        /// When they are several entities have the same Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public List<Tuple<long, T>> GetRecords<T>(string entityName)
        {
            var entity = this.Entities[entityName];
            return entity.GetRecords<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Tuple<long, object>> GetRecords(string name)
        {
            var entity = this.Entities[name];
            return entity.GetRecords();
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, string data)
        {
            unqlite.Save(key, data);
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SaveRaw(string key, ArraySegment<byte> data)
        {
            unqlite.SaveRaw(key, data);
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SaveRaw(string key, byte[] data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(data));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, bool data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data)));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, int data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data)));
        }
        
        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, long data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data)));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, float data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data)));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, double data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data)));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Save(string key, DateTime data)
        {
            unqlite.SaveRaw(key, new ArraySegment<byte>(BitConverter.GetBytes(data.Ticks)));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SaveStructure(string key, object data)
        {
            int size = Marshal.SizeOf(data);
            byte[] arr = new byte[size];
            unsafe
            {
                fixed (byte* vp = arr)
                {
                    Marshal.StructureToPtr(data, new IntPtr(vp), false);
                }
            }            
            unqlite.SaveRaw(key, new ArraySegment<byte>(arr));
        }

        /// <summary>
        /// save kv to database
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SaveObject(string key, object data)
        {
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                stream.Flush();
                stream.Position = 0;
                unqlite.SaveRaw(key, new ArraySegment<byte>(stream.ToArray()));
            }
        }

        /// <summary>
        /// get kv from database
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var data = unqlite.Get(key);
            return data;
        }

        /// <summary>
        /// get a data of type bool
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] GetRaw(string key)
        {
            var data = unqlite.GetRaw(key);
            return data;
        }

        /// <summary>
        /// get a data of type bool
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetBoolean(string key)
        {
            var data = unqlite.GetRaw(key);
            return BitConverter.ToBoolean(data, 0);
        }

        /// <summary>
        /// get a data of type int32
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInt32(string key)
        {
            var data = unqlite.GetRaw(key);
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// get a data of type int64
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetInt64(string key)
        {
            var data = unqlite.GetRaw(key);
            return BitConverter.ToInt64(data, 0);
        }

        /// <summary>
        /// get a data of type float
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetSingle(string key)
        {
            var data = unqlite.GetRaw(key);
            return BitConverter.ToSingle(data, 0);
        }

        /// <summary>
        /// get a data of type double
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double GetDouble(string key)
        {
            var data = unqlite.GetRaw(key);
            return BitConverter.ToDouble(data, 0);
        }

        /// <summary>
        /// get a data of type datatime
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DateTime GetDateTime(string key)
        {
            var data = unqlite.GetRaw(key);
            return new DateTime(BitConverter.ToInt64(data, 0));
        }

        /// <summary>
        /// get a data of type datatime
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public unsafe T GetStructure<T>(string key)
        {
            var data = unqlite.GetRaw(key);
            fixed(byte* vp = data)
            {
                var res = (T)Marshal.PtrToStructure(new IntPtr(vp), typeof(T));
                return res;
            }
        }

        /// <summary>
        /// get a data of type datatime
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public unsafe T GetObject<T>(string key)
        {
            var data = unqlite.GetRaw(key);
            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// add a kv
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="record"></param>
        public long Add(string entityName, object record)
        {
            var entity = this.Entities[entityName];
            var data = entity.Serialize(record);
            var id = entity.NewID();
            var key = $"{entityName}.{id}";
            //check unique key
            string uniqueKey = entity.GetUniqueKeyValue?.Invoke(record);
            if (entity.Contains(uniqueKey))
            {
                throw new Exception($"object {entity.Name}.{uniqueKey} has existed");
            }
            //save to database
            var state = unqlite.TrySave(key, data);
            if (state != UnQLiteResultCode.Ok)
            {
                throw new Exception($"page add exception, {state}");
            }
            //save to cache
            entity.Add(id, uniqueKey, record);
            return id;
        }

        /// <summary>
        /// remove a kv
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        public void Remove(string entityName, long id)
        {
            var entity = this.Entities[entityName];
            if (!entity.Contains(id))
            {
                throw new Exception($"object {entityName}.{id} not exist");
            }
            var key = $"{entityName}.{id}";
            //remove from database
            var state = unqlite.TryRemove(key);
            if (state != UnQLiteResultCode.Ok)
            {
                throw new Exception($"row remove exception, {state}");
            }
            //remove from cache
            entity.Remove(id);
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
                string key = temp[0];
                if(temp.Length == 2 && Entities.ContainsKey(key))
                {
                    var entity = Entities[key];
                    var id = Convert.ToInt64(temp[1]);
                    var record = entity.Deserialize(item.Item2);
                    var uniqueKey = entity.GetUniqueKeyValue?.Invoke(record);
                    entity.Add(id, uniqueKey, record);
                }
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
    public class UnQRecord
    {
        /// <summary>
        /// An Entity
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        /// <param name="uniqueKey"></param>
        internal UnQRecord(string name, Type t, string uniqueKey = null)
        {
            Name = name;
            Type = t;
            UniqueKey = uniqueKey;
            serializer = new DataContractJsonSerializer(t);
            Objects = new Dictionary<long, Record>();
            SetUniqueKeyValue();
        }

        /*
         * DataContractJsonSerializer,  System.Runtime.Serialization.Json
         * JavaScriptSerializer	System.Web.Script.Serialization
         * JsonConvert 、JArray 、JObject 、JValue 、JProperty	Newtonsoft.Json
         */
        private DataContractJsonSerializer serializer;

        private long CurrentID = 0;

        /// <summary>
        /// Name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// unique key
        /// </summary>
        public readonly string UniqueKey;

        /// <summary>
        /// DataType
        /// </summary>
        public readonly Type Type;

        private Dictionary<long, Record> Objects;

        internal delegate string GetFieldDelegate(object record);

        internal GetFieldDelegate GetUniqueKeyValue
        {
            get;
            private set;
        }

        internal long NewID()
        {
            CurrentID += 1;
            return CurrentID;
        }

        internal string Serialize(object record)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, record);
                string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return jsonString;
            }
        }

        internal object Deserialize(string jsonString)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// Clear the rows
        /// </summary>
        internal void Clear()
        {
            this.Objects.Clear();
        }

        /// <summary>
        /// return true if object has exists in database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool Contains(long id)
        {
            return Objects.ContainsKey(id);
        }

        internal bool Contains(string key)
        {
            if(UniqueKey == null)
            {
                return false;
            }
            return Objects.Values.Any(bb => bb.UniqueKey == key);
        }

        /// <summary>
        /// add an kv to cache
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uniqueKey"></param>
        /// <param name="data"></param>
        internal void Add(long id, string uniqueKey, object data)
        {
            this.Objects.Add(id, new Record(id, uniqueKey, data));
        }

        /// <summary>
        /// When they are several entities have the same Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal List<Tuple<long, T>> GetRecords<T>()
        {
            return this.Objects.Values.Select(record => new Tuple<long, T>(record.ID, (T)record.Data)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal List<Tuple<long, object>> GetRecords()
        {
            return this.Objects.Values.Select(record => new Tuple<long, object>(record.ID, record.Data)).ToList();
        }

        /// <summary>
        /// remove a kv from cache
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(long id)
        {
            this.Objects.Remove(id);
        }

        private void SetUniqueKeyValue()
        {
            if (String.IsNullOrEmpty(UniqueKey))
            {
                return;
            }
            var propertyInfo = Type.GetProperty(UniqueKey,
                BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                GetUniqueKeyValue = (object value) => 
                {
                    var key = propertyInfo.GetValue(value, null);
                    if (key == null)
                    {
                        throw new Exception($"name must not be null");
                    }
                    return key.ToString();
                };
                return;
            }
            var filedInfo = Type.GetField(UniqueKey,
                BindingFlags.Public | BindingFlags.Instance);
            if (filedInfo != null)
            {
                GetUniqueKeyValue = (object value) =>
                {
                    var key = filedInfo.GetValue(value);
                    if (key == null)
                    {
                        throw new Exception($"name must not be null");
                    }
                    return key.ToString();
                };
                return;
            }
            else
            {
                throw new Exception("UniqueKey not exists");
            }
        }

        internal class Record
        {
            internal Record(long id, string key, object data)
            {
                this.ID = id;
                this.UniqueKey = key;
                this.Data = data;
            }
            internal long ID;
            internal string UniqueKey;
            internal object Data;
        }
    }
}
