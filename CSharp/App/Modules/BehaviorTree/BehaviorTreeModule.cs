using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.BehaviorTree
{
	[ModuleExport(moduleType: typeof (BehaviorTreeModule))]
	public class BehaviorTreeModule: IModule
	{
		public void Initialize()
		{
		}
	}
}