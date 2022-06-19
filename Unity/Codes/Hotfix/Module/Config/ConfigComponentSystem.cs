using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
	[FriendOf(typeof(ConfigComponent))]
	public static class ConfigComponentSystem
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
		
		public static void LoadOneConfig(this ConfigComponent self, Type configType)
		{
			byte[] oneConfigBytes = Game.EventSystem.Callback<string, byte[]>(CallbackType.GetOneConfigBytes, configType.FullName);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			self.AllConfig[configType] = category;
		}
		
		public static void Load(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			Game.EventSystem.Callback(CallbackType.GetAllConfigBytes, configBytes);

			foreach (Type type in types)
			{
				self.LoadOneInThread(type, configBytes);
			}
		}
		
		public static async ETTask LoadAsync(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
			
			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			Game.EventSystem.Callback(CallbackType.GetAllConfigBytes, configBytes);

			using (ListComponent<Task> listTasks = ListComponent<Task>.Create())
			{
				foreach (Type type in types)
				{
					Task task = Task.Run(() => self.LoadOneInThread(type, configBytes));
					listTasks.Add(task);
				}

				await Task.WhenAll(listTasks.ToArray());
			}
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