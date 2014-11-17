using Common.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class Unit: Entity<Unit>
	{
		[BsonElement]
		private int configId { get; set; }

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