using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[ObjectEvent((int)EntityEventId.EventComponent)]
	public class EventComponent : Component, IAwake
	{
		private Dictionary<EventIdType, List<object>> allEvents;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allEvents = new Dictionary<EventIdType, List<object>>();
			
			Type[] types = DllHelper.GetHotfixTypes();
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
					Log.Error(e.ToStr());
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