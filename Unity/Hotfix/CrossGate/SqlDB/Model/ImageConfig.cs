using SQLite4Unity3d;

namespace ETHotfix
{
    public class ImageConfig
    {
        //可否行走|偏移x|偏移y|占地东|占地南|图片id|地图编号id
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool Walkable { get; set; }
        public float PianyiX { get; set; }
        public float PianyiY { get; set; }
        public int ZhandiDong { get; set; }
        public int ZhandiNan { get; set; }
        public string PngId { get; set; }
        public int MapId { get; set; }
        public bool IsDiban { get; set; }
    }
}
