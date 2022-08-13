using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET.Server
{
    public class CodeLoader
    {
        public static CodeLoader Instance { get; set; } = new CodeLoader();
        
        private AssemblyLoadContext assemblyLoadContext;
        
        public void LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof(Program).Assembly, typeof (Game).Assembly, typeof(Entry).Assembly, assembly);
			
            Game.EventSystem.Add(types);
        }
    }
}