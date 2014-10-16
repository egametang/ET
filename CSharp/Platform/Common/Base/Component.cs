using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    /// <summary>
    /// Component的Id与Owner Entity Id一样
    /// </summary>
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
                this.Id = this.owner.Id;
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