using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace BehaviorTree
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