using System.Collections.Generic;

namespace Component
{
	public class AllConfigManager
	{
		public readonly Dictionary<string, object> allConfig = new Dictionary<string, object>();
 
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
