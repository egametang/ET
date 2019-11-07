using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ETModel
{
#if !SERVER
	[HideInHierarchy]
#endif
	[NoObjectPool]
	public class StartConfig: Entity
	{
		[BsonIgnore]
		public long SceneInstanceId { get; set; }
		
		public List<StartConfig> List = new List<StartConfig>();

		public void Add(StartConfig startConfig)
		{
			startConfig.parent = this;
			this.List.Add(startConfig);
		}

		public StartConfig Get(long id)
		{
			foreach (StartConfig startConfig in this.List)
			{
				if (startConfig.Id == id)
				{
					return startConfig;
				}
			}

			return null;
		}

		public void Remove(StartConfig startConfig)
		{
			this.List.Remove(startConfig);
		}

		public override void EndInit()
		{
			base.EndInit();

			foreach (StartConfig startConfig in this.List)
			{
				startConfig.parent = this;
			}
		}
	}
}