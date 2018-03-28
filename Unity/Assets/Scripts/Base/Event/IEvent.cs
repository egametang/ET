using System;

namespace ETModel
{
	public interface IEvent
	{
		void Handle();
		void Handle(object a);
		void Handle(object a, object b);
		void Handle(object a, object b, object c);
	}

	public abstract class AEvent : IEvent
	{
		public void Handle()
		{
			this.Run();
		}

		public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

		public abstract void Run();
	}

	public abstract class AEvent<A>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

		public void Handle(object a)
		{
			this.Run((A)a);
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

		public abstract void Run(A a);
	}

	public abstract class AEvent<A, B>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

		public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			this.Run((A)a, (B)b);
		}

		public void Handle(object a, object b, object c)
		{
			throw new NotImplementedException();
		}

		public abstract void Run(A a, B b);
	}

	public abstract class AEvent<A, B, C>: IEvent
	{
		public void Handle()
		{
			throw new NotImplementedException();
		}

		public void Handle(object a)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b)
		{
			throw new NotImplementedException();
		}

		public void Handle(object a, object b, object c)
		{
			this.Run((A)a, (B)b, (C)c);
		}

		public abstract void Run(A a, B b, C c);
	}
}