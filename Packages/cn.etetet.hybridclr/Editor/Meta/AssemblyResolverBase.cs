using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Meta
{
    public abstract class AssemblyResolverBase : IAssemblyResolver
    {
        public string ResolveAssembly(string assemblyName, bool throwExIfNotFind)
        {
            if (TryResolveAssembly(assemblyName, out string assemblyPath))
            {
                return assemblyPath;
            }
            if (throwExIfNotFind)
            {
                if (SettingsUtil.HotUpdateAssemblyNamesIncludePreserved.Contains(assemblyName))
                {
                    throw new Exception($"resolve Hot update dll:{assemblyName} failed! Please make sure that this hot update dll exists or the search path is configured in the external hot update path.");
                }
                else
                {
                    throw new Exception($"resolve AOT dll:{assemblyName} failed! Please make sure that the AOT project has referenced the dll and generated the trimmed AOT dll correctly.");
                }
            }
            return null;
        }

        protected abstract bool TryResolveAssembly(string assemblyName, out string assemblyPath);
    }
}
