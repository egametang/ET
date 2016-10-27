using System;
using Base;
using MongoDB.Bson.Serialization.Attributes;
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
			return MongoHelper.FromBson<Options>(MongoHelper.ToBson(this));
		}
	}

	[BsonIgnoreExtraElements]
	public class InnerConfig: Component
	{
		public string Host { get; set; }
		public int Port { get; set; }

		[BsonIgnore]
		public string Address
		{
			get
			{
				return $"{this.Host}:{this.Port}";
			}
		}
	}

	[BsonIgnoreExtraElements]
	public class OuterConfig: Component
	{
		public string Host { get; set; }
		public int Port { get; set; }

		[BsonIgnore]
		public string Address
		{
			get
			{
				return $"{this.Host}:{this.Port}";
			}
		}
	}
}