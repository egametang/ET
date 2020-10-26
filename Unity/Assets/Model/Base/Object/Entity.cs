using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ET
{
	[Flags]
	public enum EntityStatus: byte
	{
		None = 0,
		IsFromPool = 1,
		IsRegister = 1 << 1,
		IsComponent = 1 << 2,
		IsCreate = 1 << 3,
	}
	
	public partial class Entity : Object
	{
		private static readonly Pool<HashSet<Entity>> hashSetPool = new Pool<HashSet<Entity>>();
		
		private static readonly Pool<Dictionary<Type, Entity>> dictPool = new Pool<Dictionary<Type, Entity>>();
		
		private static readonly Pool<Dictionary<long, Entity>> childrenPool = new Pool<Dictionary<long, Entity>>();
		
		
		[BsonIgnore]
		public long InstanceId { get; set; }

		protected Entity()
		{
		}

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
			}
		}
		
		[BsonIgnore]
		public bool IsRegister
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

				EventSystem.Instance.RegisterSystem(this, value);
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
		public bool IsCreate
		{
			get
			{
				return (this.status & EntityStatus.IsCreate) == EntityStatus.IsCreate;
			}
			set
			{
				if (value)
				{
					this.status |= EntityStatus.IsCreate;
				}
				else
				{
					this.status &= ~EntityStatus.IsCreate;
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
					throw new Exception($"Component parent is not null: {this.GetType().Name}");
				}

				this.parent = value;
				
				this.IsComponent = true;

				AfterSetParent();
			}
		}

		private void AfterSetParent()
		{
			this.Domain = this.parent.domain;

#if UNITY_EDITOR
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
			return this.ToJson();
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
				
				//if (!(this.domain is Scene))
				//{
				//	throw new Exception($"domain is not scene: {this.GetType().Name}");
				//}
				
				if (preDomain == null)
				{
					this.InstanceId = IdGenerater.GenerateInstanceId();

					// 反序列化出来的需要设置父子关系
					if (!this.IsCreate)
					{
						if (this.componentsDB != null)
						{
							foreach (Entity component in this.componentsDB)
							{
								component.IsComponent = true;
								this.Components.Add(component.GetType(), component);
								component.parent = this;
							}
						}

						if (this.childrenDB != null)
						{
							foreach (Entity child in this.childrenDB)
							{
								child.IsComponent = false;
								this.Children.Add(child.Id, child);
								child.parent = this;
							}
						}
					}
				}
				
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
				
				if (preDomain == null && !this.IsCreate)
				{
					EventSystem.Instance.Deserialize(this);
				}
			}
		}

		[BsonElement("Children")]
		[BsonIgnoreIfNull]
		protected HashSet<Entity> childrenDB;

		[BsonIgnore]
		protected Dictionary<long, Entity> children;
		
		[BsonIgnore]
		public Dictionary<long, Entity> Children 
		{
			get
			{
				return this.children ?? (this.children = childrenPool.Fetch());
			}
		}
		
		public void AddChild(Entity entity)
		{
			this.Children.Add(entity.Id, entity);
			this.AddChildDB(entity);
		}
		
		public void RemoveChild(Entity entity)
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
		
		public void RemoveChildDB(Entity entity)
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

		public void RemoveAllChild()
		{
			this.children.Clear();
			this.childrenDB.Clear();
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
				return this.components ?? (this.components = dictPool.Fetch());
			}
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			EventSystem.Instance.Remove(this.InstanceId);
			this.InstanceId = 0;
			
			// 清理Component
			if (this.components != null)
			{
				foreach (var kv in this.components)
				{
					kv.Value.Dispose();
				}
				
				this.components.Clear();
				dictPool.Recycle(this.components);
				this.components = null;
				
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
			
			// 清理Children
			if (this.children != null)
			{
				foreach (Entity child in this.children.Values)
				{
					child.Dispose();
				}

				this.children.Clear();
				childrenPool.Recycle(this.children);
				this.children = null;
				
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
			
			// 触发Destroy事件
			EventSystem.Instance.Destroy(this);
			
			this.domain = null;

			if (this.parent != null && !this.parent.IsDisposed)
			{
				if (this.IsComponent)
				{
					this.parent.RemoveComponent(this);
				}
				else
				{
					this.parent.RemoveChild(this);
				}
			}

			this.parent = null;

			if (this.IsFromPool)
			{
				ObjectPool.Instance.Recycle(this);
			}
			else
			{
				base.Dispose();
			}

			status = EntityStatus.None;
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