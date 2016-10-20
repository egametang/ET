using System.IO;
using System.Reflection;

namespace Model
{
	public static class DllHelper
	{
		public static Assembly GetController()
		{
			byte[] dllBytes = File.ReadAllBytes("./Controller.dll");
#if __MonoCS__
			byte[] pdbBytes = File.ReadAllBytes("./Controller.dll.mdb");
#else
			byte[] pdbBytes = File.ReadAllBytes("./Controller.pdb");
#endif
			Assembly assembly = Assembly.Load(dllBytes, pdbBytes);
			return assembly;
		}
	}
}
