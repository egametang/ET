using System;
using System.Text;
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
		[Option("appId", Required = true, HelpText = "Id.")]
#endif
		public int AppId { get; set; }

#if SERVER
		[Option("appType", Required = true, HelpText = "AppType: realm gate")]
#endif
		public string AppType { get; set; }

#if SERVER
		[HelpOption]
#endif
		public string GetUsage()
		{
			// this without using CommandLine.Text
			StringBuilder usage = new StringBuilder();
			usage.AppendLine("Quickstart Application 1.0");
			usage.AppendLine("Read user manual for usage instructions...");
			return usage.ToString();
		}

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