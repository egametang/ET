using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public class LSInputComponent: LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        [BsonIgnore]
        public LSInput LSInput { get; set; } = new LSInput();
    }
}