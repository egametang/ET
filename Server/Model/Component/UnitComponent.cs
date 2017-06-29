using System.Collections.Generic;

namespace Model
{
	public class UnitComponent: Component
	{
		private readonly Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();

		public void Add(Unit unit)
		{
			this.idUnits.Add(unit.Id, unit);
		}

		public Unit Get(long id)
		{
			this.idUnits.TryGetValue(id, out Unit unit);
			return unit;
		}

		public void Remove(long id)
		{
			this.idUnits.Remove(id);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();

			foreach (Unit unit in this.idUnits.Values)
			{
				unit.Dispose();
			}
		}
	}
}