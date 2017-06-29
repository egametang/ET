using System;
using System.Collections.Generic;
using Base;

namespace Model
{
	[ObjectEvent]
	public class EventComponentEvent : ObjectEvent<EventComponent>, IAwake, ILoad
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Load()
		{
			this.Get().Load();
		}
	}

	/// <summary>
	/// 事件分发
	/// </summary>
	public class EventComponent: Component
	{
		private Dictionary<int, List<object>> allEvents;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allEvents = new Dictionary<int, List<object>>();

			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute) attr;

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
					IEvent iEvent = obj as IEvent;
					if (iEvent == null)
					{
						throw new Exception($"event type: {type} is not IEvent");
					}
					iEvent.Run();
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
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
					var iEvent = obj as IEvent<A>;
					if (iEvent == null)
					{
						throw new Exception($"event type: {type} is not IEvent<{typeof (A).Name}>");
					}
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
					var iEvent = obj as IEvent<A, B>;
					if (iEvent == null)
					{
						throw new Exception($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}>");
					}
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
					var iEvent = obj as IEvent<A, B, C>;
					if (iEvent == null)
					{
						throw new Exception($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}>");
					}
					iEvent.Run(a, b, c);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D>(int type, A a, B b, C c, D d)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					var iEvent = obj as IEvent<A, B, C, D>;
					if (iEvent == null)
					{
						throw new Exception($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}>");
					}
					iEvent.Run(a, b, c, d);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D, E>(int type, A a, B b, C c, D d, E e)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					var iEvent = obj as IEvent<A, B, C, D, E>;
					if (iEvent == null)
					{
						throw new Exception(
								$"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}, {typeof (E).Name}>");
					}
					iEvent.Run(a, b, c, d, e);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D, E, F>(int type, A a, B b, C c, D d, E e, F f)
		{
			if (!this.allEvents.TryGetValue(type, out List<object> iEvents))
			{
				return;
			}

			foreach (object obj in iEvents)
			{
				try
				{
					var iEvent = obj as IEvent<A, B, C, D, E, F>;
					if (iEvent == null)
					{
						throw new Exception(
								$"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}, {typeof (E).Name}>");
					}
					iEvent.Run(a, b, c, d, e, f);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}
	}
}