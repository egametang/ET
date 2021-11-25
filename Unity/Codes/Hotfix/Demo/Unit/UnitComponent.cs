using System.Collections.Generic;
using System.Linq;

namespace ET
{
	[ObjectSystem]
	public class UnitComponentAwakeSystem : AwakeSystem<UnitComponent>
	{
		public override void Awake(UnitComponent self)
		{
		}
	}
	
	[ObjectSystem]
	public class UnitComponentDestroySystem : DestroySystem<UnitComponent>
	{
		public override void Destroy(UnitComponent self)
		{
			foreach (Unit unit in self.idUnits.Values)
			{
				unit.Dispose();
			}

			self.idUnits.Clear();
		}
	}
	
	public static class UnitComponentSystem
	{
		public static void Add(this UnitComponent self, Unit unit)
		{
			self.idUnits.Add(unit.Id, unit);
		}

		public static Unit Get(this UnitComponent self, long id)
		{
			Unit unit;
			self.idUnits.TryGetValue(id, out unit);
			return unit;
		}

		public static void Remove(this UnitComponent self, long id)
		{
			Unit unit;
			self.idUnits.TryGetValue(id, out unit);
			self.idUnits.Remove(id);
			unit?.Dispose();
		}

		public static void RemoveNoDispose(this UnitComponent self, long id)
		{
			self.idUnits.Remove(id);
		}

		public static Unit[] GetAll(this UnitComponent self)
		{
			return self.idUnits.Values.ToArray();
		}
	}
}