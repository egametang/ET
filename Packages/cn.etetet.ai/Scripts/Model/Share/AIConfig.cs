using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	public partial class AIConfigCategory
	{
		[BsonIgnore]
		public Dictionary<int, List<AIConfig>> AIConfigs = new ();

		public List<AIConfig> GetAI(int aiConfigId)
		{
			return this.AIConfigs[aiConfigId];
		}

		public override void EndInit()
		{
			foreach (var kv in this.GetAll())
			{
				List<AIConfig> aiNodeConfig;
				if (!this.AIConfigs.TryGetValue(kv.Value.AIConfigId, out aiNodeConfig))
				{
					aiNodeConfig = new List<AIConfig>();
					this.AIConfigs.Add(kv.Value.AIConfigId, aiNodeConfig);
				}
				
				aiNodeConfig.Add(kv.Value);
			}

			foreach (var kv in this.AIConfigs)
			{
				kv.Value.Sort((x, y)=>x.AIConfigId - y.AIConfigId);
			}
		}
	}
}
