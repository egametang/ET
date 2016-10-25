using System;
using Base;

namespace Model
{
	public class StartConfig: Entity
	{
		public int AppId { get; set; }

		public string AppType { get; set; }

		public string ServerIP { get; set; }

		public StartConfig(): base(EntityType.Config)
		{
		}

		public object Clone()
		{
			return MongoHelper.FromJson<StartConfig>(MongoHelper.ToJson(this));
		}
	}
}
