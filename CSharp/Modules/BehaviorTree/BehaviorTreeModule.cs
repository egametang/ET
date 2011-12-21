using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace BehaviorTree
{
	[ModuleExport(typeof(BehaviorTreeModule))]
	public class BehaviorTreeModule : IModule
	{
		public void Initialize()
		{
		}
	}
}
