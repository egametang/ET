using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
	public partial class AIConfigCategory
	{
		public Dictionary<int, SortedDictionary<int, AIConfig>> AIConfigs = new Dictionary<int, SortedDictionary<int, AIConfig>>();

		public SortedDictionary<int, AIConfig> GetAI(int aiConfigId)
		{
			return this.AIConfigs[aiConfigId];
		}
		
		public override void EndInit()
		{
			base.EndInit();
			
			foreach (var kv in this.GetAll())
			{
				SortedDictionary<int, AIConfig> aiNodeConfig;
				if (!this.AIConfigs.TryGetValue(kv.Value.AIConfigId, out aiNodeConfig))
				{
					aiNodeConfig = new SortedDictionary<int, AIConfig>();
					this.AIConfigs.Add(kv.Value.AIConfigId, aiNodeConfig);
				}
				
				aiNodeConfig.Add(kv.Key, kv.Value);
			}
		}
	}
}
