﻿using System;
using System.Collections.Generic;

namespace ETModel
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
	public class ConfigComponent: Entity
	{
		private Dictionary<Type, ACategory> allConfig = new Dictionary<Type, ACategory>();

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allConfig.Clear();
			HashSet<Type> types = Game.EventSystem.GetTypes(typeof(ConfigAttribute));

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (ConfigAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				ConfigAttribute configAttribute = attrs[0] as ConfigAttribute;
				// 只加载指定的配置
				if (!configAttribute.Type.Is(AppType.ClientM))
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

				this.allConfig[iCategory.ConfigType] = iCategory;
			}
		}

		public IConfig GetOne(Type type)
		{
			ACategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return configCategory.GetOne();
		}

		public IConfig Get(Type type, int id)
		{
			ACategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}

			return configCategory.TryGet(id);
		}

		public IConfig TryGet(Type type, int id)
		{
			ACategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				return null;
			}
			return configCategory.TryGet(id);
		}

		public IConfig[] GetAll(Type type)
		{
			ACategory configCategory;
			if (!this.allConfig.TryGetValue(type, out configCategory))
			{
				throw new Exception($"ConfigComponent not found key: {type.FullName}");
			}
			return configCategory.GetAll();
		}

		public ACategory GetCategory(Type type)
		{
			ACategory configCategory;
			bool ret = this.allConfig.TryGetValue(type, out configCategory);
			return ret ? configCategory : null;
		}
	}
}