using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 可穿戴信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Equipable : Entity
    {
        public int Hp { get; set; }
        public int Mp { get; set; }

        public int Attack { get; set; }
        public int Defend { get; set; }
        public int Speed { get; set; }
        public int Jingshen { get; set; }
        public int Huifu { get; set; }
        public int Mogong { get; set; }

        public int Bisha { get; set; }
        public int Fanji { get; set; }
        public int Minzhong { get; set; }
        public int Shanduo { get; set; }

        public int Di { get; set; }
        public int Shui { get; set; }
        public int Huo { get; set; }
        public int Feng { get; set; }

        public int Kangdu { get; set; }
        public int Kanghunshui { get; set; }
        public int Kangshihua { get; set; }
        public int Kangjiuzui { get; set; }
        public int Kanghunluan { get; set; }
        public int Kangyiwang { get; set; }

        public int ShuxingType1 { get; set; }
        public int ShuxingType2 { get; set; }
        public int ShuxingValue1 { get; set; }
        public int ShuxingValue2 { get; set; }
    }
}
