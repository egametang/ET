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
		Editor,
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
		private static EventSystem instance;

		public static EventSystem Instance
		{
			get
			{
				return instance ?? (instance = new EventSystem());
			}
		}

		private Assembly hotfixAssembly;

		public Assembly HotfixAssembly
		{
			get
			{
				return this.hotfixAssembly;
			}
			set
			{
				this.hotfixAssembly = value;
			}
		}

		private readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();

		private readonly Dictionary<EventIdType, List<IEventMethod>> allEvents = new Dictionary<EventIdType, List<IEventMethod>>();

		private readonly Dictionary<Type, IObjectSystem> disposerEvents = new Dictionary<Type, IObjectSystem>();

		private Queue<Disposer> updates = new Queue<Disposer>();
		private Queue<Disposer> updates2 = new Queue<Disposer>();

		private readonly Queue<Disposer> starts = new Queue<Disposer>();

		private Queue<Disposer> loaders = new Queue<Disposer>();
		private Queue<Disposer> loaders2 = new Queue<Disposer>();

		private Queue<Disposer> lateUpdates = new Queue<Disposer>();
		private Queue<Disposer> lateUpdates2 = new Queue<Disposer>();

		public static void Close()
		{
			instance = null;
		}

		public void LoadHotfixDll()
		{
#if ILRuntime
			DllHelper.LoadHotfixAssembly();	
#else
			EventSystem.Instance.HotfixAssembly = DllHelper.LoadHotfixAssembly();
#endif
			this.Register();
			this.Load();
		}

		public void Add(DLLType dllType, Assembly assembly)
		{
			this.assemblies[dllType] = assembly;
			this.Register();
		}

		private void Register()
		{
			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ObjectEventAttribute), false);

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


			this.allEvents.Clear();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					if (!this.allEvents.ContainsKey((EventIdType)aEventAttribute.Type))
					{
						this.allEvents.Add((EventIdType)aEventAttribute.Type, new List<IEventMethod>());
					}
					this.allEvents[(EventIdType)aEventAttribute.Type].Add(new IEventMonoMethod(obj));
				}
			}

			// hotfix dll
			Type[] hotfixTypes = DllHelper.GetHotfixTypes();
			foreach (Type type in hotfixTypes)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
#if ILRuntime
					IEventMethod method = new IEventILMethod(type, "Run");
#else
					object obj = Activator.CreateInstance(type);
					IEventMethod method = new IEventMonoMethod(obj);
#endif
					if (!allEvents.ContainsKey((EventIdType)aEventAttribute.Type))
					{
						allEvents.Add((EventIdType)aEventAttribute.Type, new List<IEventMethod>());
					}
					allEvents[(EventIdType)aEventAttribute.Type].Add(method);
				}
			}
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
			IObjectSystem objectSystem;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
			{
				return;
			}

			if (objectSystem is ILoad)
			{
				this.loaders.Enqueue(disposer);
			}

			if (objectSystem is IUpdate)
			{
				this.updates.Enqueue(disposer);
			}

			if (objectSystem is IStart)
			{
				this.starts.Enqueue(disposer);
			}
		}

		public void Awake(Disposer disposer)
		{
			EventSystem.Instance.Add(disposer);

			IObjectSystem objectSystem;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
			{
				return;
			}
			IAwake iAwake = objectSystem as IAwake;
			if (iAwake == null)
			{
				return;
			}
			objectSystem.Set(disposer);
			iAwake.Awake();
		}

		public void Awake<P1>(Disposer disposer, P1 p1)
		{
			EventSystem.Instance.Add(disposer);

			IObjectSystem objectSystem;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
			{
				return;
			}
			IAwake<P1> iAwake = objectSystem as IAwake<P1>;
			if (iAwake == null)
			{
				return;
			}
			objectSystem.Set(disposer);
			iAwake.Awake(p1);
		}

		public void Awake<P1, P2>(Disposer disposer, P1 p1, P2 p2)
		{
			EventSystem.Instance.Add(disposer);

			IObjectSystem objectSystem;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
			{
				return;
			}
			IAwake<P1, P2> iAwake = objectSystem as IAwake<P1, P2>;
			if (iAwake == null)
			{
				return;
			}
			objectSystem.Set(disposer);
			iAwake.Awake(p1, p2);
		}

		public void Awake<P1, P2, P3>(Disposer disposer, P1 p1, P2 p2, P3 p3)
		{
			EventSystem.Instance.Add(disposer);

			IObjectSystem objectSystem;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
			{
				return;
			}
			IAwake<P1, P2, P3> iAwake = objectSystem as IAwake<P1, P2, P3>;
			if (iAwake == null)
			{
				return;
			}
			objectSystem.Set(disposer);
			iAwake.Awake(p1, p2, p3);
		}

		public void Load()
		{
			while (this.loaders.Count > 0)
			{
				Disposer disposer = this.loaders.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				IObjectSystem objectSystem;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
				{
					continue;
				}

				this.loaders2.Enqueue(disposer);

				ILoad iLoad = objectSystem as ILoad;
				if (iLoad == null)
				{
					continue;
				}
				objectSystem.Set(disposer);
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
			while (this.starts.Count > 0)
			{
				Disposer disposer = this.starts.Dequeue();

				IObjectSystem objectSystem;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
				{
					continue;
				}
				IStart iStart = objectSystem as IStart;
				if (iStart == null)
				{
					continue;
				}
				objectSystem.Set(disposer);
				iStart.Start();
			}
		}

		public void Update()
		{
			this.Start();

			while (this.updates.Count > 0)
			{
				Disposer disposer = this.updates.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				IObjectSystem objectSystem;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
				{
					continue;
				}

				this.updates2.Enqueue(disposer);

				IUpdate iUpdate = objectSystem as IUpdate;
				if (iUpdate == null)
				{
					continue;
				}
				objectSystem.Set(disposer);
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

		public void LateUpdate()
		{
			while (this.lateUpdates.Count > 0)
			{
				Disposer disposer = this.lateUpdates.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				IObjectSystem objectSystem;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out objectSystem))
				{
					continue;
				}

				this.lateUpdates2.Enqueue(disposer);

				ILateUpdate iLateUpdate = objectSystem as ILateUpdate;
				if (iLateUpdate == null)
				{
					continue;
				}
				objectSystem.Set(disposer);
				try
				{
					iLateUpdate.LateUpdate();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}

			ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
		}

		public void Run(EventIdType type)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
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
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A, B>(EventIdType type, A a, B b)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A, B, C>(EventIdType type, A a, B b, C c)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}