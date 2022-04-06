using System;

namespace ET
{
	public interface IEvent
	{
		Type GetEventType();
	}

	public interface IEventClass: IEvent
	{
		void Handle(object a);
	}

	[Event]
	public abstract class AEventClass<A>: IEventClass where A: class
	{
		public Type GetEventType()
		{
			return typeof (A);
		}

		protected abstract void Run(object a);

		public void Handle(object a)
		{
			try
			{
				Run(a);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
	
	[Event]
	public abstract class AEvent<A>: IEvent where A: struct
	{
		public Type GetEventType()
		{
			return typeof (A);
		}

		protected abstract void Run(A a);

		public void Handle(A a)
		{
			try
			{
				Run(a);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
	
	[Event]
	public abstract class AEventAsync<A>: IEvent where A: struct
	{
		public Type GetEventType()
		{
			return typeof (A);
		}

		protected abstract ETTask Run(A a);

		public async ETTask Handle(A a)
		{
			try
			{
				await Run(a);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}