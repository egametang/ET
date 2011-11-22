using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Module.Login
{
	[ModuleExport(typeof(LoginModule))]
	public class LoginModule : IModule
	{
		public void Initialize()
		{
		}
	}
}
