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
            foreach (Assembly ass in assemblies)
            {
                if (ass.GetName().Name == "Model")
                {
                    this.assembly = ass;
                    break;
                }
            }

            Assembly hotfixAssembly = this.LoadHotfix();

            World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });

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
			
            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new[] { typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly });

            codeTypes.CreateCode();
            Log.Debug($"reload dll finish!");
        }
    }
}