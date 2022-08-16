using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
    public class CodeLoader: IDisposable
    {
        private AssemblyLoadContext assemblyLoadContext;
        
        private Assembly hotfix;
        
        private static CodeLoader instance;
		
        public static CodeLoader Instance 
        {
            get
            {
                return instance ??= new CodeLoader();
            }
        }

        private CodeLoader()
        {
        }
		
        public void Dispose()
        {
            instance = null;
        }

        public void Start()
        {
            this.LoadHotfix();
            
            Entry.Start();
        }

        public void LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            this.hotfix = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof(Init).Assembly, typeof (Game).Assembly, typeof(Entry).Assembly, this.hotfix);
			
            Game.EventSystem.Add(types);
        }
    }
}