using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
	public abstract class Entity<T>: Object where T : Entity<T>
	{
		[BsonElement, BsonIgnoreIfNull]
		private HashSet<Component<T>> components;

		private Dictionary<Type, Component<T>> componentDict = new Dictionary<Type, Component<T>>();

		protected Entity()
		{
		}

		protected Entity(ObjectId id): base(id)
		{
		}

		public T Clone()
		{
			return MongoHelper.FromBson<T>(MongoHelper.ToBson(this));
		}

		public K AddComponent<K>() where K : Component<T>, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = (T) this;

			if (this.componentDict.ContainsKey(component.GetComponentType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetComponentType(), component);
			return component;
		}

		public void AddComponent(Component<T> component)
		{
			if (this.componentDict.ContainsKey(component.GetComponentType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {component.GetComponentType().Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<Component<T>>();
			}
			this.components.Add(component);
			this.componentDict.Add(component.GetComponentType(), component);
		}

		public void RemoveComponent<K>() where K : Component<T>
		{
			Component<T> component;
			if (!this.componentDict.TryGetValue(typeof (K), out component))
			{
				throw new Exception($"RemoveComponent, component not exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			this.components.Remove(component);
			this.componentDict.Remove(typeof (K));

			if (this.components.Count == 0)
			{
				this.components = null;
			}
		}

		public K GetComponent<K>() where K : Component<T>
		{
			Component<T> component;
			if (!this.componentDict.TryGetValue(typeof (K), out component))
			{
				return default (K);
			}
			return (K) component;
		}

		public Component<T>[] GetComponents()
		{
			return this.components.ToArray();
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
				this.componentDict.Add(component.GetComponentType(), component);
			}
		}
	}
}