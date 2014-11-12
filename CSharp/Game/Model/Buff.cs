using Common.Base;
using Common.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Buff: Entity<Buff>
    {
        [BsonElement]
        private int configId { get; set; }

        [BsonElement]
        private long expiration;

        [BsonIgnore]
        private ObjectId timerId;

        [BsonIgnore]
        public long Expiration 
        {
            get
            {
                return this.expiration;
            }
            set
            {
                this.expiration = value;
            }
        }

        [BsonIgnore]
        public ObjectId TimerId 
        {
            get
            {
                return this.timerId;
            }
            set
            {
                this.timerId = value;
            }
        }

        public Buff(int configId)
        {
            this.configId = configId;
            if (this.Config.Duration != 0)
            {
                this.Expiration = TimeHelper.Now() + this.Config.Duration;
            }
        }

        [BsonIgnore]
        public BuffConfig Config
        {
            get
            {
                return World.Instance.GetComponent<ConfigComponent>().Get<BuffConfig>(this.configId);
            }
        }
    }
}