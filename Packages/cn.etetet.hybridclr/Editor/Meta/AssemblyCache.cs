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
    public class AssemblyCache : AssemblyCacheBase
    {

        public AssemblyCache(IAssemblyResolver assemblyResolver) : base(assemblyResolver)
        {

        }
    }
}
