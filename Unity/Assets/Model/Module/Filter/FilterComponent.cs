using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[ObjectSystem]
	public class FilterAwakeSystem : AwakeSystem<FilterComponent>
	{
		public override void Awake(FilterComponent self)
		{
			self.Load();
		}
	}

	[ObjectSystem]
	public class FilterLoadSystem : LoadSystem<FilterComponent>
	{
		public override void Load(FilterComponent self)
		{
			self.Load();
		}
	}
	
	
	[BsonIgnoreExtraElements]
	public class FilterComponent: Component
	{
		// 一个Filter过滤了哪些组件
		public UnOrderMultiMap<string, Type> filterComponets = new UnOrderMultiMap<string, Type>();
		
		// 一个组件被哪些Filter过滤
		public UnOrderMultiMap<Type, string> componentFilters = new UnOrderMultiMap<Type, string>();
		
		
		
		// 满足该Filter的所有entity
		public UnOrderMultiMap<string, Entity> filterEntitys = new UnOrderMultiMap<string, Entity>();
		
		// 一个entity满足哪些Filter
		public UnOrderMultiMap<Entity, string> entityFilters = new UnOrderMultiMap<Entity, string>();

		// 尽量不要在服务端热更增加或减少Filter
		public void Load()
		{
			this.filterComponets.Clear();
			this.componentFilters.Clear();

			Type parentType = this.Parent.GetType();
			List<Type> types = Game.EventSystem.GetTypes(typeof(FilterAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(FilterAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				
				FilterAttribute filterAttribute = attrs[0] as FilterAttribute;
				if (filterAttribute.ManagerTypeName != parentType.Name)
				{
					continue;
				}
				
				string filterName = type.Name;
				IFilter filter = Activator.CreateInstance(type) as IFilter;
				if (filter == null)
				{
					Log.Error($"filterSystem: {type.Name} 需要继承 IFilterSystem");
					continue;
				}

				foreach (Type filterComponent in filter.GetFilter())
				{
					this.filterComponets.Add(filterName, filterComponent);
					this.componentFilters.Add(filterComponent, filterName);
				}
			}
		}

		// 热更后挂载Filter的管理器需要清理FilterComponent，重新add entity进来
		public void Clear()
		{
			this.filterEntitys.Clear();
			this.entityFilters.Clear();
		}

		public List<Entity> GetByFilter(Type type)
		{
			List<Entity> list = this.filterEntitys[type.Name];
			return list;
		}

		public void Add(Entity entity)
		{
			foreach (var kv in this.filterComponets.GetDictionary())
			{
				string systemType = kv.Key;
				bool hasAllComponents = true;
				foreach (Type type in kv.Value)
				{
					if (entity.GetComponent(type) != null)
					{
						continue;
					}

					hasAllComponents = false;
					break;
				}

				if (!hasAllComponents)
				{
					continue;
				}

				this.filterEntitys.Add(systemType, entity);
				this.entityFilters.Add(entity, systemType);
			}
		}

		public void Remove(Entity entity)
		{
			List<string> systemTypes = this.entityFilters[entity];
			foreach (string systemType in systemTypes)
			{
				this.filterEntitys[systemType].Remove(entity);
			}
		}

		public void OnRemoveComponent(Entity entity, Type componentType)
		{
			List<string> systemTypes = this.componentFilters[componentType];
			foreach (string systemType in systemTypes)
			{
				this.filterEntitys.Remove(systemType, entity);
				this.entityFilters.Remove(entity, systemType);
			}
		}
		
		public void OnAddComponent(Entity entity, Type componentType)
		{
			List<string> systemTypes = this.componentFilters[componentType];
			foreach (string systemType in systemTypes)
			{
				this.filterEntitys.Add(systemType, entity);
				this.entityFilters.Add(entity, systemType);
			}
		}
	}
}