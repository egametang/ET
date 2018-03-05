using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ETModel
{
	[ObjectSystem]
	public class StartConfigComponentSystem : AwakeSystem<StartConfigComponent, string, int>
	{
		public override void Awake(StartConfigComponent self, string a, int b)
		{
			self.Awake(a, b);
		}
	}
	
	public class StartConfigComponent: Component
	{
		private Dictionary<int, StartConfig> configDict;
		
		public StartConfig StartConfig { get; private set; }

		public StartConfig DBConfig { get; private set; }

		public StartConfig RealmConfig { get; private set; }

		public StartConfig LocationConfig { get; private set; }

		public List<StartConfig> MapConfigs { get; private set; }

		public List<StartConfig> GateConfigs { get; private set; }

		public void Awake(string path, int appId)
		{
			this.configDict = new Dictionary<int, StartConfig>();
			this.MapConfigs = new List<StartConfig>();
			this.GateConfigs = new List<StartConfig>();

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
					this.configDict.Add(startConfig.AppId, startConfig);

					if (startConfig.AppType.Is(AppType.Realm))
					{
						this.RealmConfig = startConfig;
					}

					if (startConfig.AppType.Is(AppType.Location))
					{
						this.LocationConfig = startConfig;
					}

					if (startConfig.AppType.Is(AppType.DB))
					{
						this.DBConfig = startConfig;
					}

					if (startConfig.AppType.Is(AppType.Map))
					{
						this.MapConfigs.Add(startConfig);
					}

					if (startConfig.AppType.Is(AppType.Gate))
					{
						this.GateConfigs.Add(startConfig);
					}
				}
				catch (Exception e)
				{
					Log.Error($"config错误: {s2} {e}");
				}
			}

			this.StartConfig = this.Get(appId);
		}

		public StartConfig Get(int id)
		{
			try
			{
				return this.configDict[id];
			}
			catch (Exception e)
			{
				throw new Exception($"not found startconfig: {id}", e);
			}
		}

		public StartConfig[] GetAll()
		{
			return this.configDict.Values.ToArray();
		}

		public int Count
		{
			get
			{
				return this.configDict.Count;
			}
		}
	}
}
