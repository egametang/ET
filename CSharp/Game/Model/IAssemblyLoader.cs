using System.Reflection;

namespace Model
{
	/// <summary>
	/// World的Componet实现该接口,World.Load会调用Load方法
	/// </summary>
	public interface IAssemblyLoader
	{
		void Load(Assembly assembly);
	}
}