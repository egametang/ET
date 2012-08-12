using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.BehaviorTree
{
	[ModuleExport(typeof (BehaviorTreeModule))]
	public class BehaviorTreeModule : IModule
	{
		#region IModule Members

		public void Initialize()
		{
		}

		#endregion
	}
}