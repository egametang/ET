using Common.Config;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Buff: GameObject
    {
        [BsonElement]
        private int configId { get; set; }

        public Buff(int configId)
        {
            this.configId = configId;
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