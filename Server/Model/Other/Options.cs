using System;
using MongoDB.Bson;

#if SERVER
using CommandLine;
#endif

namespace Model
{
	public class Options: ICloneable
	{
		[Option("appId", Required = true)]
		public int AppId { get; set; }
		
		[Option("appType", Required = true)]
		public AppType AppType { get; set; }

		[Option("config", Required = false, DefaultValue = "Start.txt")]
		public string Config { get; set; }

		public object Clone()
		{
			return MongoHelper.FromBson<Options>(this.ToBson());
		}
	}
}