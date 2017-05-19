using System;
using MongoDB.Bson;

#if SERVER
using CommandLine;
#endif

namespace Model
{
	public class Options: ICloneable
	{
#if SERVER
		[Option("appId", Required = true)]
#endif
		public int AppId { get; set; }

#if SERVER
		[Option("appType", Required = true)]
#endif
		public AppType AppType { get; set; }

#if SERVER
		[Option("config", Required = false, DefaultValue = "Start.txt")]
#endif
		public string Config { get; set; }

		public object Clone()
		{
			return MongoHelper.FromBson<Options>(this.ToBson());
		}
	}
}