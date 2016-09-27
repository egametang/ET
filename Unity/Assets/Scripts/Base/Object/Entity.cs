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

		[BsonIgnore]
		public T Parent { get; set; }
		public string Name { get; }

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<long, T> idChildren;

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<string, T> nameChildren;

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


		public int Count
		{
			get
			{
				return this.idChildren.Count;
			}
		}

		public void Add(T t)
		{
			t.Parent = (T)this;
			if (this.idChildren == null)
			{
				this.idChildren = new Dictionary<long, T>();
				this.nameChildren = new Dictionary<string, T>();
			}
			this.idChildren.Add(t.Id, t);
			this.nameChildren.Add(t.Name, t);
		}

		private void Remove(T t)
		{
			this.idChildren.Remove(t.Id);
			this.nameChildren.Remove(t.Name);
			if (this.idChildren.Count == 0)
			{
				this.idChildren = null;
				this.nameChildren = null;
			}
			t.Dispose();
		}

		public void Remove(long id)
		{
			T t;
			if (!this.idChildren.TryGetValue(id, out t))
			{
				return;
			}
			this.Remove(t);
		}

		public void Remove(string name)
		{
			T t;
			if (!this.nameChildren.TryGetValue(name, out t))
			{
				return;
			}
			this.Remove(t);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (T t in this.idChildren.Values.ToArray())
			{
				t.Dispose();
			}

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
			this.Parent?.Remove(this.Id);

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