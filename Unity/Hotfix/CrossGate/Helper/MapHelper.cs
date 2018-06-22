using System.IO;
using UnityEngine;

namespace ETHotfix
{
    public static class MapHelper
    {
        public static int[] GetMapSize(ref BinaryReader reader)
        {
            reader.BaseStream.Seek(12, SeekOrigin.Begin);//DWORD地圖長度-東(W) 4字节
            int width = reader.ReadInt16();
            reader.BaseStream.Seek(12 + 4, SeekOrigin.Begin); //DWORD地圖長度-南(H) 4字节
            int high = reader.ReadInt16();
            return new[] { width, high };
        }

        public static TileDat GetTileInfo(ref BinaryReader reader, int[] mapsize, int dong, int nan)
        {
            //超出边界判断
            if (dong > mapsize[0] - 1 || dong < 0 || nan > mapsize[1] - 1 || nan < 0) return null;

            long baseTile = 0;
            long topTile = 0;

            //因为前面有3个固定Map字节 + 9个空白字节 所以真正开始的数据块是 12+4字节(东南长度)

            //正式开始地图Block的位置是从20字节开始(但是实际是从22开始 ?未知)

            //通过坐标反算计算格子序号
            int blockIndex = mapsize[0] * nan + dong; //宽的格子数量 * 往下偏移的数量(也就是南) + 东偏移的数量

            //地板
            reader.BaseStream.Seek(20 + 2 * blockIndex, SeekOrigin.Begin);
            baseTile = reader.ReadInt16();
            if (baseTile < 0) //溢出判断
            {
                reader.BaseStream.Seek(20 + 2 * blockIndex, SeekOrigin.Begin);
                baseTile = reader.ReadUInt16(); //地板
            }

            //遮挡
            reader.BaseStream.Seek(20 + (mapsize[0] * mapsize[1] * 2) + 2 * blockIndex, SeekOrigin.Begin);
            topTile = reader.ReadInt16();
            if (topTile < 0) //溢出判断
            {
                reader.BaseStream.Seek(20 + (mapsize[0] * mapsize[1] * 2) + 2 * blockIndex, SeekOrigin.Begin);
                topTile = reader.ReadUInt16(); //地板
            }

            int bb = (int)baseTile;
            int tt = (int)topTile;

            //200-225是音乐编号
            //250-265是场景音效
            int soundid = -1;
            if (tt >= 200 && tt <= 225)
            {
                soundid = tt;
            }

            //判断是否有用类型
            if (bb == 999 || bb == 20 || bb == 99) bb = 0;
            if (tt == 2 || tt == 999 || tt == 20 || tt == 99 || (tt >= 200 && tt <= 263)) tt = 0;

            //返回信息
            return new TileDat { BaseTile = bb, TopTile = tt, SoundId = soundid };
        }

        public static Vector3 ConverCordToVector3(int dong, int nan)
        {
            return new Vector3(dong * 0.315f, dong * 0.235f, 0) + new Vector3(nan * 0.315f, nan * -0.235f, 0);
        }
    }
}
