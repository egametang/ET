using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
    public static class DllHelper
    {
        private static AssemblyLoadContext assemblyLoadContext;
        
        public static Assembly GetHotfixAssembly()
        {
            assemblyLoadContext?.Unload();
            System.GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Server.Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Server.Hotfix.pdb");
            Assembly assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            return assembly;
        }
    }
}