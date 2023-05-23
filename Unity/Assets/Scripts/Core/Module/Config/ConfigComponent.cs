using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bright.Serialization;

namespace ET
{
	/// <summary>
    /// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
    /// </summary>
    public class ConfigComponent: Singleton<ConfigComponent>
    {
        public struct GetAllConfigBytes
        {
        }
        
        public struct GetOneConfigBytes
        {
            public string ConfigName;
        }
		
        private readonly Dictionary<string, IConfigSingleton> allConfig = new Dictionary<string, IConfigSingleton>(20);

		public override void Dispose()
		{
			foreach (var kv in this.allConfig)
			{
				kv.Value.Destroy();
			}
		}

		public object LoadOneConfig(Type configType)
		{
			this.allConfig.TryGetValue(configType.Name, out IConfigSingleton oneConfig);
			if (oneConfig != null)
			{
				oneConfig.Destroy();
			}
			
			ByteBuf oneConfigBytes = EventSystem.Instance.Invoke<GetOneConfigBytes, ByteBuf>(new GetOneConfigBytes() {ConfigName = configType.FullName});

			object category = Activator.CreateInstance(configType, oneConfigBytes);
			IConfigSingleton singleton = category as IConfigSingleton;
			singleton.Register();
			
			this.allConfig[configType.Name] = singleton;
			return category;
		}
		
		public void Load()
		{
			this.allConfig.Clear();
			Dictionary<Type, ByteBuf> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, ByteBuf>>(new GetAllConfigBytes());

			foreach (Type type in configBytes.Keys)
			{
				ByteBuf oneConfigBytes = configBytes[type];
				this.LoadOneInThread(type, oneConfigBytes);
			}
			
			foreach (IConfigSingleton category in this.allConfig.Values)
			{
				category.Register();
				category.Resolve(allConfig);
			}
		}
		
		public async ETTask LoadAsync()
		{
			this.allConfig.Clear();
			Dictionary<Type, ByteBuf> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, ByteBuf>>(new GetAllConfigBytes());

			using ListComponent<Task> listTasks = ListComponent<Task>.Create();
			
			foreach (Type type in configBytes.Keys)
			{
				ByteBuf oneConfigBytes = configBytes[type];
				Task task = Task.Run(() => LoadOneInThread(type, oneConfigBytes));
				listTasks.Add(task);
			}

			await Task.WhenAll(listTasks.ToArray());

			foreach (IConfigSingleton category in this.allConfig.Values)
			{
				category.Register();
			}
			
			foreach (IConfigSingleton category in this.allConfig.Values)
			{
				category.Resolve(allConfig);
			}
		}
		
		private void LoadOneInThread(Type configType, ByteBuf oneConfigBytes)
		{
			object category = Activator.CreateInstance(configType, oneConfigBytes);
			lock (this)
			{
				this.allConfig[configType.Name] = category as IConfigSingleton;	
			}
		}
		
		public void TranslateText(Func<string, string, string> translator)
		{
			foreach (IConfigSingleton category in this.allConfig.Values)
			{
				category.TranslateText(translator);
			}
		}
	}
}