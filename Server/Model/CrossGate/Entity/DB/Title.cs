using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    /// <summary>
    /// 称号信息
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Title : Entity
    {
        public int TitleId { get; set; }
        public int TitleType { get; set; }
    }
}
