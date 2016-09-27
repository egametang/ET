namespace Base
{
	/// <summary>
	/// World的Componet实现该接口后,会在World.Start时调用该Start方法
	/// </summary>
	public interface IStart
	{
		void Start();
	}
}