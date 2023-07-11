using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
	/// <summary>
    /// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
    /// </summary>
    public class ConfigComponent: Singleton<ConfigComponent>, ISingletonAwake
    {
        public struct GetAllConfigBytes
        {
        }
        
        public struct GetOneConfigBytes
        {
            public string ConfigName;
        }
		
        private readonly ConcurrentDictionary<Type, ISingleton> allConfig = new();
        
        public void Awake()
        {
        }

		public void Reload(Type configType)
		{
			byte[] oneConfigBytes =
					EventSystem.Instance.Invoke<GetOneConfigBytes, byte[]>(new GetOneConfigBytes() { ConfigName = configType.Name });

			object category = MongoHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
			ISingleton singleton = category as ISingleton;
			this.allConfig[configType] = singleton;
			
			singleton.Register();
		}
		
		public void Load()
		{
			this.allConfig.Clear();
			Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());

			foreach (Type type in configBytes.Keys)
			{
				byte[] oneConfigBytes = configBytes[type];
				this.LoadOneInThread(type, oneConfigBytes);
			}
		}
		
		public async ETTask LoadAsync()
		{
			this.allConfig.Clear();
			Dictionary<Type, byte[]> configBytes = EventSystem.Instance.Invoke<GetAllConfigBytes, Dictionary<Type, byte[]>>(new GetAllConfigBytes());

			using ListComponent<Task> listTasks = ListComponent<Task>.Create();
			
			foreach (Type type in configBytes.Keys)
			{
				byte[] oneConfigBytes = configBytes[type];
				Task task = Task.Run(() => LoadOneInThread(type, oneConfigBytes));
				listTasks.Add(task);
			}

			await Task.WhenAll(listTasks.ToArray());
		}
		
		private void LoadOneInThread(Type configType, byte[] oneConfigBytes)
		{
			object category = MongoHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
			
			lock (this)
			{
				ISingleton singleton = category as ISingleton;
				this.allConfig[configType] = singleton;
				
				singleton.Register();
			}
		}
    }
}