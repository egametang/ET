using System;
using System.Collections.Generic;
using System.IO;
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
			string s = File.ReadAllText("./CommandLineConfig.txt");
			CommandLines commandLines = MongoHelper.FromJson<CommandLines>(s);
			foreach (CommandLine commandLine in commandLines.Commands)
			{
				if (commandLine.Options.AppType != "Gate")
				{
					continue;
				}
				this.GateAddress.Add($"{commandLine.Options.Host}:{commandLine.Options.Port}");
			}
		}

		public string GetAddress()
		{
			int n = RandomHelper.RandomNumber(0, this.GateAddress.Count);
			return this.GateAddress[n];
		}
	}
}
