namespace Base
{
	/// <summary>
	/// World的Componet实现该接口,World.Load会调用Load方法
	/// </summary>
	public interface ILoader
	{
		void Load();
	}
}