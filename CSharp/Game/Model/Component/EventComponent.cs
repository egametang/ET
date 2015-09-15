using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.Base;
using Common.Logger;

namespace Model
{
	public class EventComponent<AttributeType>: Component<World>, IAssemblyLoader
			where AttributeType : AEventAttribute
	{
		private Dictionary<EventType, List<object>> allEvents;

		public void Load(Assembly assembly)
		{
			this.allEvents = new Dictionary<EventType, List<object>>();

			ServerType serverType = World.Instance.Options.ServerType;

			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(typeof (AttributeType), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				AEventAttribute aEventAttribute = (AEventAttribute) attrs[0];
				if (!aEventAttribute.Contains(serverType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(t);
				if (obj == null)
				{
					throw new Exception($"event not inherit IEvent or IEventAsync interface: {obj.GetType().FullName}");
				}
				if (!this.allEvents.ContainsKey(aEventAttribute.Type))
				{
					this.allEvents.Add(aEventAttribute.Type, new List<object>());
				}
				this.allEvents[aEventAttribute.Type].Add(obj);
			}
		}

		public async Task RunAsync(EventType type)
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
					AEvent iEvent = obj as AEvent;
					if (iEvent == null)
					{
						throw new GameException($"event type: {type} is not IEvent");
					}
					iEvent.Run();
					await iEvent.RunAsync();
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}

		public async Task RunAsync<A>(EventType type, A a)
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
					AEvent<A> iEvent = obj as AEvent<A>;
					if (iEvent == null)
					{
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}>");
					}
					iEvent.Run(a);
					await iEvent.RunAsync(a);
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}

		public async Task RunAsync<A, B>(EventType type, A a, B b)
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
					AEvent<A, B> iEvent = obj as AEvent<A, B>;
					if (iEvent == null)
					{
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}>");
					}
					iEvent.Run(a, b);
					await iEvent.RunAsync(a, b);
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}

		public async Task RunAsync<A, B, C>(EventType type, A a, B b, C c)
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
					AEvent<A, B, C> iEvent = obj as AEvent<A, B, C>;
					if (iEvent == null)
					{
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}>");
					}
					iEvent.Run(a, b, c);
					await iEvent.RunAsync(a, b, c);
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}

		public async Task RunAsync<A, B, C, D>(EventType type, A a, B b, C c, D d)
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
					AEvent<A, B, C, D> iEvent = obj as AEvent<A, B, C, D>;
					if (iEvent == null)
					{
						throw new GameException($"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}>");
					}
					iEvent.Run(a, b, c, d);
					await iEvent.RunAsync(a, b, c, d);
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}

		public async Task RunAsync<A, B, C, D, E>(EventType type, A a, B b, C c, D d, E e)
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
					AEvent<A, B, C, D, E> iEvent = obj as AEvent<A, B, C, D, E>;
					if (iEvent == null)
					{
						throw new GameException(
								$"event type: {type} is not IEvent<{typeof (A).Name}, {typeof (B).Name}, {typeof (C).Name}, {typeof (D).Name}, {typeof (E).Name}>");
					}
					iEvent.Run(a, b, c, d, e);
					await iEvent.RunAsync(a, b, c, d, e);
				}
				catch (Exception err)
				{
					Log.Debug(err.ToString());
				}
			}
		}
	}
}