using System;
using System.Reflection;

namespace Hotfix
{
	public static class HotfixHelper
	{
		public static object Create(object old)
		{
			Assembly assembly = typeof(HotfixHelper).Assembly;
			string objectName = old.GetType().FullName;
			return Activator.CreateInstance(assembly.GetType(objectName));
		}
	}
}
