using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Base;

namespace Model
{
	public interface IDisposerEvent
	{
		Type ValueType();
		void SetValue(object value);
	}

	public abstract class DisposerEvent<T> : IDisposerEvent
	{
		private T value;

		protected T GetValue()
		{
			return value;
		}

		public void SetValue(object v)
		{
			this.value = (T)v;
		}

		public Type ValueType()
		{
			return typeof(T);
		}
	}

	public sealed class DisposerManager : IDisposable
	{
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private Dictionary<Type, IDisposerEvent> disposerEvents;

		private readonly HashSet<Disposer> disposers = new HashSet<Disposer>();
		private readonly HashSet<Disposer> updates = new HashSet<Disposer>();
		private readonly HashSet<Disposer> loaders = new HashSet<Disposer>();

		private static DisposerManager instance = new DisposerManager();

		public static DisposerManager Instance
		{
			get
			{
				return instance;
			}
		}

		private DisposerManager()
		{
		}

		public static void Reset()
		{
			instance.Dispose();
			instance = new DisposerManager();
		}

		public void Dispose()
		{
			foreach (Disposer o in this.disposers.ToArray())
			{
				o.Dispose();
			}
		}

		public void Register(string name, Assembly assembly)
		{
			this.assemblies[name] = assembly;

			this.disposerEvents = new Dictionary<Type, IDisposerEvent>();
			foreach (Assembly ass in this.assemblies.Values)
			{
				Type[] types = ass.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(DisposerEventAttribute), false);

					if (attrs.Length == 0)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);
					IDisposerEvent disposerEvent = obj as IDisposerEvent;
					if (disposerEvent == null)
					{
						Log.Error($"组件事件没有继承IComponentEvent: {type.Name}");
					}
					this.disposerEvents[disposerEvent.ValueType()] = disposerEvent;
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
				IDisposerEvent disposerEvent;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
				{
					continue;
				}
				ILoader iLoader = disposerEvent as ILoader;
				if (iLoader == null)
				{
					continue;
				}
				disposerEvent.SetValue(disposer);
				iLoader.Load();
			}
		}

		public void Add(Disposer disposer)
		{
			if (this.disposerEvents == null)
			{
				return;
			}

			this.disposers.Add(disposer);
			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}

			IUpdate iUpdate = disposerEvent as IUpdate;
			if (iUpdate != null)
			{
				this.updates.Add(disposer);
			}

			ILoader iLoader = disposerEvent as ILoader;
			if (iLoader != null)
			{
				this.loaders.Add(disposer);
			}
		}

		public void Remove(Disposer disposer)
		{
			this.disposers.Remove(disposer);

			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}

			IUpdate iUpdate = disposerEvent as IUpdate;
			if (iUpdate != null)
			{
				this.updates.Remove(disposer);
			}

			ILoader iLoader = disposerEvent as ILoader;
			if (iLoader != null)
			{
				this.loaders.Remove(disposer);
			}
		}

		public void Awake(Disposer disposer)
		{
			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}
			IAwake iAwake = disposerEvent as IAwake;
			if (iAwake == null)
			{
				return;
			}
			disposerEvent.SetValue(disposer);
			iAwake.Awake();
		}

		public void Awake<P1>(Disposer disposer, P1 p1)
		{
			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}
			IAwake<P1> iAwake = disposerEvent as IAwake<P1>;
			if (iAwake == null)
			{
				return;
			}
			disposerEvent.SetValue(disposer);
			iAwake.Awake(p1);
		}

		public void Awake<P1, P2>(Disposer disposer, P1 p1, P2 p2)
		{
			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}
			IAwake<P1, P2> iAwake = disposerEvent as IAwake<P1, P2>;
			if (iAwake == null)
			{
				return;
			}
			disposerEvent.SetValue(disposer);
			iAwake.Awake(p1, p2);
		}

		public void Awake<P1, P2, P3>(Disposer disposer, P1 p1, P2 p2, P3 p3)
		{
			IDisposerEvent disposerEvent;
			if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
			{
				return;
			}
			IAwake<P1, P2, P3> iAwake = disposerEvent as IAwake<P1, P2, P3>;
			if (iAwake == null)
			{
				return;
			}
			disposerEvent.SetValue(disposer);
			iAwake.Awake(p1, p2, p3);
		}
		
		public void Update()
		{
			foreach (Disposer disposer in updates)
			{
				IDisposerEvent disposerEvent;
				if (!this.disposerEvents.TryGetValue(disposer.GetType(), out disposerEvent))
				{
					continue;
				}
				IUpdate iUpdate = disposerEvent as IUpdate;
				if (iUpdate == null)
				{
					continue;
				}
				disposerEvent.SetValue(disposer);
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

		public override string ToString()
		{
			var info = new Dictionary<string, int>();
			foreach (Disposer obj in this.disposers)
			{
				if (info.ContainsKey(obj.GetType().Name))
				{
					info[obj.GetType().Name] += 1;
				}
				else
				{
					info[obj.GetType().Name] = 1;
				}
			}
			info = info.OrderByDescending(s => s.Value).ToDictionary(p => p.Key, p => p.Value);
			StringBuilder sb = new StringBuilder();
			sb.Append("\r\n");
			foreach (string key in info.Keys)
			{
				sb.Append($"{info[key],10} {key}\r\n");
			}

			sb.Append($"\r\n update: {this.updates.Count} total: {this.disposers.Count}");
			return sb.ToString();
		}
	}
}