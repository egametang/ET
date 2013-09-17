using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace BehaviorTree
{
	public class Config
	{
		public uint Id { get; set; }

		public string Name { get; set; }

		[BsonIgnoreIfNull]
		public List<string> Args { get; set; }

		[BsonIgnoreIfNull]
		public List<Config> SubConfigs { get; set; }

		public void AddArgs(string arg)
		{
			if (this.Args == null)
			{
				this.Args = new List<string>();
			}

			this.Args.Add(arg);
		}

		public void AddSubConfig(Config subConfig)
		{
			if (this.SubConfigs == null)
			{
				this.SubConfigs = new List<Config>();
			}

			this.SubConfigs.Add(subConfig);
		}
	}
}
