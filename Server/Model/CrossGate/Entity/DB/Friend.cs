using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 好友列表
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Friend : Entity
    {
        public int UserID { get; set; }
        public int QinmiPoint { get; set; }
    }
}
