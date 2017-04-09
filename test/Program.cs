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
            TestCursor();
        }

        static void TestCreate()
        {
            UnQLite unqlite = new UnQLite("test.udb", UnQLiteOpenModel.Create);

            unqlite.Save("key", "value");
            string value = unqlite.Get("key");
            Contract.Assert(value == "value");
            unqlite.Remove("key");

            unqlite.Save("key1", "value1");
            unqlite.Save("key2", "value2");
            unqlite.Save("key3", "value3");
            
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
            
            var cursor = unqlite.InitCursor();
            //var data = cursor.GetAll();
            var data = cursor.GetAll(CursorWalkDirection.FirstToLast);
            foreach (var item in data)
            {
                Console.WriteLine($"{item.Item1}: {item.Item2}");
            }            
            cursor.Dispose();
            unqlite.Close();
        }
    }
}
