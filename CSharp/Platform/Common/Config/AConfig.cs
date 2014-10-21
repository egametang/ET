using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Config
{
    public abstract class AConfig : ISupportInitialize
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