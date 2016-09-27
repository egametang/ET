using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base
{
	[ObjectEvent]
	public class EventComponentEvent : ObjectEvent<EventComponent>, ILoader, IAwake
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake()
		{
			this.GetValue().Load();
		}
	}

	/// <summary>
	/// 事件分发
	/// </summary>
	public class EventComponent: Component<Scene>
	{
		private Dictionary<EventIdType, List<object>> allEvents;

		public void Load()
		{
			this.allEvents = new Dictionary<EventIdType, List<object>>();
			Assembly[] assemblies = Object.ObjectManager.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
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
		}

		public void Run(EventIdType type)
		{
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException($"event type: {type} is not IEvent");
					}
					iEvent.Run();
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A>(EventIdType type, A a)
		{
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}>");
					}
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
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}>");
					}
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
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}>");
					}
					iEvent.Run(a, b, c);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D>(EventIdType type, A a, B b, C c, D d)
		{
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}>");
					}
					iEvent.Run(a, b, c, d);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D, E>(EventIdType type, A a, B b, C c, D d, E e)
		{
			List<object> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
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
						throw new GameException(
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
        public void Run<A, B, C, D, E,F>(EventIdType type, A a, B b, C c, D d, E e,F f)
        {
            List<object> iEvents = null;
            if (!this.allEvents.TryGetValue(type, out iEvents))
            {
                return;
            }

            foreach (object obj in iEvents)
            {
                try
                {
                    var iEvent = obj as IEvent<A, B, C, D, E,F>;
                    if (iEvent == null)
                    {
                        throw new GameException(
                                $"event type: {type} is not IEvent<{typeof(A).Name}, {typeof(B).Name}, {typeof(C).Name}, {typeof(D).Name}, {typeof(E).Name}>");
                    }
                    iEvent.Run(a, b, c, d, e,f);
                }
                catch (Exception err)
                {
                    Log.Error(err.ToString());
                }
            }
        }
    }
}