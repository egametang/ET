using System.Collections.Generic;
using Base;

namespace Model
{
	[ObjectEvent]
	public class RealmGateAddressComponentEvent : ObjectEvent<RealmGateAddressComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	public class RealmGateAddressComponent : Component
	{
		private readonly List<StartConfig> GateAddress = new List<StartConfig>();

		public void Awake()
		{
			StartConfig[] startConfigs = this.GetComponent<StartConfigComponent>().GetAll();
			foreach (StartConfig config in startConfigs)
			{
				if (config.AppType != AppType.Gate)
				{
					continue;
				}
				this.GateAddress.Add(config);
			}
		}

		public Entity GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
