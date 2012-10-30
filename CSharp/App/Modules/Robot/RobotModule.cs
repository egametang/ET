using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.Robot
{
	[ModuleExport(moduleType: typeof (RobotModule))]
	public class RobotModule: IModule
	{
		public void Initialize()
		{
		}
	}
}