using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    [BsonKnownTypes(typeof(AccountInfo))]
    [BsonKnownTypes(typeof(UserInfo))]
    public partial class Entity
    {
    }
}
