namespace Base
{
	/// <summary>
	/// World的Componet实现该接口后,会在World.Start时调用该Start方法
	/// </summary>
	public interface IAwake
	{
		void Awake();
	}

	public interface IAwake<in P1>
	{
		void Awake(P1 p1);
	}

	public interface IAwake<in P1, in P2>
	{
		void Awake(P1 p1, P2 p2);
	}

	public interface IAwake<in P1, in P2, in P3>
	{
		void Awake(P1 p1, P2 p2, P3 p3);
	}
}