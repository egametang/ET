using System;
using System.IO;
using System.Reflection;

namespace Common.Helper
{
	public static class LoaderHelper
	{
		public static Assembly Load(string path)
		{
			if (!File.Exists(path))
			{
				throw new Exception($"not found path, path: {path}");
			}
			byte[] buffer = File.ReadAllBytes(path);
			Assembly assembly = Assembly.Load(buffer);
			return assembly;
		}
	}
}