using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.Tree
{
	[ModuleExport(moduleType: typeof (BehaviorTreeModule))]
	public class BehaviorTreeModule: IModule
	{
		public void Initialize()
		{
		}
	}
}