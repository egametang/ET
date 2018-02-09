using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Model
{
	public enum DLLType
	{
		Model,
		Hotfix,
	}

	public interface IObjectSystem
	{
		Type Type();
		void Set(object value);
	}

	public abstract class ObjectSystem<T> : IObjectSystem
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

	public sealed class EventSystem
	{
		private readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();

		private readonly Dictionary<EventIdType, List<object>> allEvents = new Dictionary<EventIdType, List<object>>();

		private readonly Dictionary<Type, IObjectSystem> disposerEvents = new Dictionary<Type, IObjectSystem>();

		private Queue<Disposer> updates = new Queue<Disposer>();
		private Queue<Disposer> updates2 = new Queue<Disposer>();

		private readonly Queue<Disposer> starts = new Queue<Disposer>();

		private Queue<Disposer> loaders = new Queue<Disposer>();
		private Queue<Disposer> loaders2 = new Queue<Disposer>();

		private readonly HashSet<Disposer> unique = new HashSet<Disposer>();

		public void Add(DLLType dllType, Assembly assembly)
		{
			this.assemblies[dllType] = assembly;

			this.disposerEvents.Clear();

			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ObjectSystemAttribute), false);

				if (attrs.Length == 0)
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);
				IObjectSystem objectSystem = obj as IObjectSystem;
				if (objectSystem == null)
				{
					Log.Error($"组件事件没有继承IObjectEvent: {type.Name}");
					continue;
				}
				this.disposerEvents[objectSystem.Type()] = objectSystem;
			}


			allEvents.Clear();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					if (!this.allEvents.ContainsKey((EventIdType)aEventAttribute.Type))
					{
						this.allEvents.Add((EventIdType)aEventAttribute.Type, new List<object>());
					}
					this.allEvents[(EventIdType)aEventAttribute.Type].Add(obj);
				}
			}

			this.Load();
		}

		public Assembly Get(DLLType dllType)
		{
			return this.assemblies[dllType];
		}

		public Assembly[] GetAll()
		{
			return this.assemblies.Values.ToArray();
		}

		public void Add(Disposer disposer)
		{
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
			{
				return;
			}

			if (objectEvent is ILoad)
			{
				this.loaders.Enqueue(disposer);
			}

			if (objectEvent is IUpdate)
			{
				this.updates.Enqueue(disposer);
			}

			if (objectEvent is IStart)
			{
				this.starts.Enqueue(disposer);
			}
		}

		public void Awake(Disposer disposer)
		{
			this.Add(disposer);

			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
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
			this.Add(disposer);

			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
			{
				throw new Exception($"{disposer.GetType().Name} not found awake1");
			}
			IAwake<P1> iAwake = objectEvent as IAwake<P1>;
			if (iAwake == null)
			{
				throw new Exception($"{disposer.GetType().Name} not found awake1");
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1);
		}

		public void Awake<P1, P2>(Disposer disposer, P1 p1, P2 p2)
		{
			this.Add(disposer);

			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
			{
				throw new Exception($"{disposer.GetType().Name} not found awake2");
			}
			IAwake<P1, P2> iAwake = objectEvent as IAwake<P1, P2>;
			if (iAwake == null)
			{
				throw new Exception($"{disposer.GetType().Name} not found awake2");
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1, p2);
		}

		public void Awake<P1, P2, P3>(Disposer disposer, P1 p1, P2 p2, P3 p3)
		{
			this.Add(disposer);

			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
			{
				throw new Exception($"{disposer.GetType().Name} not found awake3");
			}
			IAwake<P1, P2, P3> iAwake = objectEvent as IAwake<P1, P2, P3>;
			if (iAwake == null)
			{
				throw new Exception($"{disposer.GetType().Name} not found awake3");
			}
			objectEvent.Set(disposer);
			iAwake.Awake(p1, p2, p3);
		}

		public void Load()
		{
			unique.Clear();
			while (this.loaders.Count > 0)
			{
				Disposer disposer = this.loaders.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				if (!this.unique.Add(disposer))
				{
					continue;
				}

				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
				{
					continue;
				}

				this.loaders2.Enqueue(disposer);

				ILoad iLoad = objectEvent as ILoad;
				if (iLoad == null)
				{
					continue;
				}
				objectEvent.Set(disposer);
				try
				{
					iLoad.Load();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}

			ObjectHelper.Swap(ref this.loaders, ref this.loaders2);
		}

		private void Start()
		{
			unique.Clear();
			while (this.starts.Count > 0)
			{
				Disposer disposer = this.starts.Dequeue();

				if (!this.unique.Add(disposer))
				{
					continue;
				}

				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
				{
					continue;
				}
				IStart iStart = objectEvent as IStart;
				if (iStart == null)
				{
					continue;
				}
				objectEvent.Set(disposer);
				iStart.Start();
			}
		}

		public void Update()
		{
			this.Start();

			unique.Clear();
			while (this.updates.Count > 0)
			{
				Disposer disposer = this.updates.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				if (!this.unique.Add(disposer))
				{
					continue;
				}

				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out IObjectSystem objectEvent))
				{
					continue;
				}

				this.updates2.Enqueue(disposer);

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
			
			ObjectHelper.Swap(ref this.updates, ref this.updates2);
		}

		public void Run(EventIdType type)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (object obj in iEvents)
			{
				try
				{
					IEvent iEvent = (IEvent)obj;
					iEvent.Run();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A>(EventIdType type, A a)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					IEvent<A> iEvent = (IEvent<A>)obj;
					iEvent.Run(a);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B>(EventIdType type, A a, B b)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					IEvent<A, B> iEvent = (IEvent<A, B>)obj;
					iEvent.Run(a, b);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C>(EventIdType type, A a, B b, C c)
		{
			List<object> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					IEvent<A, B, C> iEvent = (IEvent<A, B, C>)obj;
					iEvent.Run(a, b, c);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}
	}
}