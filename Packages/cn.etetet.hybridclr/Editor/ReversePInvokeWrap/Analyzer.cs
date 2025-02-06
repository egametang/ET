using dnlib.DotNet;
using HybridCLR.Editor.ABI;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallingConvention = System.Runtime.InteropServices.CallingConvention;
using UnityEngine;

namespace HybridCLR.Editor.ReversePInvokeWrap
{
    public class RawReversePInvokeMethodInfo
    {
        public MethodDef Method { get; set; }

        public CustomAttribute GenerationAttribute { get; set; }
    }

    public class ABIReversePInvokeMethodInfo
    {
        public MethodDesc Method { get; set; }

        public CallingConvention Callvention { get; set; }

        public int Count { get; set; }

        public string Signature { get; set; }
    }

    public class Analyzer
    {

        private readonly List<ModuleDefMD> _rootModules = new List<ModuleDefMD>();

        private readonly List<RawReversePInvokeMethodInfo> _reversePInvokeMethods = new List<RawReversePInvokeMethodInfo>();

        public List<RawReversePInvokeMethodInfo> ReversePInvokeMethods => _reversePInvokeMethods;

        public Analyzer(AssemblyCache cache, List<string> assemblyNames)
        {
            foreach (var assemblyName in assemblyNames)
            {
                _rootModules.Add(cache.LoadModule(assemblyName));
            }
        }

        private void CollectReversePInvokeMethods()
        {
            foreach (var mod in _rootModules)
            {
                Debug.Log($"ass:{mod.FullName} methodcount:{mod.Metadata.TablesStream.MethodTable.Rows}");
                for (uint rid = 1, n = mod.Metadata.TablesStream.MethodTable.Rows; rid <= n; rid++)
                {
                    var method = mod.ResolveMethod(rid);
                    //Debug.Log($"method:{method}");
                    if (!method.IsStatic || !method.HasCustomAttributes)
                    {
                        continue;
                    }
                    CustomAttribute wa = method.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "MonoPInvokeCallbackAttribute");
                    if (wa == null)
                    {
                        continue;
                    }
                    //foreach (var ca in method.CustomAttributes)
                    //{
                    //    Debug.Log($"{ca.AttributeType.FullName} {ca.TypeFullName}");
                    //}
                    _reversePInvokeMethods.Add(new RawReversePInvokeMethodInfo()
                    {
                        Method = method,
                        GenerationAttribute = method.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "HybridCLR.ReversePInvokeWrapperGenerationAttribute"),
                    });
                }
            }
        }

        public void Run()
        {
            CollectReversePInvokeMethods();
        }
    }
}
