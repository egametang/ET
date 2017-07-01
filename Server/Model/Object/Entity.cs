using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class Entity: Disposer
	{
		public EntityType Type { get; set; }

		[BsonElement]
		[BsonIgnoreIfNull]
		private HashSet<Component> components = new HashSet<Component>();

		[BsonIgnore]
		private Dictionary<Type, Component> componentDict = new Dictionary<Type, Component>();

		protected Entity()
		{
			ObjectEvents.Instance.Add(this);
		}

		protected Entity(long id): base(id)
		{
			ObjectEvents.Instance.Add(this);
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
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
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
			ObjectEvents.Instance.Awake(component);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
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
			ObjectEvents.Instance.Awake(component, p1);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
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
			ObjectEvents.Instance.Awake(component, p1, p2);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
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
			ObjectEvents.Instance.Awake(component, p1, p2, p3);
			return component;
		}

		public void AddComponent(Component component)
		{
			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {component.GetType().Name}");
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
			ObjectEvents.Instance.Awake(component);
		}

		public void RemoveComponent<K>() where K : Component
		{
			if (!this.componentDict.TryGetValue(typeof(K), out Component component))
			{
				return;
			}

			this.components.Remove(component);
			this.componentDict.Remove(typeof (K));
			if (this.components.Count == 0)
			{
				this.components = null;
			}
			component.Dispose();
		}

		public void RemoveComponent(Type type)
		{
			if (!this.componentDict.TryGetValue(type, out Component component))
			{
				return;
			}

			this.components.Remove(component);
			this.componentDict.Remove(type);
			if (this.components.Count == 0)
			{
				this.components = null;
			}
			component.Dispose();
		}

		public K GetComponent<K>() where K : Component
		{
			if (!this.componentDict.TryGetValue(typeof(K), out Component component))
			{
				return default(K);
			}
			return (K) component;
		}

		public Component[] GetComponents()
		{
			return components.ToArray();
		}

		public override void BeginInit()
		{
			base.BeginInit();
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

		public override void EndInit()
		{
			base.EndInit();

			ObjectEvents.Instance.Add(this);

			if (this.components.Count == 0)
			{
				this.components = null;
				return;
			}
			foreach (Component component in this.components)
			{
				component.Owner = this;
				this.componentDict.Add(component.GetType(), component);
			}
		}
	}
}