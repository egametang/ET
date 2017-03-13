using System.Collections.Generic;
using Base;

namespace Model
{
	[EntityEvent(EntityEventId.RealmGateAddressComponent)]
	public class RealmGateAddressComponent : Component
	{
		public readonly List<StartConfig> GateAddress = new List<StartConfig>();

		public StartConfig GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
