using System;
using System.Linq;
using System.Reflection;

namespace ET
{
    public static class GameStart
    {
        public static void Start()
        {
            try
            {
                string[] assemblyNames = { "Unity.Model.dll", "Unity.Hotfix.dll", "Unity.ModelView.dll", "Unity.HotfixView.dll" };
				
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = $"{assembly.GetName().Name}.dll";
                    if (!assemblyNames.Contains(assemblyName))
                    {
                        continue;
                    }
                    Game.EventSystem.Add(assembly);
                }
				
                ProtobufHelper.Init();
				
                Game.Options = new Options();
				
                Game.EventSystem.Publish(new EventType.AppStart()).Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}