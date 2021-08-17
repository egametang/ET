using System.IO;
using System.Reflection;

namespace ET
{
    public static class DllHelper
    {
        public static Assembly GetHotfixAssembly()
        {
            byte[] dllBytes = File.ReadAllBytes("./Robot.Hotfix.dll");
            byte[] pdbBytes = File.ReadAllBytes("./Robot.Hotfix.pdb");
            Assembly assembly = Assembly.Load(dllBytes, pdbBytes);
            return assembly;
        }
    }
}