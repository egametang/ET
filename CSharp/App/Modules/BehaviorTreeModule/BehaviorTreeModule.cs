using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.BehaviorTreeModule
{
	[ModuleExport(typeof (BehaviorTreeModule))]
	public class BehaviorTreeModule: IModule
	{
		public void Initialize()
		{
		}
	}
}