using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UnQLiteNet;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestCreate();
            //TestCursor();
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

        static void TestEntity()
        {
            UnQEntitybase db = new UnQEntitybase("enitty", UnQMode.Override);
            db.AddEntity<T_DataPage>("page");
            db.AddEntity<T_DataPage2>("page2");

            db.Refresh();

            for (int i=0; i< 5; i++)
            {
                var page = new T_DataPage();
                page.Name = i.ToString();
                page.Title = "1234";
                db.Add("page", i.ToString(), page);
            }
            
            for (int i = 0; i < 5; i++)
            {
                var page = new T_DataPage2();
                page.Version = i;
                page.LevelMaxSize = new UInt16[] { 1, 2, 3};
                db.Add("page2", i.ToString(), page);
            }

            var pages = db.GetEntities<T_DataPage>();
            var page2s = db.GetEntities<T_DataPage2>();

            db.Remove("page", "2");
            db.Remove("page2", "3");

            var pages1 = db.GetEntities<T_DataPage>();
            var page2s1 = db.GetEntities<T_DataPage2>();

            db.Refresh();

            var pages2 = db.GetEntities<T_DataPage>();
            var page2s2 = db.GetEntities<T_DataPage2>();
        }

        public class T_DataPage
        {
            public string Name;
            public string Title;
        }
        public class T_DataPage2
        {
            public int Version;
            public UInt16[] LevelMaxSize;
        }
    }
}
