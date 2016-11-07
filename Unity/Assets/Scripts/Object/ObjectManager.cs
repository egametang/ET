using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Base;

namespace Model
{
	public interface IObjectEvent
	{
		Type ValueType();
		void SetValue(object value);
	}

	public abstract class ObjectEvent<T> : IObjectEvent
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

	public sealed class ObjectManager : IDisposable
	{
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private Dictionary<Type, IObjectEvent> objectEvents;

		private readonly HashSet<Object> objects = new HashSet<Object>();
		private readonly HashSet<Object> updates = new HashSet<Object>();
		private readonly HashSet<Object> loaders = new HashSet<Object>();

		private static ObjectManager instance = new ObjectManager();

		public static ObjectManager Instance
		{
			get
			{
				return instance;
			}
		}

		private ObjectManager()
		{
		}

		public static void Reset()
		{
			instance.Dispose();
			instance = new ObjectManager();
		}

		public void Dispose()
		{
			foreach (Object o in this.objects.ToArray())
			{
				o.Dispose();
			}
		}

		public void Register(string name, Assembly assembly)
		{
			this.assemblies[name] = assembly;

			objectEvents = new Dictionary<Type, IObjectEvent>();
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
						Log.Error($"组件事件没有继承IComponentEvent: {type.Name}");
					}
					objectEvents[objectEvent.ValueType()] = objectEvent;
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
			foreach (Object obj in this.loaders)
			{
				IObjectEvent objectEvent;
				if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
				{
					continue;
				}
				ILoader iLoader = objectEvent as ILoader;
				if (iLoader == null)
				{
					continue;
				}
				objectEvent.SetValue(obj);
				iLoader.Load();
			}
		}

		public void Add(Object obj)
		{
			if (objectEvents == null)
			{
				return;
			}

			this.objects.Add(obj);
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}

			IUpdate iUpdate = objectEvent as IUpdate;
			if (iUpdate != null)
			{
				this.updates.Add(obj);
			}

			ILoader iLoader = objectEvent as ILoader;
			if (iLoader != null)
			{
				this.loaders.Add(obj);
			}
		}

		public void Remove(Object obj)
		{
			this.objects.Remove(obj);

			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}

			IUpdate iUpdate = objectEvent as IUpdate;
			if (iUpdate != null)
			{
				this.updates.Remove(obj);
			}

			ILoader iLoader = objectEvent as ILoader;
			if (iLoader != null)
			{
				this.loaders.Remove(obj);
			}
		}

		public void Awake(Object obj)
		{
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}
			IAwake iAwake = objectEvent as IAwake;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.SetValue(obj);
			iAwake.Awake();
		}

		public void Awake<P1>(Object obj, P1 p1)
		{
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}
			IAwake<P1> iAwake = objectEvent as IAwake<P1>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.SetValue(obj);
			iAwake.Awake(p1);
		}

		public void Awake<P1, P2>(Object obj, P1 p1, P2 p2)
		{
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}
			IAwake<P1, P2> iAwake = objectEvent as IAwake<P1, P2>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.SetValue(obj);
			iAwake.Awake(p1, p2);
		}

		public void Awake<P1, P2, P3>(Object obj, P1 p1, P2 p2, P3 p3)
		{
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}
			IAwake<P1, P2, P3> iAwake = objectEvent as IAwake<P1, P2, P3>;
			if (iAwake == null)
			{
				return;
			}
			objectEvent.SetValue(obj);
			iAwake.Awake(p1, p2, p3);
		}
		
		public void Update()
		{
			foreach (Object obj in updates)
			{
				IObjectEvent objectEvent;
				if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
				{
					continue;
				}
				IUpdate iUpdate = objectEvent as IUpdate;
				if (iUpdate == null)
				{
					continue;
				}
				objectEvent.SetValue(obj);
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
			foreach (Object obj in objects)
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

			sb.Append($"\r\n update: {this.updates.Count} total: {this.objects.Count}");
			return sb.ToString();
		}
	}
}