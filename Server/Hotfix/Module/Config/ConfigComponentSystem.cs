using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
    {
        public override void Awake(ConfigComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class ConfigLoadSystem : LoadSystem<ConfigComponent>
    {
        public override void Load(ConfigComponent self)
        {
            self.Load();
        }
    }
    
    public static class ConfigComponentHelper
	{
		public static void Awake(this ConfigComponent self)
		{
			self.Load();
		}

		public static void Load(this ConfigComponent self)
		{
			AppType appType = StartConfigComponent.Instance.StartConfig.AppType;
			
			self.AllConfig.Clear();
			List<Type> types = Game.EventSystem.GetTypes(typeof(ConfigAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				ConfigAttribute configAttribute = attrs[0] as ConfigAttribute;
				// 只加载指定的配置
				if (!configAttribute.Type.Is(appType))
				{
					continue;
				}
				
				object obj = Activator.CreateInstance(type);

				ACategory iCategory = obj as ACategory;
				if (iCategory == null)
				{
					throw new Exception($"class: {type.Name} not inherit from ACategory");
				}
				iCategory.BeginInit();
				iCategory.EndInit();

				self.AllConfig[iCategory.ConfigType] = iCategory;
			}
		}

		public static IConfig GetOne(this ConfigComponent self, Type type)
		{
			ACategory configCategory;
			if (!self.AllConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return configCategory.GetOne();
		}

		public static IConfig Get(this ConfigComponent self, Type type, int id)
		{
			ACategory configCategory;
			if (!self.AllConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}

			return configCategory.TryGet(id);
		}

		public static IConfig TryGet(this ConfigComponent self, Type type, int id)
		{
			ACategory configCategory;
			if (!self.AllConfig.TryGetValue(type, out configCategory))
			{
				return null;
			}
			return configCategory.TryGet(id);
		}

		public static IConfig[] GetAll(this ConfigComponent self, Type type)
		{
			ACategory configCategory;
			if (!self.AllConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return configCategory.GetAll();
		}

		public static ACategory GetCategory(this ConfigComponent self, Type type)
		{
			ACategory configCategory;
			bool ret = self.AllConfig.TryGetValue(type, out configCategory);
			return ret ? configCategory : null;
		}
	}
}