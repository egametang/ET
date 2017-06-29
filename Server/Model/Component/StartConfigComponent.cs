using System;
using System.Collections.Generic;
using System.IO;
using Base;

namespace Model
{
	[ObjectEvent]
	public class StartConfigComponentEvent : ObjectEvent<StartConfigComponent>, IAwake<string, int>
	{
		public void Awake(string a, int b)
		{
			this.Get().Awake(a, b);
		}
	}
	
	public class StartConfigComponent: Component
	{
		private readonly List<StartConfig> allConfigs = new List<StartConfig>();

		private readonly Dictionary<int, StartConfig> configDict = new Dictionary<int, StartConfig>();
		
		public StartConfig StartConfig { get; private set; }

		public void Awake(string path, int appId)
		{
			string[] ss = File.ReadAllText(path).Split('\n');
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

			this.StartConfig = this.Get(appId);
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
