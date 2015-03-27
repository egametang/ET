using System.Reflection;

namespace Model
{
	internal interface IConfigLoader
	{
		void Load(Assembly assembly);
	}
}