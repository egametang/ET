using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.Meta
{
    public class FixedSetAssemblyResolver : AssemblyResolverBase
    {
        private readonly string _rootDir;
        private readonly HashSet<string> _fileNames;

        public FixedSetAssemblyResolver(string rootDir, IEnumerable<string> fileNameNotExts)
        {
            _rootDir = rootDir;
            _fileNames = new HashSet<string>(fileNameNotExts);
        }

        protected override bool TryResolveAssembly(string assemblyName, out string assemblyPath)
        {
            if (_fileNames.Contains(assemblyName))
            {
                assemblyPath = $"{_rootDir}/{assemblyName}.dll";
                if (File.Exists(assemblyPath))
                {
                    Debug.Log($"[FixedSetAssemblyResolver] resolve:{assemblyName} path:{assemblyPath}");
                    return true;
                }
            }
            assemblyPath = null;
            return false;
        }
    }
}
