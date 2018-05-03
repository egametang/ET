using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ETModel
{
    public static class DllHelper
    {
        public static Assembly GetHotfixAssembly()
        {
            try
            {
                byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
#if __MonoCS__
			byte[] pdbBytes = File.ReadAllBytes("./Hotfix.dll.mdb");
#else
                byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
#endif
                Assembly assembly = Assembly.Load(dllBytes, pdbBytes);
                return assembly;
            }
            catch (Exception e)
            {
                Log.Error($"无法加载热更新程序集.请确认已经生成了Hotfix.dll ----- {e.Message}");
                throw;
            }
        }

        public static Type[] GetMonoTypes()
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in Game.EventSystem.GetAll())
            {
                types.AddRange(assembly.GetTypes());
            }
            return types.ToArray();
        }
    }
}
