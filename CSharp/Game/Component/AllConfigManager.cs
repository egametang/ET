using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace Component
{
	public class AllConfigManager
	{
		public readonly Dictionary<string, object> allConfig = new Dictionary<string, object>();

		public AllConfigManager(string path)
		{
			foreach (var dir in Directory.GetDirectories(path))
			{
				// 配置目录名与类名一致
				string baseName = new DirectoryInfo(dir).Name;
				var assembly = typeof (AllConfigManager).Assembly;
				object obj = assembly.CreateInstance(baseName);

				var iSupportInitialize = obj as ISupportInitialize;
				if (iSupportInitialize != null)
				{
					iSupportInitialize.BeginInit();
				}

				var iInit = obj as IConfigInitialize;
				if (iInit != null)
				{
					iInit.Init(dir);
				}

				if (iSupportInitialize != null)
				{
					iSupportInitialize.EndInit();
				}
			}
		}
 
		public T Get<T>(int type) where T : IType
		{
			var configManager = (ConfigManager<T>)allConfig[typeof (T).Name];
			return configManager[type];
		}

		public Dictionary<int, T> GetAll<T>(int type) where T : IType
		{
			var configManager = (ConfigManager<T>)allConfig[typeof (T).Name];
			return configManager.GetAll();
		}

		public ConfigManager<T> GetConfigManager<T>() where T : IType
		{
			return (ConfigManager<T>)allConfig[typeof(T).Name];
		}
	}
}
