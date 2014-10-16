using Common.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Buff: AMongo
    {
        [BsonElement]
        private int ConfigId { get; set; }

        public Buff(int configId)
        {
            this.ConfigId = configId;
        }

        [BsonIgnore]
        public BuffConfig Config
        {
            get
            {
                return World.Instance.ConfigManager.Get<BuffConfig>(this.ConfigId);
            }
        }
    }
}