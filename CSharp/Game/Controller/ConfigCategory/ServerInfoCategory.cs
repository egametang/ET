using System.Collections.Generic;
using Model;

namespace Controller
{
	[Config(ServerType.All)]
	public class ServerInfoCategory: ACategory<ServerInfoConfig>
	{
		public Dictionary<string, ServerInfoConfig> NameServerInfoConfigs =
				new Dictionary<string, ServerInfoConfig>();

		public override void EndInit()
		{
			base.EndInit();

			foreach (ServerInfoConfig serverInfoConfig in this.GetAll())
			{
				this.NameServerInfoConfigs[serverInfoConfig.Name] = serverInfoConfig;
			}
		}

		public ServerInfoConfig this[string name]
		{
			get
			{
				return this.NameServerInfoConfigs[name];
			}
		}
	}
}