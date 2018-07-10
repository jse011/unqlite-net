using System;
using UnQLiteNet;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestCreate();
            //TestCursor();
            //TestAtom();
            TestEntity();
        }

        static void TestCreate()
        {
            string path = "test.udb";
            if(System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            UnQLite unqlite = new UnQLite(path, UnQLiteOpenModel.Create);

            unqlite.Save("key", "value");
            //string value = unqlite.Get("key");
            //Contract.Assert(value == "value");
            //unqlite.Remove("key");

            unqlite.Save("key1", "value1");
            unqlite.Save("key2", "value2");
            unqlite.Save("key3", "value3");
            var data = unqlite.GetAll(CursorWalkDirection.LastToFirst);

            unqlite.TryRemove("key2");
            var data1 = unqlite.GetAll(CursorWalkDirection.LastToFirst);

            unqlite.Close();
        }

        static void TestTransaction()
        {
            UnQLite unqlite = new UnQLite("test.db", UnQLiteOpenModel.Create | UnQLiteOpenModel.ReadWrite);
            //Batch save
            using (var transaction = unqlite.BeginTransaction())
            {
                unqlite.Save("key1", "value1");
                unqlite.Save("key2", "value2");
                unqlite.Save("key3", "value3");
            }

            unqlite.Close();
        }

        static void TestCursor()
        {
            UnQLite unqlite = new UnQLite("test.udb", UnQLiteOpenModel.ReadWrite);

            //var data = unqlite.GetAll(CursorWalkDirection.FirstToLast);
            var data = unqlite.GetAll(CursorWalkDirection.LastToFirst);
            foreach (var item in data)
            {
                Console.WriteLine($"{item.Item1}: {item.Item2}");
            }    
            unqlite.Close();
        }

        static void TestAtom()
        {
            UnQEntitybase db = new UnQEntitybase();

            db.Save("12", "123");
            string res = db.Get("12");

            db.Save("12", 12);
            int res1 = db.GetInt32("12");

            db.Save("12a", 12L);
            long res2 = db.GetInt64("12a");

            db.Save("12b", 12.0f);
            float res3 = db.GetSingle("12b");

            db.Save("12c", 12.0);
            double res4 = db.GetDouble("12c");

            db.Save("12d", false);
            bool res5 = db.GetBoolean("12d");

            db.Save("123", DateTime.Now);
            DateTime res6 = db.GetDateTime("123");

            db.SaveStructure("1234", new S_Entity() {
                ID =10,
                Name = "we",
                Title = "123$"
            });
            S_Entity res7 = db.GetStructure<S_Entity>("1234");

            db.SaveObject("12345", new T_Entity1()
            {
                ID = 10,
                Name = "we",
                Title = "123$"
            });
            T_Entity1 res8 = db.GetObject<T_Entity1>("12345");
        }

        static void TestEntity()
        {
            UnQEntitybase db = new UnQEntitybase("enitty", UnQMode.Create);
            //UnQEntitybase db = new UnQEntitybase("enitty", UnQMode.Override);
            //UnQEntitybase db = new UnQEntitybase();
            db.AddEntity<T_Entity1>("page", "Name");
            db.AddEntity<T_Entity2>("page2");

            db.Save("12", 12);
            int res1 = db.GetInt32("12");

            db.Refresh();

            for (int i=0; i< 5; i++)
            {
                var page = new T_Entity1();
                page.Name = i.ToString();
                page.Title = "1234";
                db.Add("page", page);
            }
            
            for (int i = 0; i < 5; i++)
            {
                var page = new T_Entity2();
                page.Version = i;
                page.LevelMaxSize = new UInt16[] { 1, 2, 3};
                db.Add("page2", page);
            }

            var pages = db.GetRecords<T_Entity1>();
            var page2s = db.GetRecords<T_Entity2>();

            db.Remove("page", 2);
            db.Remove("page2", 3);

            var pages1 = db.GetRecords<T_Entity1>();
            var page2s1 = db.GetRecords<T_Entity2>();

            db.Refresh();

            var pages2 = db.GetRecords<T_Entity1>();
            var page2s2 = db.GetRecords<T_Entity2>();
        }
        
        public struct S_Entity
        {
            public int ID;
            public string Name;
            public string Title;
        }
        [Serializable]
        public class T_Entity1
        {
            [NonSerialized]
            public int ID;
            public string Name;
            public string Title;
            public string Path;
        }
        [Serializable]
        public class T_Entity2
        {
            [NonSerialized]
            public int ID;
            public int Version;
            public UInt16[] LevelMaxSize;
        }
    }
}
