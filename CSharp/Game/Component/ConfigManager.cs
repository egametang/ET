using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Component
{
	public class ConfigManager
	{
		private static readonly ConfigManager instance = new ConfigManager();

		public static ConfigManager Instance
		{
			get
			{
				return instance;
			}
		}

		public Dictionary<string, object> allConfig;

		private ConfigManager()
		{
			this.Load();
		}

		private void Load()
		{
			this.allConfig = new Dictionary<string, object>();
			string currentDir = AppDomain.CurrentDomain.BaseDirectory;
			Type[] types = typeof(ConfigManager).Assembly.GetTypes();
			foreach (var type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				object obj = (Activator.CreateInstance(type));

				var iInit = obj as IConfigInitialize;
				if (iInit == null)
				{
					throw new Exception(string.Format("class {0} is not IConfigInitialize", type.Name));
				}

				var iSupportInitialize = obj as ISupportInitialize;
				if (iSupportInitialize != null)
				{
					iSupportInitialize.BeginInit();
				}

				string configDir = Path.Combine(
					currentDir, ((ConfigAttribute)attrs[0]).RelativeDirectory);

				if (!Directory.Exists(configDir))
				{
					throw new Exception(string.Format("not found config dir: {0}", configDir));
				}
				iInit.Init(configDir);


				if (iSupportInitialize != null)
				{
					iSupportInitialize.EndInit();
				}

				allConfig[iInit.ConfigName] = obj;
			}
		}

		public void Reload()
		{
			this.Load();
		}
 
		public T Get<T>(int type) where T : IType
		{
			var configManager = (ConfigCategory<T>)allConfig[typeof (T).Name];
			return configManager[type];
		}

		public Dictionary<int, T> GetAll<T>() where T : IType
		{
			var configManager = (ConfigCategory<T>)allConfig[typeof (T).Name];
			return configManager.GetAll();
		}

		public ConfigCategory<T> GetConfigManager<T>() where T : IType
		{
			return (ConfigCategory<T>)allConfig[typeof(T).Name];
		}
	}
}
