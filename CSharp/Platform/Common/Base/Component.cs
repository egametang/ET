using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public class Component: Object
    {
        private Entity owner;

        [BsonIgnore]
        public Entity Owner {
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

        protected Component()
        {
        }
    }
}