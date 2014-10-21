using Common.Config;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Unit : GameObject
    {
        [BsonElement]
        public int configId { get; set; }

        [BsonIgnore]
        public UnitConfig Config
        {
            get
            {
                return World.Instance.GetComponent<ConfigComponent>().Get<UnitConfig>(this.configId);
            }
        }

        public Unit(int configId)
        {
            this.configId = configId;
        }
    }
}