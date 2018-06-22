using System.Collections.Generic;
using ETModel;
using SQLite4Unity3d;

namespace ETHotfix
{
    [ObjectSystem]
    public sealed class SqlliteComponentAwakeSystem : AwakeSystem<SqlliteComponent>
    {
        public override void Awake(SqlliteComponent self)
        {
            self.Awake();
        }
    }

    public sealed class SqlliteComponent : Component
    {
        public static SqlliteComponent Instance { get; private set; }
        
        //素材图片名字偏移等信息
        private readonly Dictionary<int, ImageConfig> imageConfig_2 = new Dictionary<int, ImageConfig>();
        private readonly Dictionary<int, ImageConfig> imageConfig_3 = new Dictionary<int, ImageConfig>();

        public void Awake()
        {
            Instance = this;
        }
        
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
            
            Instance = null;

            imageConfig_2.Clear();
            imageConfig_3.Clear();
        }

        /// <summary>
        /// 获得图片的配置数据
        /// </summary>
        /// <param name="图片的MapId"></param>
        /// <param name="是否物品道具类型"></param>
        /// <returns></returns>
        public ImageConfig GetImageConfig(int mapid, bool isitemtype)
        {
            ImageConfig config = null;
            if (isitemtype)
            {
                //道具类型强制选择2.0版本
                config = GetImageConfigByType(2, mapid);
            }
            else
            {
                //地图类型优先判断3.0版本
                config = GetImageConfigByType(3, mapid);
                if (config != null) return config;
                //不存在则加载2.0版本
                config = GetImageConfigByType(2, mapid);
            }
            return config;
        }

        private ImageConfig GetImageConfigByType(int version, int mapid)
        {
            if (version == 2)
            {
                if (imageConfig_2.ContainsKey(mapid))
                {
                    return imageConfig_2[mapid];
                }
            }
            else
            {
                if (imageConfig_3.ContainsKey(mapid))
                {
                    return imageConfig_3[mapid];
                }
            }
   
            //数据库中读取
            SQLiteConnection sql = SqlConnectHelper.GetSQLiteConnection("imageinfo" + version + ".0.db");
            ImageConfig cf = sql.Table<ImageConfig>().Where(x => x.MapId == mapid).FirstOrDefault();
            sql.Dispose();
            if (cf != null)
            {
                if (version == 2)
                {
                    imageConfig_2.Add(mapid, cf);
                }
                else
                {
                    imageConfig_3.Add(mapid, cf);
                }
                return cf;
            }
            return null;
        }
    }
}