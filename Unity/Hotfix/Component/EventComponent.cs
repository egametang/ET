using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[ObjectEvent(EntityEventId.EventComponent)]
	public class EventComponent : Component, IAwake
	{
		private Dictionary<int, List<object>> allEvents;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allEvents = new Dictionary<int, List<object>>();

			Type[] types = DllHelper.GetHotfixTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					if (!this.allEvents.ContainsKey(aEventAttribute.Type))
					{
						this.allEvents.Add(aEventAttribute.Type, new List<object>());
					}
					this.allEvents[aEventAttribute.Type].Add(obj);
				}
			}
		}

		public void Run(int type)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
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

		public void Run<A>(int type, A a)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
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

		public void Run<A, B>(int type, A a, B b)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
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

		public void Run<A, B, C>(int type, A a, B b, C c)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
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