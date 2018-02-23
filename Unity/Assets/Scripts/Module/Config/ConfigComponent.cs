using System;
using System.Collections.Generic;

namespace Model
{
	[ObjectSystem]
	public class ConfigComponentAwakeSystem : AwakeSystem<ConfigComponent>
	{
		public override void Awake(ConfigComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class ConfigComponentLoadSystem : LoadSystem<ConfigComponent>
	{
		public override void Load(ConfigComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
	/// </summary>
	public class ConfigComponent: Component
	{
		private Dictionary<Type, ICategory> allConfig;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allConfig = new Dictionary<Type, ICategory>();
			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(type);

				ICategory iCategory = obj as ICategory;
				if (iCategory == null)
				{
					throw new Exception($"class: {type.Name} not inherit from ACategory");
				}
				iCategory.BeginInit();
				iCategory.EndInit();

				this.allConfig[iCategory.ConfigType] = iCategory;
			}
		}

		public T GetOne<T>() where T : AConfig
		{
			Type type = typeof (T);
			ICategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return ((ACategory<T>) configCategory).GetOne();
		}

		public T Get<T>(long id) where T : AConfig
		{
			Type type = typeof (T);
			ICategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return ((ACategory<T>) configCategory)[id];
		}

		public T TryGet<T>(int id) where T : AConfig
		{
			Type type = typeof (T);
			ICategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				return default(T);
			}
			return ((ACategory<T>) configCategory).TryGet(id);
		}

		public T[] GetAll<T>() where T : AConfig
		{
			Type type = typeof (T);
			ICategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return ((ACategory<T>) configCategory).GetAll();
		}

		public T GetCategory<T>() where T : class, ICategory, new()
		{
			T t = new T();
			Type type = t.ConfigType;
			ICategory configCategory;
			bool ret = this.allConfig.TryGetValue(type, out configCategory);
			return ret? (T)configCategory : null;
		}
	}
}