using System.Reflection;

namespace Model
{
	interface IConfigLoader
	{
		void Load(Assembly assembly);
	}
}
