using System.Collections.Generic;

namespace ET
{
	
	public class UnitComponent: Entity
	{
		public Dictionary<long, Unit> idUnits = new Dictionary<long, Unit>();
		public long MyId;
	}
}