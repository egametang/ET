using System;
using System.Collections.Generic;
using ETModel;
using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ETHotfix
{
	[Flags]
	public enum EntityStatus: byte
	{
		None = 0,
		IsFromPool = 0x01,
		IsRegister = 0x02,
		IsComponent = 0x04
	}
	
	public partial class Entity : Object, IDisposable
	{
		private static readonly Pool<HashSet<Entity>> hashSetPool = new Pool<HashSet<Entity>>();
		
		private static readonly Pool<Dictionary<Type, Entity>> dictPool = new Pool<Dictionary<Type, Entity>>();
		
		private static readonly Pool<Dictionary<long, Entity>> childrenPool = new Pool<Dictionary<long, Entity>>();
		
		
		[BsonIgnore]
		public long InstanceId { get; set; }
		
#if !SERVER
		public static GameObject Global { get; } = GameObject.Find("/Global");
		
		[BsonIgnore]
		public GameObject ViewGO { get; set; }
#endif

		[BsonIgnore]
		private EntityStatus status = EntityStatus.None;

		[BsonIgnore]
		public bool IsFromPool
		{
			get
			{
				return (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
			}
			set
			{
				if (value)
				{
					this.status |= EntityStatus.IsFromPool;
				}
				else
				{
					this.status &= ~EntityStatus.IsFromPool;
				}

				if (this.InstanceId == 0)
				{
					this.InstanceId = IdGenerater.GenerateId();
				}

				this.IsRegister = value;
			}
		}
		
		[BsonIgnore]
		private bool IsRegister
		{
			get
			{
				return (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
			}
			set
			{
				if (this.IsRegister == value)
				{
					return;
				}
				if (value)
				{
					this.status |= EntityStatus.IsRegister;
				}
				else
				{
					this.status &= ~EntityStatus.IsRegister;
				}
				Game.EventSystem.RegisterSystem(this, value);
			}
		}
		
		[BsonIgnore]
		private bool IsComponent
		{
			get
			{
				return (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
			}
			set
			{
				if (value)
				{
					this.status |= EntityStatus.IsComponent;
				}
				else
				{
					this.status &= ~EntityStatus.IsComponent;
				}
			}
		}

		[BsonIgnore]
		public bool IsDisposed
		{
			get
			{
				return this.InstanceId == 0;
			}
		}

		[BsonIgnore]
		protected Entity parent;
		
		[BsonIgnore]
		public Entity Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				if (value == null)
				{
					throw new Exception($"cant set parent null: {this.GetType().Name}");
				}
				
				if (this.parent != null) // 之前有parent
				{
					// parent相同，不设置
					if (this.parent.InstanceId == value.InstanceId)
					{
						Log.Error($"重复设置了Parent: {this.GetType().Name} parent: {this.parent.GetType().Name}");
						return;
					}
					
					this.parent.RemoveChild(this);
					
					this.parent = value;
					this.parent.AddChild(this);
					
					this.Domain = this.parent.domain;
				}
				else
				{
					this.parent = value;
					this.parent.AddChild(this);
				
					this.IsComponent = false;
				
					AfterSetParent();
				}
			}
		}
		
		// 该方法只能在AddComponent中调用，其他人不允许调用
		[BsonIgnore]
		private Entity ComponentParent
		{
			set
			{
				if (this.parent != null)
				{
					throw new Exception($"Component parent is null: {this.GetType().Name}");
				}

				this.parent = value;
				
				this.IsComponent = true;

				AfterSetParent();
			}
		}

		private void AfterSetParent()
		{
			if (this.parent.domain != null)
			{
				this.Domain = this.parent.domain;
			}

			// 检测自己的domain是不是跟父亲一样
			if (this.Domain != null && this.parent.Domain != null && this.Domain.InstanceId != this.parent.Domain.InstanceId && !(this is Scene))
			{
				Log.Error($"自己的domain跟parent不一样: {this.GetType().Name}");
			}
#if !SERVER
			if (this.ViewGO != null && this.parent.ViewGO != null)
			{
				this.ViewGO.transform.SetParent(this.parent.ViewGO.transform, false);
			}
#endif
		}

		public T GetParent<T>() where T : Entity
		{
			return this.Parent as T;
		}
		
		public override string ToString()
		{
			return MongoHelper.ToJson(this);
		}
		
		
		[BsonIgnoreIfDefault]
		[BsonDefaultValue(0L)]
		[BsonElement]
		[BsonId]
		public long Id { get; set; }

		[BsonIgnore]
		protected Entity domain;

		[BsonIgnore]
		public Entity Domain 
		{
			get
			{
				return this.domain;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				
				Entity preDomain = this.domain;
				this.domain = value;
				
				if (!(this.domain is Scene))
				{
					throw new Exception($"domain is not scene: {this.GetType().Name}");
				}
				
				this.domain = value;
				
				// 是否注册跟parent一致
				if (this.parent != null)
				{
					this.IsRegister = this.Parent.IsRegister;
				}

				// 递归设置孩子的Domain
				if (this.children != null)
				{
					foreach (Entity entity in this.children.Values)
					{
						entity.Domain = this.domain;
					}
				}
				
				if (this.components != null)
				{
					foreach (Entity component in this.components.Values)
					{
						component.Domain = this.domain;
					}
				}
				
				if (preDomain == null && !this.IsFromPool)
				{
					Game.EventSystem.Deserialize(this);
				}
			}
		}

		[BsonElement("Children")]
		[BsonIgnoreIfNull]
		private HashSet<Entity> childrenDB;

		[BsonIgnore]
		private Dictionary<long, Entity> children;
		
		[BsonIgnore]
		public Dictionary<long, Entity> Children 
		{
			get
			{
				if (this.children == null)
				{
					this.children = childrenPool.Fetch();
				}

				return this.children;
			}
		}
		
		private void AddChild(Entity entity)
		{
			this.Children.Add(entity.Id, entity);
			this.AddChildDB(entity);
		}
		
		private void RemoveChild(Entity entity)
		{
			if (this.children == null)
			{
				return;
			}

			this.children.Remove(entity.Id);

			if (this.children.Count == 0)
			{
				childrenPool.Recycle(this.children);
				this.children = null;
			}
			this.RemoveChildDB(entity);
		}
		
		private void AddChildDB(Entity entity)
		{
			if (!(entity is ISerializeToEntity))
			{
				return;
			}
			if (this.childrenDB == null)
			{
				this.childrenDB = hashSetPool.Fetch();
			}
			this.childrenDB.Add(entity);
		}
		
		private void RemoveChildDB(Entity entity)
		{
			if (!(entity is ISerializeToEntity))
			{
				return;
			}

			if (this.childrenDB == null)
			{
				return;
			}
			
			this.childrenDB.Remove(entity);
			
			if (this.childrenDB.Count == 0)
			{
				if (this.IsFromPool)
				{
					hashSetPool.Recycle(this.childrenDB);
					this.childrenDB = null;
				}
			}
		}

		[BsonElement("C")]
		[BsonIgnoreIfNull]
		private HashSet<Entity> componentsDB;
		
		[BsonIgnore]
		private Dictionary<Type, Entity> components;

		[BsonIgnore]
		public Dictionary<Type, Entity> Components
		{
			get
			{
				return this.components;
			}
		}
		
		protected Entity()
		{
			this.InstanceId = IdGenerater.GenerateId();

#if !SERVER
			if (!this.GetType().IsDefined(typeof (HideInHierarchy), true))
			{
				this.ViewGO = new GameObject();
				this.ViewGO.name = this.GetType().Name;
				this.ViewGO.layer = LayerNames.GetLayerInt(LayerNames.HIDDEN);
				this.ViewGO.transform.SetParent(Global.transform, false);
				this.ViewGO.AddComponent<ComponentView>().Component = this;
			}
#endif
		}

		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			long instanceId = this.InstanceId;
			this.InstanceId = 0;
			
			Game.EventSystem.Remove(instanceId);

			// 触发Destroy事件
			Game.EventSystem.Destroy(this);

			this.domain = null;
		
			// 清理Children
			if (this.children != null)
			{
				var deletes = this.children;
				this.children = null;

				foreach (Entity child in deletes.Values)
				{
					child.Dispose();
				}

				deletes.Clear();
				childrenPool.Recycle(deletes);
				
				if (this.childrenDB != null)
				{
					this.childrenDB.Clear();
					// 从池中创建的才需要回到池中,从db中不需要回收
					if (this.IsFromPool)
					{
						hashSetPool.Recycle(this.childrenDB);
						this.childrenDB = null;
					}
				}
			}

			// 清理Component
			if (this.components != null)
			{
				var deletes = this.components;
				this.components = null;
				foreach (var kv in deletes)
				{
					kv.Value.Dispose();
				}
				
				deletes.Clear();
				dictPool.Recycle(deletes);
				
				// 从池中创建的才需要回到池中,从db中不需要回收
				if (this.componentsDB != null)
				{
					this.componentsDB.Clear();
					
					if (this.IsFromPool)
					{
						hashSetPool.Recycle(this.componentsDB);
						this.componentsDB = null;
					}
				}
			}

			if (this.IsComponent)
			{
				this.parent?.RemoveComponent(this);
			}
			else
			{
				this.parent?.RemoveChild(this);	
			}

			this.parent = null;

			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}
			else
			{
#if !SERVER
				if (this.ViewGO != null)
				{
					UnityEngine.Object.Destroy(this.ViewGO);
				}
#endif
			}

			status = EntityStatus.None;
		}
		
		public override void EndInit()
		{
			try
			{
				if (this.childrenDB != null)
				{
					foreach (Entity child in this.childrenDB)
					{
						child.IsComponent = false;
						this.AddChild(child);
						child.parent = this;
					}
				}
				
				if (this.componentsDB != null)
				{
					foreach (Entity component in this.componentsDB)
					{
						component.IsComponent = true;
						this.AddToComponent(component.GetType(), component);
						component.parent = this;
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		private void AddToComponentsDB(Entity component)
		{
			if (this.componentsDB == null)
			{
				this.componentsDB = hashSetPool.Fetch();
			}

			this.componentsDB.Add(component);
		}
		
		private void RemoveFromComponentsDB(Entity component)
		{
			if (this.componentsDB == null)
			{
				return;
			}
			this.componentsDB.Remove(component);
			if (this.componentsDB.Count == 0 && this.IsFromPool)
			{
				hashSetPool.Recycle(this.componentsDB);
				this.componentsDB = null;
			}
		}
		
		private void AddToComponent(Type type, Entity component)
		{
			if (this.components == null)
			{
				this.components = dictPool.Fetch();
			}

			this.components.Add(type, component);
			
			if (component is ISerializeToEntity)
			{
				this.AddToComponentsDB(component);
			}
		}
		
		private void RemoveFromComponent(Type type, Entity component)
		{
			if (this.components == null)
			{
				return;
			}
			
			this.components.Remove(type);
			
			if (this.components.Count == 0 && this.IsFromPool)
			{
				dictPool.Recycle(this.components);
				this.components = null;
			}
			
			this.RemoveFromComponentsDB(component);
		}
		
		public Entity AddComponent(Entity component)
		{
			component.ComponentParent = this;
			
			Type type = component.GetType();
			
			this.AddToComponent(type, component);

			return component;
		}

		public Entity AddComponent(Type type)
		{
			Entity component = CreateWithComponentParent(type);

			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponent<K>() where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K>();

			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K, P1>(p1);
			
			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Entity, new()
		{
			Type type = typeof (K);

			K component = CreateWithComponentParent<K, P1, P2>(p1, p2);
			
			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K, P1, P2, P3>(p1, p2, p3);
			
			this.AddToComponent(type, component);
			
			return component;
		}
		
		public K AddComponentNoPool<K>() where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K>(false);

			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponentNoPool<K, P1>(P1 p1) where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K, P1>(p1, false);
			
			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponentNoPool<K, P1, P2>(P1 p1, P2 p2) where K : Entity, new()
		{
			Type type = typeof (K);

			K component = CreateWithComponentParent<K, P1, P2>(p1, p2, false);
			
			this.AddToComponent(type, component);
			
			return component;
		}

		public K AddComponentNoPool<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Entity, new()
		{
			Type type = typeof (K);
			
			K component = CreateWithComponentParent<K, P1, P2, P3>(p1, p2, p3, false);
			
			this.AddToComponent(type, component);
			
			return component;
		}

		public void RemoveComponent<K>() where K : Entity
		{
			if (this.IsDisposed)
			{
				return;
			}

			if (this.components == null)
			{
				return;
			}
			
			Type type = typeof (K);
			Entity c = this.GetComponent(type);
			if (c == null)
			{
				return;
			}

			this.RemoveFromComponent(type, c);
			c.Dispose();
		}
		
		public void RemoveComponent(Entity component)
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			if (this.components == null)
			{
				return;
			}

			Type type = component.GetType();
			Entity c = this.GetComponent(component.GetType());
			if (c == null)
			{
				return;
			}
			if (c.InstanceId != component.InstanceId)
			{
				return;
			}
			
			this.RemoveFromComponent(type, c);
			c.Dispose();
		}

		public void RemoveComponent(Type type)
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			Entity c = this.GetComponent(type);
			if (c == null)
			{
				return;
			}

			RemoveFromComponent(type, c);
			c.Dispose();
		}

		public K GetComponent<K>() where K : Entity
		{
			if (this.components == null)
			{
				return null;
			}
			Entity component;
			if (!this.components.TryGetValue(typeof(K), out component))
			{
				return default(K);
			}
			return (K)component;
		}

		public Entity GetComponent(Type type)
		{
			if (this.components == null)
			{
				return null;
			}
			Entity component;
			if (!this.components.TryGetValue(type, out component))
			{
				return null;
			}
			return component;
		}
	}
}