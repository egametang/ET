using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	[BsonKnownTypes(typeof(EntityDB))]
	public class Entity : Disposer, ISupportInitialize
	{
		[BsonIgnore]
		public Entity Parent { get; set; }

		[BsonElement]
		[BsonIgnoreIfNull]
		private HashSet<Component> components;

		[BsonIgnore]
		private Dictionary<Type, Component> componentDict = new Dictionary<Type, Component>();

		protected Entity()
		{
			this.Id = IdGenerater.GenerateId();
		}

		protected Entity(long id)
		{
			this.Id = id;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Component component in this.GetComponents())
			{
				try
				{
					component.Dispose();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public K AddComponent<K>() where K : Component, new()
		{
			K component = ComponentFactory.Create<K>(this);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component>();
			}

			if (component is ComponentDB)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			K component = ComponentFactory.Create<K, P1>(this, p1);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component>();
			}

			if (component is ComponentDB)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			K component = ComponentFactory.Create<K, P1, P2>(this, p1, p2);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component>();
			}

			if (component is ComponentDB)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			K component = ComponentFactory.Create<K, P1, P2, P3>(this, p1, p2, p3);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component>();
			}

			if (component is ComponentDB)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public void RemoveComponent<K>() where K : Component
		{
			Component component;
			if (!this.componentDict.TryGetValue(typeof(K), out component))
			{
				return;
			}

			this.components?.Remove(component);
			this.componentDict.Remove(typeof(K));
			if (this.components != null && this.components.Count == 0)
			{
				this.components = null;
			}
			component.Dispose();
		}

		public void RemoveComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}

			this.components?.Remove(component);
			this.componentDict.Remove(type);
			if (this.components != null && this.components.Count == 0)
			{
				this.components = null;
			}
			component.Dispose();
		}

		public K GetComponent<K>() where K : Component
		{
			Component component;
			if (!this.componentDict.TryGetValue(typeof(K), out component))
			{
				return default(K);
			}
			return (K)component;
		}

		public Component[] GetComponents()
		{
			return this.componentDict.Values.ToArray();
		}

		public virtual void BeginInit()
		{
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

		public virtual void EndInit()
		{
			ObjectEvents.Instance.Add(this);

			if (this.components != null && this.components.Count == 0)
			{
				this.components = null;
			}
			if (this.components != null)
			{
				foreach (Component component in this.components)
				{
					component.Entity = this;
					this.componentDict.Add(component.GetType(), component);
				}
			}
		}
	}
}