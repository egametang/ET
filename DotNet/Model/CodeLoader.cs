using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
    public class CodeLoader
    {
        public static CodeLoader Instance { get; set; } = new CodeLoader();
        
        private AssemblyLoadContext assemblyLoadContext;
        
        public void LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            System.GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, this.GetType().Assembly, assembly);
			
            Game.EventSystem.Add(types);
        }
    }
}