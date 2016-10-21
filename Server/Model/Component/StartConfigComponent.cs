using System;
using System.Collections.Generic;
using System.IO;
using Base;
using CommandLine;
using MongoDB.Bson.Serialization;

namespace Model
{
	[ObjectEvent]
	public class OptionsComponentEvent : ObjectEvent<StartConfigComponent>, IAwake<string[]>
	{
		public void Awake(string[] args)
		{
			this.GetValue().Awake(args);
		}
	}

	public class StartConfigComponent: Component
	{
		private readonly List<StartConfig> allConfigs = new List<StartConfig>();

		private readonly Dictionary<int, StartConfig> configDict = new Dictionary<int, StartConfig>();

		public StartConfig MyConfig { get; private set; }

		public void Awake(string[] args)
		{
			//StartConfig sc = new StartConfig();
			////sc.IP = "192.168.12.112";
			////sc.Options.AppType = "Realm";
			////sc.Options.Id = 1;
			//
			//InnerConfig inneConfig = sc.Config.AddComponent<InnerConfig>();
			////inneConfig.Host = "127.0.0.1";
			////inneConfig.Port = 10002;
			//
			//OuterConfig outerConfig = sc.Config.AddComponent<OuterConfig>();
			////outerConfig.Host = "127.0.0.1";
			////outerConfig.Port = 10003;
			//
			//string s3 = MongoHelper.ToJson(sc);
			//StartConfig s4 = MongoHelper.FromJson<StartConfig>(s3);

			//BsonClassMap.RegisterClassMap<OuterConfig>();
			//BsonClassMap.RegisterClassMap<InnerConfig>();

			string[] ss = File.ReadAllText("./Start.txt").Split('\n');
			foreach (string s in ss)
			{
				string s2 = s.Trim();
				if (s2 == "")
				{
					continue;
				}
				try
				{
					StartConfig startConfig = MongoHelper.FromJson<StartConfig>(s2);
					this.allConfigs.Add(startConfig);
					this.configDict.Add(startConfig.Options.Id, startConfig);
				}
				catch (Exception)
				{
					Log.Error($"config错误: {s2}");
				}
			}

			Options options = new Options();
			if (!Parser.Default.ParseArguments(args, options))
			{
				throw new Exception($"命令行格式错误!");
			}

			this.MyConfig = this.Get(options.Id);
		}

		public StartConfig Get(int id)
		{
			return this.configDict[id];
		}

		public StartConfig[] GetAll()
		{
			return this.allConfigs.ToArray();
		}
	}
}
