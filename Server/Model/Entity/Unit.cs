using System.Numerics;

namespace Model
{
	public enum UnitType
	{
		Hero,
		Npc
	}
	
	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; }

		public Gamer Gamer;

		public Vector3 Position;

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}

		public Unit(Gamer gamer, UnitType unitType)
		{
			this.Gamer = gamer;
			this.UnitType = unitType;
		}
	}
}