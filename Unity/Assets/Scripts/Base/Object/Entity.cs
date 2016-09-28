using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	public abstract class Entity<T>: Object where T : Entity<T>
	{
		[BsonElement, BsonIgnoreIfNull]
		private HashSet<Component<T>> components = new HashSet<Component<T>>();
		private Dictionary<Type, Component<T>> componentDict = new Dictionary<Type, Component<T>>();

		public string Name { get; }

		protected Entity()
		{
			this.Name = "";
			ObjectManager.Add(this);
		}

		protected Entity(string name)
		{
			this.Name = name;
			ObjectManager.Add(this);
		}

		protected Entity(long id, string name): base(id)
		{
			this.Name = name;
			ObjectManager.Add(this);
		}

		public T Clone()
		{
			return MongoHelper.FromBson<T>(MongoHelper.ToBson(this));
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Component<T> component in this.GetComponents())
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
			ObjectManager.Remove(this.Id);
		}

		public K AddComponent<K>() where K : Component<T>, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = (T) this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			ObjectManager.Awake(component.Id);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component<T>, new()
		{
			K component = (K)Activator.CreateInstance(typeof(K));
			component.Owner = (T)this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			ObjectManager.Awake(component.Id, p1);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component<T>, new()
		{
			K component = (K)Activator.CreateInstance(typeof(K));
			component.Owner = (T)this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			ObjectManager.Awake(component.Id, p1, p2);
			return component;
		}


		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component<T>, new()
		{
			K component = (K)Activator.CreateInstance(typeof(K));
			component.Owner = (T)this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			ObjectManager.Awake(component.Id, p1, p2, p3);
			return component;
		}

		public void AddComponent(Component<T> component)
		{
			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {component.GetType().Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}
			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			ObjectManager.Awake(component.Id);
		}

		public void RemoveComponent<K>() where K : Component<T>
		{
			Component<T> component;
			if (!this.componentDict.TryGetValue(typeof (K), out component))
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

		public K GetComponent<K>() where K : Component<T>
		{
			Component<T> component;
			if (!this.componentDict.TryGetValue(typeof (K), out component))
			{
				return default(K);
			}
			return (K) component;
		}

		public Component<T>[] GetComponents()
		{
			return components.ToArray();
		}

		public override void BeginInit()
		{
			base.BeginInit();
			this.components = new HashSet<Component<T>>();
			this.componentDict = new Dictionary<Type, Component<T>>();
		}

		public override void EndInit()
		{
			base.EndInit();
			if (this.components.Count == 0)
			{
				this.components = null;
				return;
			}
			foreach (Component<T> component in this.components)
			{
				component.Owner = (T) this;
				this.componentDict.Add(component.GetType(), component);
			}
		}
	}
}