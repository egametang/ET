using System.Threading.Tasks;
#pragma warning disable 1998

namespace Model
{
	public abstract class AEvent
	{
		public virtual void Run()
		{
		}

		public async virtual Task RunAsync()
		{
		}
	}

	public abstract class AEvent<A>
	{
		public virtual void Run(A a)
		{
		}

		public async virtual Task RunAsync(A a)
		{	
		}
	}

	public abstract class AEvent<A, B>
	{
		public virtual void Run(A a, B b)
		{
		}

		public async virtual Task RunAsync(A a, B b)
		{
		}
	}

	public abstract class AEvent<A, B, C>
	{
		public virtual void Run(A a, B b, C c)
		{
		}

		public async virtual Task RunAsync(A a, B b, C c)
		{
		}
	}

	public abstract class AEvent<A, B, C, D>
	{
		public virtual void Run(A a, B b, C c, D d)
		{
		}

		public async virtual Task RunAsync(A a, B b, C c, D d)
		{
		}
	}

	public abstract class AEvent<A, B, C, D, E>
	{
		public virtual void Run(A a, B b, C c, D d, E e)
		{
		}

		public async virtual Task RunAsync(A a, B b, C c, D d, E e)
		{
		}
	}
}