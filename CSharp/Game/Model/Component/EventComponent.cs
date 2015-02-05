using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Base;
using Common.Event;

namespace Model
{
	public class EventComponent<AttributeType>: Component<World>, IAssemblyLoader
			where AttributeType : AEventAttribute
	{
		private Dictionary<int, List<IEvent>> events;

		public void Load(Assembly assembly)
		{
			this.events = new Dictionary<int, List<IEvent>>();

			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(typeof (AttributeType), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(t);
				IEvent iEvent = obj as IEvent;
				if (iEvent == null)
				{
					throw new Exception(string.Format("event not inherit IEvent interface: {0}",
					                                  obj.GetType().FullName));
				}

				AEventAttribute iEventAttribute = (AEventAttribute) attrs[0];

				if (!this.events.ContainsKey(iEventAttribute.Type))
				{
					this.events.Add(iEventAttribute.Type, new List<IEvent>());
				}
				this.events[iEventAttribute.Type].Add(iEvent);
			}
		}

		public void Run(int type, Env env)
		{
			List<IEvent> iEventDict = null;
			if (!this.events.TryGetValue(type, out iEventDict))
			{
				return;
			}

			foreach (var iEvent in iEventDict)
			{
				iEvent.Run(env);
			}
		}
	}
}