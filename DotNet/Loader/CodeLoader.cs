using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace ET
{
   
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        private AssemblyLoadContext assemblyLoadContext;

        private Assembly assembly;

        public void Awake()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == "Model")
                {
                    this.assembly = assembly;
                    break;
                }
            }
            Assembly hotfixAssembly = this.LoadHotfix();
            
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly);
            World.Instance.AddSingleton<EventSystem, Dictionary<string, Type>>(types);
            
            IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start");
            start.Run();
        }

        private Assembly LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            return hotfixAssembly;
        }
        
        public void Reload()
        {
            Assembly hotfixAssembly = this.LoadHotfix();
			
            Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly);
            World.Instance.AddSingleton<EventSystem, Dictionary<string, Type>>(types, true);
			
            World.Instance.Load();
			
            Log.Debug($"reload dll finish!");
        }
    }
}