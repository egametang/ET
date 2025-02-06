using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Meta
{
    public class AssemblyReferenceDeepCollector : AssemblyCacheBase
    {
        private readonly List<string> _rootAssemblies;

        public IReadOnlyList<string> GetRootAssemblyNames()
        {
            return _rootAssemblies;
        }

        public List<ModuleDefMD> GetLoadedModulesExcludeRootAssemblies()
        {
            return LoadedModules.Where(e => !_rootAssemblies.Contains(e.Key)).Select(e => e.Value).ToList();
        }

        public List<ModuleDefMD> GetLoadedModules()
        {
            return LoadedModules.Select(e => e.Value).ToList();
        }

        public List<ModuleDefMD> GetLoadedModulesOfRootAssemblies()
        {
            return _rootAssemblies.Select(ass => LoadedModules[ass]).ToList();
        }

        public AssemblyReferenceDeepCollector(IAssemblyResolver assemblyResolver, List<string> rootAssemblies) : base(assemblyResolver)
        {
            _rootAssemblies = rootAssemblies;
            LoadAllAssembiles();
        }

        private void LoadAllAssembiles()
        {
            foreach (var asm in _rootAssemblies)
            {
                LoadModule(asm);
            }
        }
    }
}
