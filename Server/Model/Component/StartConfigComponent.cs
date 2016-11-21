using System;
using System.Collections.Generic;
using System.IO;
using Base;
using CommandLine;

namespace Model
{
	[DisposerEvent]
	public class StartConfigComponentEvent : DisposerEvent<StartConfigComponent>, IAwake<string[]>
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

		public Options Options = new Options();

		public StartConfig MyConfig { get; private set; }

		public void Awake(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, this.Options))
			{
				throw new Exception($"命令行格式错误!");
			}
			
			string[] ss = File.ReadAllText(this.Options.Config).Split('\n');
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

			this.MyConfig = this.Get(this.Options.AppId);
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
