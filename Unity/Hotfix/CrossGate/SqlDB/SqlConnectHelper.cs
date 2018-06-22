using SQLite4Unity3d;
using UnityEngine;
#if !UNITY_EDITOR
using System.IO;
using ETModel;
#endif

namespace ETHotfix
{
    public static class SqlConnectHelper
    {
        public static SQLiteConnection GetSQLiteConnection(string dbname)
        {
            var dbPath = Path.Combine(GetSavePath("DB"), dbname);
            //Log.Debug(dbPath);
            return new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }

        public static string GetSavePath(string subfolder)
        {
            if (GetPlatfromType.IsEditorMode())
            {
                return Path.Combine(Application.dataPath, "Res", subfolder);
            }
            else
            {
                return Path.Combine(PathHelper.AppHotfixResPath, subfolder);
            }
        }

        public static void GetTest()
        {
            //var sql = SqlConnectHelper.GetSQLiteConnection("dreamgate.db");
            //ImageConfig c = new ImageConfig { Id = 999, Name = "鸡爱母" };
            //sql.Insert(c);
            //var data = sql.Table<ImageConfig>();
            //foreach (var info in data)
            //{
            //    DebugText._instance.Show($"{JsonHelper.ToJson(info)}");
            //}

            //var sql = SqlConnectHelper.GetSQLiteConnection("test.db");
            //sql.DropTable<ImageConfig>();
            //sql.CreateTable<ImageConfig>();
            //ImageConfig c = new ImageConfig { Id = 666, Name = "你好" };
            //sql.Insert(c);
            //c = new ImageConfig { Id = 777, Name = "我很好" };
            //sql.Insert(c);
            //var data = sql.Table<ImageConfig>();
            //foreach (var info in data)
            //{
            //    DebugText._instance.Show($"{JsonHelper.ToJson(info)}");
            //}
            //sql.Dispose();

            //_connection.DropTable<Person>();
            //_connection.CreateTable<Person>();

            //_connection.InsertAll(new[]{
            //        new Person{
            //                Id = 1,
            //                Name = "Tom",
            //                Surname = "Perez",
            //                Age = 56
            //        },
            //        new Person{
            //                Id = 2,
            //                Name = "Fred",
            //                Surname = "Arthurson",
            //                Age = 16
            //        },
            //        new Person{
            //                Id = 3,
            //                Name = "John",
            //                Surname = "Doe",
            //                Age = 25
            //        },
            //        new Person{
            //                Id = 4,
            //                Name = "Roberto",
            //                Surname = "Huertas",
            //                Age = 37
            //        }
            //});

            //return _connection.Table<Person>().Where(x => x.Name == "Roberto");

            //return _connection.Table<Person>().Where(x => x.Name == "Johnny").FirstOrDefault();
        }
    }
}