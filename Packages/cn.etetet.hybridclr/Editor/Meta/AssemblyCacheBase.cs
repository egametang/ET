using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Meta
{
    public abstract class AssemblyCacheBase
    {
        private readonly IAssemblyResolver _assemblyPathResolver;
        private readonly ModuleContext _modCtx;
        private readonly AssemblyResolver _asmResolver;
        private bool _loadedNetstandard;


        public ModuleContext ModCtx => _modCtx;

        public Dictionary<string, ModuleDefMD> LoadedModules { get; } = new Dictionary<string, ModuleDefMD>();

        private readonly List<ModuleDefMD> _loadedModulesIncludeNetstandard = new List<ModuleDefMD>();

        protected AssemblyCacheBase(IAssemblyResolver assemblyResolver)
        {
            _assemblyPathResolver = assemblyResolver;
            _modCtx = ModuleDef.CreateModuleContext();
            _asmResolver = (AssemblyResolver)_modCtx.AssemblyResolver;
            _asmResolver.EnableTypeDefCache = true;
            _asmResolver.UseGAC = false;
        }


        public ModuleDefMD TryLoadModule(string moduleName, bool loadReferenceAssemblies = true)
        {
            string dllPath = _assemblyPathResolver.ResolveAssembly(moduleName, false);
            if (string.IsNullOrEmpty(dllPath))
            {
                return null;
            }
            return LoadModule(moduleName, loadReferenceAssemblies);
        }

        public ModuleDefMD LoadModule(string moduleName, bool loadReferenceAssemblies = true)
        {
            // Debug.Log($"load module:{moduleName}");
            if (LoadedModules.TryGetValue(moduleName, out var mod))
            {
                return mod;
            }
            if (moduleName == "netstandard")
            {
                if (!_loadedNetstandard)
                {
                    LoadNetStandard();
                }
                return null;
            }
            mod = DoLoadModule(_assemblyPathResolver.ResolveAssembly(moduleName, true));
            LoadedModules.Add(moduleName, mod);

            if (loadReferenceAssemblies)
            {
                foreach (var refAsm in mod.GetAssemblyRefs())
                {
                    LoadModule(refAsm.Name);
                }
            }

            return mod;
        }

        private void LoadNetStandard()
        {
            string netstandardDllPath = _assemblyPathResolver.ResolveAssembly("netstandard", false);
            if (!string.IsNullOrEmpty(netstandardDllPath))
            {
                DoLoadModule(netstandardDllPath);
            }
            else
            {
                DoLoadModule(MetaUtil.ResolveNetStandardAssemblyPath("netstandard2.0"));
                DoLoadModule(MetaUtil.ResolveNetStandardAssemblyPath("netstandard2.1"));
            }
            _loadedNetstandard = true;
        }

        private ModuleDefMD DoLoadModule(string dllPath)
        {
            //Debug.Log($"do load module:{dllPath}");
            ModuleDefMD mod = ModuleDefMD.Load(File.ReadAllBytes(dllPath), _modCtx);
            mod.EnableTypeDefFindCache = true;
            _asmResolver.AddToCache(mod);
            _loadedModulesIncludeNetstandard.Add(mod);
            return mod;
        }
    }
}
