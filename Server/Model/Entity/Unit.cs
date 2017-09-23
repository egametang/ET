using System.Numerics;

namespace Model
{
	public enum UnitType
	{
		Hero,
		Npc
	}

	[ObjectEvent]
	public class UnitEvent : ObjectEvent<Unit>, IAwake<UnitType>
	{
		public void Awake(UnitType unitType)
		{
			this.Get().Awake(unitType);
		}
	}

	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; private set; }

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