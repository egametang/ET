using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
    public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
    {
        public override void Awake(ConfigComponent self)
        {
	        ConfigComponent.Instance = self;
            self.Awake();
        }
    }

    public class ConfigLoadSystem : LoadSystem<ConfigComponent>
    {
        public override void Load(ConfigComponent self)
        {
            self.Load();
        }
    }
    
    public class ConfigDestroySystem : DestroySystem<ConfigComponent>
    {
	    public override void Destroy(ConfigComponent self)
	    {
		    ConfigComponent.Instance = null;
	    }
    }
    
    public static class ConfigComponentSystem
	{
		public static void Awake(this ConfigComponent self)
		{
			self.Load();
		}

		public static void Load(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			FunctionCallback.GetAllConfigBytes(configBytes);

			List<Task> listTasks = new List<Task>();

			foreach (Type type in types)
			{
				Task task = Task.Run(() => self.LoadOneInThread(type, configBytes));
				listTasks.Add(task);
			}

			Task.WaitAll(listTasks.ToArray());
		}
		
		
		public static async ETTask LoadAsync(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			FunctionCallback.GetAllConfigBytes(configBytes);

			List<Task> listTasks = new List<Task>();

			foreach (Type type in types)
			{
				Task task = Task.Run(() => self.LoadOneInThread(type, configBytes));
				listTasks.Add(task);
			}

			await Task.WhenAll(listTasks.ToArray());
		}

		private static void LoadOneInThread(this ConfigComponent self, Type configType, Dictionary<string, byte[]> configBytes)
		{
			byte[] oneConfigBytes = configBytes[configType.Name];

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			lock (self)
			{
				self.AllConfig[configType] = category;	
			}
		}
	}
}