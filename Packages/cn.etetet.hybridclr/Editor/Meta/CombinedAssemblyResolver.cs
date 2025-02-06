using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Meta
{
    public class CombinedAssemblyResolver : AssemblyResolverBase
    {
        private readonly IAssemblyResolver[] _resolvers;

        public CombinedAssemblyResolver(params IAssemblyResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        protected override bool TryResolveAssembly(string assemblyName, out string assemblyPath)
        {
            foreach(var resolver in _resolvers)
            {
                var assembly = resolver.ResolveAssembly(assemblyName, false);
                if (assembly != null)
                {
                    assemblyPath = assembly;
                    return true;
                }
            }
            assemblyPath = null;
            return false;
        }
    }
}
