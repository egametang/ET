using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	[BsonIgnoreExtraElements]
	public class Entity : Component
	{
		[BsonElement]
		[BsonIgnoreIfNull]
		private readonly HashSet<Component> components;

		[BsonIgnore]
		private readonly Dictionary<Type, Component> componentDict;

		protected Entity()
		{
			this.Id = IdGenerater.GenerateId();
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

		protected Entity(long id)
		{
			this.Id = id;
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
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

			this.components.Clear();
			this.componentDict.Clear();
		}

		public K AddComponent<K>() where K : Component, new()
		{
			K component = ComponentFactory.CreateWithParent<K>(this);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			K component = ComponentFactory.CreateWithParent<K, P1>(this, p1);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			K component = ComponentFactory.CreateWithParent<K, P1, P2>(this, p1, p2);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(component.GetType(), component);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			K component = ComponentFactory.CreateWithParent<K, P1, P2, P3>(this, p1, p2, p3);

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			if (component is ISerializeToEntity)
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

			this.components.Remove(component);
			this.componentDict.Remove(typeof(K));

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
	}
}