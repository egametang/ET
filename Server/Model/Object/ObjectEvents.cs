using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Base;

namespace Model
{
	public interface IObjectEvent
	{
		Type Type();
		void Set(object value);
	}

	public abstract class ObjectEvent<T> : IObjectEvent
	{
		private T value;

		protected T Get()
		{
			return value;
		}

		public void Set(object v)
		{
			this.value = (T)v;
		}

		public Type Type()
		{
			return typeof(T);
		}
	}

	public sealed class ObjectEvents
	{
		private static ObjectEvents instance;

		public static ObjectEvents Instance
		{
			get
			{
				return instance ?? (instance = new ObjectEvents());
			}
		}

		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private Dictionary<Type, IObjectEvent> disposerEvents;

		private readonly HashSet<Disposer> updates = new HashSet<Disposer>();
		private readonly HashSet<Disposer> loaders = new HashSet<Disposer>();

		private readonly Queue<Disposer> adds = new Queue<Disposer>();
		private readonly Queue<Disposer> removes = new Queue<Disposer>();

		public void Register(string name, Assembly assembly)
		{
			this.assemblies[name] = assembly;

			this.disposerEvents = new Dictionary<Type, IObjectEvent>();
			foreach (Assembly ass in this.assemblies.Values)
			{
				Type[] types = ass.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(ObjectEventAttribute), false);

					if (attrs.Length == 0)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);
					IObjectEvent objectEvent = obj as IObjectEvent;
					if (objectEvent == null)
					{
						Log.Error($"组件事件没有继承IObjectEvent: {type.Name}");
						continue;
					}
					this.disposerEvents[objectEvent.Type()] = objectEvent;
				}
			}

			this.Load();
		}

		public Assembly GetAssembly(string name)
		{
			return this.assemblies[name];
		}

		public Assembly[] GetAssemblies()
		{
			return this.assemblies.Values.ToArray();
		}

		private void Load()
		{
			foreach (Disposer disposer in this.loaders)
			{
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
				{
					continue;
				}
				ILoad iLoader = objectEvent as ILoad;
				if (iLoader == null)
				{
					continue;
				}
				objectEvent.Set(disposer);
				iLoader.Load();
			}
		}

		public void Add(Disposer disposer)
		{
			this.adds.Enqueue(disposer);
		}

		public void Remove(Disposer disposer)
		{
			this.removes.Enqueue(disposer);
		}

		public void UpdateAdd()
		{
			while (this.adds.Count > 0)
			{
				Disposer disposer = this.adds.Dequeue();
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
				{
					continue;
				}

				IUpdate iUpdate = objectEvent as IUpdate;
				if (iUpdate != null)
				{
					this.updates.Add(disposer);
				}

				ILoad iLoader = objectEvent as ILoad;
				if (iLoader != null)
				{
					this.loaders.Add(disposer);
				}
			}
		}

		public void UpdateRemove()
		{
			while (this.removes.Count > 0)
			{
				Disposer disposer = this.removes.Dequeue();
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
				{
					continue;
				}

				IUpdate iUpdate = objectEvent as IUpdate;
				if (iUpdate != null)
				{
					this.updates.Remove(disposer);
				}

				ILoad iLoader = objectEvent as ILoad;
				if (iLoader != null)
				{
					this.loaders.Remove(disposer);
				}
			}
		}

		public void Awake(Disposer disposer)
		{
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
			{
				return;
			}
			IAwake iAwake = objectEvent as IAwake;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.Set(disposer);
			iAwake.Awake();
		}

		public void Awake<P1>(Disposer disposer, P1 p1)
		{
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
			{
				return;
			}
			IAwake<P1> iAwake = objectEvent as IAwake<P1>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1);
		}

		public void Awake<P1, P2>(Disposer disposer, P1 p1, P2 p2)
		{
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
			{
				return;
			}
			IAwake<P1, P2> iAwake = objectEvent as IAwake<P1, P2>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1, p2);
		}

		public void Awake<P1, P2, P3>(Disposer disposer, P1 p1, P2 p2, P3 p3)
		{
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
			{
				return;
			}
			IAwake<P1, P2, P3> iAwake = objectEvent as IAwake<P1, P2, P3>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1, p2, p3);
		}

		public void Update()
		{
			this.UpdateAdd();
			this.UpdateRemove();

			foreach (Disposer disposer in updates)
			{
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectEvent objectEvent))
				{
					continue;
				}
				IUpdate iUpdate = objectEvent as IUpdate;
				if (iUpdate == null)
				{
					continue;
				}
				objectEvent.Set(disposer);
				try
				{
					iUpdate.Update();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}