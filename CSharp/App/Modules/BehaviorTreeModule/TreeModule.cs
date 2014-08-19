using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;

namespace Modules.BehaviorTreeModule
{
    [ModuleExport(moduleType: typeof (TreeModule))]
    public class TreeModule: IModule
    {
        public void Initialize()
        {
        }
    }
}