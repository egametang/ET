using System;
using System.Collections.Generic;

namespace ETModel
{
	public class EventProxy: IEvent
	{
		public Action<List<object>> action;
		public List<object> param = new List<object>();

		public EventProxy(Action<List<object>> action)
		{
			this.action = action;
		}

		public void Handle()
		{
			this.param.Clear();
			this.action.Invoke(this.param);
		}

		public void Handle(object a)
		{
			this.param.Clear();
			this.param.Add(a);
			this.action.Invoke(this.param);
		}

		public void Handle(object a, object b)
		{
			this.param.Clear();
			this.param.Add(a);
			this.param.Add(b);
			this.action.Invoke(this.param);
		}

		public void Handle(object a, object b, object c)
		{
			this.param.Clear();
			this.param.Add(a);
			this.param.Add(b);
			this.param.Add(c);
			this.action.Invoke(this.param);
		}
	}
}
