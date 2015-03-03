using System.Collections.Generic;

namespace Model
{
	public class NodeConfig: AConfig
	{
		public List<string> Args { get; set; }
		public List<NodeConfig> SubConfigs { get; set; }
	}
}