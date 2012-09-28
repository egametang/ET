using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.WaiGua
{
	[ModuleExport(moduleType: typeof(WaiGuaModule))]
	public class WaiGuaModule : IModule
	{
		public void Initialize()
		{
		}
	}
}