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
			byte[] dllBytes = File.ReadAllBytes("./Hotfix.dll");
#if __MonoCS__
			byte[] pdbBytes = File.ReadAllBytes("./Hotfix.dll.mdb");
#else
			byte[] pdbBytes = File.ReadAllBytes("./Hotfix.pdb");
#endif
			Assembly assembly = Assembly.Load(dllBytes, pdbBytes);
			return assembly;
		}
	}
}
