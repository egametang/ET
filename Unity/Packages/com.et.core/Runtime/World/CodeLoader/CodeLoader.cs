using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
    {
        private List<Assembly> modelAssemblies;

        public void Awake()
        {
        }

        public void AddModel(params Assembly[] models)
        {
            this.modelAssemblies.Clear();
            this.modelAssemblies.AddRange(models);
        }

        public void LoadHotfix(params Assembly[] hotfixes)
        {
            List<Assembly> ass = new List<Assembly>();
            ass.AddRange(this.modelAssemblies);
            ass.AddRange(hotfixes);
            CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(ass.ToArray());
            codeTypes.CreateCode();
            
            Log.Info($"load dll finish!");
        }
    }
}