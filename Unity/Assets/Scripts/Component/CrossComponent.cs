using System;
using System.Collections.Generic;

namespace Model
{
	/// <summary>
	/// 事件分发,可以将事件分发到IL层
	/// </summary>
	[EntityEvent(EntityEventId.CrossComponent)]
	public class CrossComponent : Component, IAwake
	{
		private static object[] args0 = new object[0];
		private static object[] args1 = new object[1];
		private static object[] args2 = new object[2];
		private static object[] args3 = new object[3];
		private static object[] args4 = new object[4];

		private Dictionary<int, List<IInstanceMethod>> allEvents;

		public void Awake()
		{
			this.Load();
		}

		private void Load()
		{
			this.allEvents = new Dictionary<int, List<IInstanceMethod>>();

			Type[] types = DllHelper.GetHotfixTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					IInstanceMethod method = new ILInstanceMethod(type, "Run");
					if (!this.allEvents.ContainsKey(aEventAttribute.Type))
					{
						this.allEvents.Add(aEventAttribute.Type, new List<IInstanceMethod>());
					}
					this.allEvents[aEventAttribute.Type].Add(method);
				}
			}
		}

		public void Run(int type)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					obj.Run(args0);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A>(int type, A a)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					args1[0] = a;
					obj.Run(args1);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B>(int type, A a, B b)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					args2[0] = a;
					args2[1] = b;
					obj.Run(args2);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C>(int type, A a, B b, C c)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					args3[0] = a;
					args3[1] = b;
					args3[2] = c;
					obj.Run(args3);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run<A, B, C, D>(int type, A a, B b, C c, D d)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					args4[0] = a;
					args4[1] = b;
					args4[2] = c;
					args4[3] = d;
					obj.Run(args4);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}

		public void Run(int type, params object[] param)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					obj.Run(param);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}
	}
}