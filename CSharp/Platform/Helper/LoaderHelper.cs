using System.IO;
using System.Reflection;

namespace Helper
{
	public static class LoaderHelper
	{
		public static Assembly Load(string path)
		{
			byte[] buffer = File.ReadAllBytes(path);
			var assembly = Assembly.Load(buffer);
			return assembly;
		}
	}
}