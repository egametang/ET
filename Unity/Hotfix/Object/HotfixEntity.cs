using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace Hotfix
{
	public class HotfixEntity: Disposer
	{
		public EntityType Type { get; set; }
		
		private HashSet<HotfixComponent> components = new HashSet<HotfixComponent>();
		
		private readonly Dictionary<Type, HotfixComponent> componentDict = new Dictionary<Type, HotfixComponent>();

		protected HotfixEntity()
		{
			this.Type = EntityType.None;
		}

		protected HotfixEntity(EntityType entityType)
		{
			this.Type = entityType;
		}

		protected HotfixEntity(long id, EntityType entityType): base(id)
		{
			this.Type = entityType;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (HotfixComponent component in this.GetComponents())
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

		public K AddComponent<K>() where K : HotfixComponent, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;
			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}
			if (this.components == null)
			{
				this.components = new HashSet<HotfixComponent>();
			}
			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			IAwake awake = component as IAwake;
			awake?.Awake();
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : HotfixComponent, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<HotfixComponent>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			IAwake<P1> awake = component as IAwake<P1>;
			awake?.Awake(p1);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : HotfixComponent, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<HotfixComponent>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			IAwake<P1, P2> awake = component as IAwake<P1, P2>;
			awake?.Awake(p1, p2);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : HotfixComponent, new()
		{
			K component = (K) Activator.CreateInstance(typeof (K));
			component.Owner = this;

			if (this.componentDict.ContainsKey(component.GetType()))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof (K).Name}");
			}

			if (this.components == null)
			{
				this.components = new HashSet<HotfixComponent>();
			}

			this.components.Add(component);
			this.componentDict.Add(component.GetType(), component);
			IAwake<P1, P2, P3> awake = component as IAwake<P1, P2, P3>;
			awake?.Awake(p1, p2, p3);
			return component;
		}

		public void RemoveComponent<K>() where K : HotfixComponent
		{
			HotfixComponent component;
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

		public void RemoveComponent(Type type)
		{
			HotfixComponent component;
			if (!this.componentDict.TryGetValue(type, out component))
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

		public K GetComponent<K>() where K : HotfixComponent
		{
			HotfixComponent component;
			if (!this.componentDict.TryGetValue(typeof (K), out component))
			{
				return default(K);
			}
			return (K) component;
		}

		public HotfixComponent[] GetComponents()
		{
			return components.ToArray();
		}
	}
}