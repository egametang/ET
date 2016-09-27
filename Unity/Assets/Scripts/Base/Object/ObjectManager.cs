using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Base;

namespace Base
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

	public class ObjectManager : IDisposable
	{
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private Dictionary<Type, IObjectEvent> objectEvents;

		private readonly Dictionary<long, Object> objects = new Dictionary<long, Object>();

		private List<long> starts = new List<long>();
		private List<long> newStarts = new List<long>();

		private List<long> updates = new List<long>(3000);
		private List<long> newUpdates = new List<long>(3000);

		private readonly List<long> loaders = new List<long>();

		public void Dispose()
		{
			foreach (Object o in this.objects.Values.ToArray())
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
			foreach (long id in this.loaders)
			{
				Object obj;
				if (!this.objects.TryGetValue(id, out obj))
				{
					continue;
				}
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

			this.objects.Add(obj.Id, obj);
			IObjectEvent objectEvent;
			if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
			{
				return;
			}

			IStart iStart = objectEvent as IStart;
			if (iStart != null)
			{
				this.newStarts.Add(obj.Id);
			}

			IUpdate iUpdate = objectEvent as IUpdate;
			if (iUpdate != null)
			{
				this.newUpdates.Add(obj.Id);
			}

			ILoader iLoader = objectEvent as ILoader;
			if (iLoader != null)
			{
				this.loaders.Add(obj.Id);
			}
		}

		public void Remove(long id)
		{
			this.objects.Remove(id);
		}

		public void Open(long id)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
			IObjectEvent e;
			if (!objectEvents.TryGetValue(obj.GetType(), out e))
			{
				return;
			}
			IOpen open = e as IOpen;
			if (open == null)
			{
				return;
			}
			try
			{
				e.SetValue(obj);
				open.Open();
			}
			catch (Exception exc)
			{
				Log.Error(exc.ToString());
			}
		}

		public void Close(long id)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
			IObjectEvent e;
			if (!objectEvents.TryGetValue(obj.GetType(), out e))
			{
				return;
			}
			IClose close = e as IClose;
			if (close == null)
			{
				return;
			}
			try
			{
				e.SetValue(obj);
				close.Close();
			}
			catch (Exception exc)
			{
				Log.Error(exc.ToString());
			}
		}

		public void Awake(long id)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
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

		public void Awake<P1>(long id, P1 p1)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
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

		public void Awake<P1, P2>(long id, P1 p1, P2 p2)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
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

		public void Awake<P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3)
		{
			Object obj;
			if (!objects.TryGetValue(id, out obj))
			{
				return;
			}
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

		private void Start()
		{
			starts = newStarts;
			newStarts = new List<long>();
			foreach (long id in starts)
			{
				Object obj;
				if (!this.objects.TryGetValue(id, out obj))
				{
					continue;
				}
				IObjectEvent objectEvent;
				if (!objectEvents.TryGetValue(obj.GetType(), out objectEvent))
				{
					continue;
				}
				IStart iStart = objectEvent as IStart;
				if (iStart == null)
				{
					continue;
				}
				objectEvent.SetValue(obj);
				iStart.Start();
			}
		}
		
		public void Update()
		{
			this.Start();

			// 交换update
			List<long> tmpUpdate = updates;
			updates = newUpdates;
			newUpdates = tmpUpdate;
			newUpdates.Clear();
			foreach (long id in updates)
			{
				Object obj;
				if (!objects.TryGetValue(id, out obj))
				{
					continue;
				}
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
				newUpdates.Add(id);
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
			foreach (Object obj in objects.Values)
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

			sb.Append($"\r\n start: {newStarts.Count}, update: {newUpdates.Count} total: {this.objects.Count}");
			return sb.ToString();
		}
	}
}