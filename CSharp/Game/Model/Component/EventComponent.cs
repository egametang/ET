using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.Base;
using Common.Event;

namespace Model
{
	public class EventComponent<AttributeType>: Component<World>, IAssemblyLoader
			where AttributeType : AEventAttribute
	{
		private Dictionary<int, List<IEventSync>> eventSyncs;

		private Dictionary<int, List<IEventAsync>> eventAsyncs;

		public void Load(Assembly assembly)
		{
			this.eventSyncs = new Dictionary<int, List<IEventSync>>();
			this.eventAsyncs = new Dictionary<int, List<IEventAsync>>();

			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(typeof (AttributeType), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(t);
				IEventSync iEventSync = obj as IEventSync;
				if (iEventSync != null)
				{
					AEventAttribute iEventAttribute = (AEventAttribute)attrs[0];

					if (!this.eventSyncs.ContainsKey(iEventAttribute.Type))
					{
						this.eventSyncs.Add(iEventAttribute.Type, new List<IEventSync>());
					}
					this.eventSyncs[iEventAttribute.Type].Add(iEventSync);
					continue;
				}

				IEventAsync iEventAsync = obj as IEventAsync;
				// ReSharper disable once InvertIf
				if (iEventAsync != null)
				{
					AEventAttribute iEventAttribute = (AEventAttribute)attrs[0];

					if (!this.eventAsyncs.ContainsKey(iEventAttribute.Type))
					{
						this.eventAsyncs.Add(iEventAttribute.Type, new List<IEventAsync>());
					}
					this.eventAsyncs[iEventAttribute.Type].Add(iEventAsync);
					continue;
				}

				throw new Exception(
					string.Format("event not inherit IEventSync or IEventAsync interface: {0}",
						obj.GetType().FullName));
			}
		}

		public void Run(int type, Env env)
		{
			List<IEventSync> iEventSyncs = null;
			if (!this.eventSyncs.TryGetValue(type, out iEventSyncs))
			{
				throw new Exception(
					string.Format("no event handler, AttributeType: {0} type: {1}",
						typeof (AttributeType).Name, type));
			}

			foreach (IEventSync iEventSync in iEventSyncs)
			{
				iEventSync.Run(env);
			}
		}

		public async Task RunAsync(int type, Env env)
		{
			List<IEventSync> iEventSyncs = null;
			this.eventSyncs.TryGetValue(type, out iEventSyncs);

			List<IEventAsync> iEventAsyncs = null;
			this.eventAsyncs.TryGetValue(type, out iEventAsyncs);

			if (iEventSyncs == null && iEventAsyncs == null)
			{
				throw new Exception(string.Format("no event handler, AttributeType: {0} type: {1}",
					typeof(AttributeType).Name, type));
			}

			if (iEventSyncs != null)
			{
				foreach (IEventSync iEventSync in iEventSyncs)
				{
					iEventSync.Run(env);
				}
			}

			if (iEventAsyncs != null)
			{
				foreach (IEventAsync iEventAsync in iEventAsyncs)
				{
					await iEventAsync.RunAsync(env);
				}
			}
		}
	}
}