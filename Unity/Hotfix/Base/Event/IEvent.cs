namespace Hotfix
{
#if ILRuntime
	public interface IEvent
	{
		void Run();
	}

	public interface IEvent<in A>
	{
		void Run(A uid);
	}

	public interface IEvent<in A, in B>
	{
		void Run(A a, B b);
	}

	public interface IEvent<in A, in B, in C>
	{
		void Run(A a, B b, C c);
	}

	public interface IEvent<in A, in B, in C, in D>
	{
		void Run(A a, B b, C c, D d);
	}

	public interface IEvent<in A, in B, in C, in D, in E>
	{
		void Run(A a, B b, C c, D d, E e);
	}

	public interface IEvent<in A, in B, in C, in D, in E, in F>
	{
		void Run(A a, B b, C c, D d, E e, F f);
	}
#else
	public interface IEvent : Model.IEvent
	{
	}

	public interface IEvent<in A> : Model.IEvent<A>
	{
	}

	public interface IEvent<in A, in B> : Model.IEvent<A, B>
	{
	}

	public interface IEvent<in A, in B, in C> : Model.IEvent<A, B, C>
	{
	}

	public interface IEvent<in A, in B, in C, in D> : Model.IEvent<A, B, C, D>
	{
	}

	public interface IEvent<in A, in B, in C, in D, in E> : Model.IEvent<A, B, C, D, E>
	{
	}

	public interface IEvent<in A, in B, in C, in D, in E, in F> : Model.IEvent<A, B, C, D, E, F>
	{
	}
#endif
}