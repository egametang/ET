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
		private readonly List<string> GateAddress = new List<string>();

		public void Awake()
		{
			CommandLines commandLines = this.GetComponent<OptionsComponent>().AllOptions;
			foreach (Options options in commandLines.Options)
			{
				if (options.AppType != "Gate")
				{
					continue;
				}
				this.GateAddress.Add($"{options.Host}:{options.Port}");
			}
		}

		public string GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
