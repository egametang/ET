using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
	[ObjectSystem]
    public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
    {
        public override void Awake(ConfigComponent self)
        {
	        ConfigComponent.Instance = self;
        }
    }
    
    [ObjectSystem]
    public class ConfigDestroySystem : DestroySystem<ConfigComponent>
    {
	    public override void Destroy(ConfigComponent self)
	    {
		    ConfigComponent.Instance = null;
	    }
    }
    
    public static class ConfigComponentSystem
	{
		public static void LoadOneConfig(this ConfigComponent self, Type configType)
		{
			byte[] oneConfigBytes = self.ConfigLoader.GetOneConfigBytes(configType.FullName);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			self.AllConfig[configType] = category;
		}
		
		public static void Load(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			self.ConfigLoader.GetAllConfigBytes(configBytes);
Log.Error("加载config");
			foreach (Type type in types)
			{
				Log.Error("加载config："+type.Name);
				self.LoadOneInThread(type, configBytes);
			}
			Log.Error("加载config完成");
		}
		
		public  static async ETTask LoadAsync(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			self.ConfigLoader.GetAllConfigBytes(configBytes);
            
//#if !NOT_UNITY
			async ETTask Load1(Type configType, Dictionary<string, byte[]> configBytes)
			{
			   await ETTask.CompletedTask;
				self.LoadOneInThread(configType, configBytes);
			}
			List<ETTask> tasks = new List<ETTask>();

			foreach (var item in types)
			{

				tasks.Add(Load1(item, configBytes));//好像这么写还是同步加载
			}
			await ETTaskHelper.WaitAll(tasks);
//#else
            // List<Task> listTasks = new List<Task>();

            // foreach (Type type in types)
            // {
                // Task task = Task.Run(() => self.LoadOneInThread(type, configBytes));
                // listTasks.Add(task);
            // }

            // await Task.WhenAll(listTasks.ToArray());
//#endif
		}

		private static void LoadOneInThread(this ConfigComponent self, Type configType, Dictionary<string, byte[]> configBytes)
		{
				Log.Error("加载1");
			byte[] oneConfigBytes = configBytes[configType.Name];
	Log.Error("加载2");
			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);
	Log.Error("加载3");
			lock (self)
			{
				self.AllConfig[configType] = category;	
			}
				Log.Error("加载4");
		}
	}
}