using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public enum UnitType
	{
		Hero,
		Npc
	}

	[ObjectSystem]
	public class UnitSystem : ObjectSystem<Unit>, IAwake<UnitType>
	{
		public void Awake(UnitType unitType)
		{
			this.Get().Awake(unitType);
		}
	}

	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; private set; }
		
		[BsonIgnore]
		public Vector3 Position { get; set; }
		
		public void Awake(UnitType unitType)
		{
			this.UnitType = unitType;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}