using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public class Entity : ComponentWithId
	{
		[BsonElement]
		[BsonIgnoreIfNull]
		private HashSet<Component> components;

		[BsonIgnore]
		private Dictionary<Type, Component> componentDict;

		public Entity()
		{
			this.components = new HashSet<Component>();
			this.componentDict = new Dictionary<Type, Component>();
		}

		protected Entity(long id): base(id)
		{
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
					Log.Error(e);
				}
			}

			this.components.Clear();
			this.componentDict.Clear();
		}
		
		public Component AddComponent(Component component)
		{
			Type type = component.GetType();
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public Component AddComponent(Type type)
		{
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}

			Component component = ComponentFactory.CreateWithParent(type, this);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K>() where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K>(this);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1>(this, p1);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2>(this, p1, p2);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2, P3>(this, p1, p2, p3);
			
			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}
			this.componentDict.Add(type, component);
			return component;
		}

		public void RemoveComponent<K>() where K : Component
		{
			Type type = typeof (K);
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}

			this.components.Remove(component);
			this.componentDict.Remove(type);

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

		public Component GetComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return null;
			}
			return component;
		}

		public Component[] GetComponents()
		{
			return this.componentDict.Values.ToArray();
		}

		public override void EndInit()
		{
			try
			{
				base.EndInit();
				
				this.componentDict.Clear();

				if (this.components != null)
				{
					foreach (Component component in this.components)
					{
						component.Parent = this;
						this.componentDict.Add(component.GetType(), component);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public override void BeginSerialize()
		{
			base.BeginSerialize();

			foreach (Component component in this.components)
			{
				component.BeginSerialize();
			}
		}

		public override void EndDeSerialize()
		{
			base.EndDeSerialize();
			
			foreach (Component component in this.components)
			{
				component.EndDeSerialize();
			}
		}
	}
}