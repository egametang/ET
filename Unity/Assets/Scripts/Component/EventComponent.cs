using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace Model
{
	public interface IEventMethod
	{
		void Run();
		void Run<A>(A a);
		void Run<A, B>(A a, B b);
		void Run<A, B, C>(A a, B b, C c);
		void Run<A, B, C, D>(A a, B b, C c, D d);
	}

	public class IEventMonoMethod : IEventMethod
	{
		private readonly object obj;

		public IEventMonoMethod(object obj)
		{
			this.obj = obj;
		}

		public void Run()
		{
			((IEvent)obj).Run();
		}

		public void Run<A>(A a)
		{
			((IEvent<A>)obj).Run(a);
		}

		public void Run<A, B>(A a, B b)
		{
			((IEvent<A, B>)obj).Run(a, b);
		}

		public void Run<A, B, C>(A a, B b, C c)
		{
			((IEvent<A, B, C>)obj).Run(a, b, c);
		}

		public void Run<A, B, C, D>(A a, B b, C c, D d)
		{
			((IEvent<A, B, C, D>)obj).Run(a, b, c, d);
		}
	}

	public class IEventILMethod : IEventMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly ILTypeInstance instance;
		private readonly IMethod method;
		private readonly object[] param;

		public IEventILMethod(Type type, string methodName)
		{
			appDomain = Init.Instance.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod(methodName);
			int n = this.method.ParameterCount;
			this.param = new object[n];
		}

		public void Run()
		{
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A>(A a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B>(A a, B b)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B, C>(A a, B b, C c)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B, C, D>(A a, B b, C c, D d)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.param[3] = d;
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}

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

	public class EventComponent : Component
	{
		public static EventComponent Instance;

		private Dictionary<EventIdType, List<IEventMethod>> allEvents;

		public void Awake()
		{
			Instance = this;
			this.Load();
		}

		public void Load()
		{
			this.allEvents = new Dictionary<EventIdType, List<IEventMethod>>();

			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					if (!this.allEvents.ContainsKey((EventIdType)aEventAttribute.Type))
					{
						this.allEvents.Add((EventIdType)aEventAttribute.Type, new List<IEventMethod>());
					}
					this.allEvents[(EventIdType)aEventAttribute.Type].Add(new IEventMonoMethod(obj));
				}
			}

			// hotfix dll
			Type[] hotfixTypes = DllHelper.GetHotfixTypes();
			foreach (Type type in hotfixTypes)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);
				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
#if ILRuntime
					IEventMethod method = new IEventILMethod(type, "Run");
#else
					object obj = Activator.CreateInstance(type);
					IEventMethod method = new IEventMonoMethod(obj);
#endif
					if (!allEvents.ContainsKey((EventIdType)aEventAttribute.Type))
					{
						allEvents.Add((EventIdType)aEventAttribute.Type, new List<IEventMethod>());
					}
					allEvents[(EventIdType)aEventAttribute.Type].Add(method);
				}
			}
		}

		public void Run(EventIdType type)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A>(EventIdType type, A a)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A, B>(EventIdType type, A a, B b)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run<A, B, C>(EventIdType type, A a, B b, C c)
		{
			List<IEventMethod> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEventMethod iEvent in iEvents)
			{
				try
				{
					iEvent.Run(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}