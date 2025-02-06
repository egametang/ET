using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.HotUpdate
{
    public class MissingMetadataChecker
    {
        private readonly HashSet<string> _aotAssNames;

        private readonly AssemblyCache _assCache;

        public MissingMetadataChecker(string aotDllDir, IEnumerable<string> excludeDllNames)
        {

            var excludeDllNameSet = new HashSet<string>(excludeDllNames ?? new List<string>());
            _aotAssNames = new HashSet<string>();
            foreach (var aotFile in Directory.GetFiles(aotDllDir, "*.dll"))
            {
                string aotAssName = Path.GetFileNameWithoutExtension(aotFile);
                if (excludeDllNameSet.Contains(aotAssName))
                {
                    continue;
                }
                _aotAssNames.Add(aotAssName);
            }
            _assCache = new AssemblyCache(new PathAssemblyResolver(aotDllDir));
        }

        public bool Check(string hotUpdateDllPath)
        {
            ModuleDef mod = ModuleDefMD.Load(File.ReadAllBytes(hotUpdateDllPath), _assCache.ModCtx);

            foreach (var refass in mod.GetAssemblyRefs())
            {
                string refAssName = refass.Name;
                if (_aotAssNames.Contains(refAssName))
                {
                    _assCache.LoadModule(refass.Name, true);
                }
            }

            bool anyMissing = false;

            foreach (TypeRef typeRef in mod.GetTypeRefs())
            {
                string defAssName = typeRef.DefinitionAssembly.Name;
                if (!_aotAssNames.Contains(defAssName))
                {
                    continue;
                }
                if (typeRef.ResolveTypeDef() == null)
                {
                    UnityEngine.Debug.LogError($"Missing Type: {typeRef.FullName}");
                    anyMissing = true;
                }
            }

            foreach (IMethodDefOrRef methodRef in mod.GetMemberRefs())
            {
                if (methodRef.DeclaringType.DefinitionAssembly == null)
                {
                    continue;
                }
                string defAssName = methodRef.DeclaringType.DefinitionAssembly.Name;
                if (!_aotAssNames.Contains(defAssName))
                {
                    continue;
                }
                if (methodRef.IsField)
                {
                    
                }
                else if (methodRef.IsMethod)
                {
                    TypeSig declaringTypeSig = methodRef.DeclaringType.ToTypeSig();
                    if (methodRef.ResolveMethodDef() == null)
                    {
                        if (declaringTypeSig.ElementType == ElementType.Array || declaringTypeSig.ElementType == ElementType.SZArray)
                        {
                            continue;
                        }
                        UnityEngine.Debug.LogError($"Missing Method: {methodRef.FullName}");
                        anyMissing = true;
                    }
                }
            }
            return !anyMissing;
        }
    }
}
