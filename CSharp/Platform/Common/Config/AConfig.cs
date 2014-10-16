using Common.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Config
{
    public abstract class AConfig : IMongo
    {
        [BsonId]
        public int Id { get; set; }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }
}