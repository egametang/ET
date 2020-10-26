using System;
using System.Collections.Generic;


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
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof(ConfigAttribute));

			foreach (Type type in types)
			{
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
	}
}