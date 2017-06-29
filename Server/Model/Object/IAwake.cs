namespace Model
{
	public interface IAwake
	{
		void Awake();
	}

	public interface IAwake<A>
	{
		void Awake(A a);
	}

	public interface IAwake<A, B>
	{
		void Awake(A a, B b);
	}

	public interface IAwake<A, B, C>
	{
		void Awake(A a, B b, C c);
	}
}
