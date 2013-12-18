using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Tree
{
	[ModuleExport(moduleType: typeof (BehaviorTreeModule))]
	public class BehaviorTreeModule: IModule
	{
		public void Initialize()
		{
		}
	}
}