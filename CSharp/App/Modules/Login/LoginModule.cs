using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Login
{
	[ModuleExport(moduleType: typeof (LoginModule))]
	public class LoginModule: IModule
	{
		public void Initialize()
		{
		}
	}
}