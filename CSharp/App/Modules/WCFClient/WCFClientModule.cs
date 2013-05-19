using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.WCFClient
{
	[ModuleExport(moduleType: typeof(WCFClientModule))]
	public class WCFClientModule : IModule
	{
		public void Initialize()
		{
		}
	}
}
