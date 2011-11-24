using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Module.TreeCanvas
{
	[ModuleExport(typeof(TreeCanvasModule))]
	public class TreeCanvasModule : IModule
	{
		public void Initialize()
		{
		}
	}
}
