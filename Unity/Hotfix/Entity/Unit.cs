namespace Hotfix
{
	public enum UnitType
	{
		Hero,
		Npc
	}
	
	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; }

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}

		public Unit(UnitType unitType)
		{
			this.UnitType = unitType;
		}
	}
}