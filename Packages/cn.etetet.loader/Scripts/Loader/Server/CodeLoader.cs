#if DOTNET
using System;
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
        }

        public void Start()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies)
            {
                if (ass.GetName().Name == "ET.Model")
                {
                    this.assembly = ass;
                    break;
                }
            }

            Assembly hotfixAssembly = this.LoadHotfix();

            World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly]);

            IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start");
            start.Run();
        }

        private Assembly LoadHotfix()
        {
            assemblyLoadContext?.Unload();
            GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("ET.Hotfix", true);
            byte[] dllBytes = File.ReadAllBytes("./Bin/ET.Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Bin/ET.Hotfix.pdb");
            Assembly hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            return hotfixAssembly;
        }
        
        public void Reload()
        {
            Assembly hotfixAssembly = this.LoadHotfix();
			
            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly
            ]);

            codeTypes.CodeProcess();
            Log.Debug($"reload dll finish!");
        }
    }
}
#endif