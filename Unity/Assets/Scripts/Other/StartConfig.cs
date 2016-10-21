using System;
using Base;

namespace Model
{
	public class StartConfig: ICloneable
	{
		public Options Options { get; set; }

		public string IP { get; set; }

		public Entity Config { get; set; }

		public StartConfig()
		{
			this.Options = new Options();
			this.Config = new Entity("StartConfig");
		}

		public object Clone()
		{
			return MongoHelper.FromJson<StartConfig>(MongoHelper.ToJson(this));
		}
	}
}
