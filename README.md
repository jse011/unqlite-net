# unqlite-net
UnQlite binding for .Net

## Sample
```csharp
UnQLite unqlite = new UnQLite("test.db", UnQLiteOpenModel.Create | UnQLiteOpenModel.ReadWrite);

unqlite.Save("key", "value");           
string value = unqlite.Get("key");      
Contract.Assert(value == "value");
unqlite.Remove("key");

//Batch save
using(var transaction = unqlite.BeginTransaction()) {
    unqlite.Save("key1", "value1");
    unqlite.Save("key2", "value2");
    unqlite.Save("key3", "value3");
}

//GetAll
var data = unqlite.GetAll(CursorWalkDirection.FirstToLast);
var data1 = unqlite.GetAll(CursorWalkDirection.LastToFirst);

unqlite.Close();
```

## Installation
### NuGet
https://www.nuget.org/packages/UnQLiteNet/  

`Install-Package UnQLiteNet`


## *License*
[Apache 2.0 license](https://raw.githubusercontent.com/sy-yanghuan/unqlite-net/master/LICENSE).
