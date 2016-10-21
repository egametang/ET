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
		private readonly List<Entity> GateAddress = new List<Entity>();

		public void Awake()
		{
			StartConfig[] startConfigs = this.GetComponent<StartConfigComponent>().GetAll();
			foreach (StartConfig config in startConfigs)
			{
				if (config.Options.AppType != "Gate")
				{
					continue;
				}
				this.GateAddress.Add(config.Config);
			}
		}

		public Entity GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
