using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETHotfix
{
    public static class GenerateDbDataHelper
    {
        public static void MakeImageInfo(string version)
        {
            var sql = SqlConnectHelper.GetSQLiteConnection("imageinfo" + version + ".db");

            sql.DropTable<ImageConfig>();
            sql.CreateTable<ImageConfig>();

            string path = Path.Combine(Application.dataPath + "/Res/Text", "ImageConfig_" + version + ".txt");
            string[] itemStrArray = File.ReadAllLines(path);

            HashSet<int> containlist = new HashSet<int>();

            foreach (string itemStr in itemStrArray)
            {
                if (itemStr.Contains("#")) continue;

                string[] proArray = itemStr.Split('|');

                proArray[6] = proArray[6].Replace("\r", string.Empty); //去除回车符号

                if (containlist.Contains(int.Parse(proArray[6]))) continue;

                ImageConfig info = new ImageConfig();
                switch (proArray[0])
                {
                    case "0": //阻挡物
                        info.Walkable = false;
                        break;
                    case "1": //花草
                        info.Walkable = true;
                        break;
                    case "44": //悬崖边角
                        info.Walkable = false;
                        break;
                    case "45": //路灯
                        info.Walkable = true;
                        break;
                    case "8": //地板
                        info.Walkable = true;
                        info.IsDiban = true;
                        break;
                }

                info.PianyiX = float.Parse(proArray[1]);
                info.PianyiY = float.Parse(proArray[2]);
                info.ZhandiDong = int.Parse(proArray[3]);
                info.ZhandiNan = int.Parse(proArray[4]);
                info.PngId = proArray[5];
                info.MapId = int.Parse(proArray[6]);
                containlist.Add(info.MapId);

                //写入到数据库
                sql.Insert(info);
            }
            sql.Dispose();
        }
    }
}
