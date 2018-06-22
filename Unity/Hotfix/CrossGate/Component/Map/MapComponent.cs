using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ETHotFix;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [ObjectSystem]
    public class MapComponentAwakeSystem : AwakeSystem<MapComponent>
    {
        public override void Awake(MapComponent self)
        {
            self.Awake();
        }
    }

    public class MapComponent : Component
    {
        /// <summary>
        /// 单利
        /// </summary>
        public static MapComponent Instance { get; private set; }

        private UnityWebRequestAsync webRequest;

        /// <summary>
        /// 管理地图组件
        /// </summary>
        private MapTile MapTile;

        /// <summary>
        /// 地图dat数据
        /// </summary>
        private byte[] MapByte;

        /// <summary>
        /// 地图尺寸
        /// </summary>
        private int[] MapSize;

        /// <summary>
        /// 地图ID
        /// </summary>
        private string MapId;

        public void Awake()
        {
            Instance = this;
            MapTile = ComponentFactory.Create<MapTile>();
        }

        public async void Init(int dong, int nan, string mapid)
        {
            if (MapByte == null || MapId != mapid)
            {
                //读取地图Bytes
                string path = Path.Combine((GetPlatfromType.IsEditorMode() ? Application.dataPath + "/Res" : PathHelper.AppHotfixResPath) + "/Map", mapid + ".dat");
                //Log.Debug(path);
                try
                {
                    using (this.webRequest = ETModel.ComponentFactory.Create<UnityWebRequestAsync>())
                    {
                        await this.webRequest.DownloadAsync(path);
                        MapByte = this.webRequest.Request.downloadHandler.data;
                    }
                    //WWWAsync www = ETModel.ComponentFactory.Create<WWWAsync, string>(path);
                    //bool loaded = await www.LoadDatFile(path);
                    //if (!loaded)
                    //{
                    //    Log.Error("读取地图Bytes Error! 请检查地图是否存在 - ID: " + mapid);
                    //    return;
                    //}
                    //MapByte = www.www.bytes;
                    //www.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error("读取地图Bytes Error! 请检查地图是否存在 - ID: " + mapid);
                    Log.Debug(e.Message);
                }
            }

            //读取流数据
            MemoryStream stream = new MemoryStream(MapByte);
            BinaryReader reader = new BinaryReader(stream);

            //获得地图尺寸
            MapSize = MapHelper.GetMapSize(ref reader);

            //TODO 小地图

            //一共421格子
            int count = 0;
            //Long 15
            int a = dong - 1;
            int b = nan - 13;
            for (int j = 0; j < 15; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    int d = a + i;
                    int n = b + i;
                    CreateTileByDat(MapHelper.GetTileInfo(ref reader, MapSize, d, n), d, n);
                    //间隔1毫秒
                    //await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(1);
                    count++;
                }
                a--;
                b++;
            }

            //Short 14
            a = dong - 1;
            b = nan - 12;
            for (int j = 0; j < 14; j++)
            {
                for (int i = 0; i < 14; i++)
                {
                    int d = a + i;
                    int n = b + i;
                    CreateTileByDat(MapHelper.GetTileInfo(ref reader, MapSize, d, n), d, n);
                    //间隔1毫秒
                    //await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(1);
                    count++;
                }
                a--;
                b++;
            }

            //释放内存
            stream.Dispose();
            reader.Dispose();
        }

        private void CreateTileByDat(TileDat dat, int dong, int nan)
        {
            //创建地板层
            Tile tile = TileFactory.CreateBase();
            tile.Init(dong, nan, dat);
            
            //创建遮挡层
            if (dat != null && dat.TopTile > 0)
            {
                ImageConfig config = SqlliteComponent.Instance.GetImageConfig(dat.TopTile, false);
                if (config != null)
                {
                    TileFactory.CreateTop(ref tile);
                    tile.TopTileComponent.Init(config);
                    //注意: 设置层次一定要在位置偏移前面
                    tile.TopTileComponent.spriteRenderer.sortingOrder = (int) (tile.TopTileGameObject.transform.position.y * -10f);
                    //设置偏移
                    Vector2 v2 = new Vector2(config.PianyiX, config.PianyiY);
                    tile.TopTileGameObject.transform.localPosition = v2;
                }
            }

            //存到字典
            MapTile.Add(tile);
        }

        public override void Dispose()
        {
            MapTile.Dispose();
            base.Dispose();
        }
    }
}
