namespace Hotfix
{
	public interface IEvent
	{
		void Run();
	}

	public interface IEvent<in A>
	{
		void Run(A a);
	}

	public interface IEvent<in A, in B>
	{
		void Run(A a, B b);
	}

	public interface IEvent<in A, in B, in C>
	{
		void Run(A a, B b, C c);
	}
}
