using System;
using System.Collections.Generic;
using System.IO;
using Base;
using CommandLine;

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
					this.configDict.Add(startConfig.AppId, startConfig);
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
