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
            TestCreate();
            //TestCursor();
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
    }
}
