using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public abstract class Component : Object, ISupportInitialize
    {
        private Entity owner;

        [BsonIgnore]
        public Entity Owner
        {
            get
            {
                return owner;
            }
            set
            {
                this.owner = value;
                this.Guid = this.owner.Guid;
            }
        }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }
}