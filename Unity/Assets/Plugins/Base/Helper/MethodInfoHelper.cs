using System.Reflection;

namespace Base
{
	public static class MethodInfoHelper
	{
		public static void Run(this MethodInfo methodInfo, object obj)
		{
			if (methodInfo.IsStatic)
			{
				methodInfo.Invoke(null, new object[] { obj });
			}
			else
			{
				methodInfo.Invoke(obj, new object[] { });
			}
		}

		public static void Run(this MethodInfo methodInfo, object obj, object p1)
		{
			if (methodInfo.IsStatic)
			{
				methodInfo.Invoke(null, new object[] { obj, p1 });
			}
			else
			{
				methodInfo.Invoke(obj, new object[] { p1 });
			}
		}

		public static void Run(this MethodInfo methodInfo, object obj, object p1, object p2)
		{
			if (methodInfo.IsStatic)
			{
				methodInfo.Invoke(null, new object[] { obj, p1, p2 });
			}
			else
			{
				methodInfo.Invoke(obj, new object[] { p1, p2 });
			}
		}

		public static void Run(this MethodInfo methodInfo, object obj, object p1, object p2, object p3)
		{
			if (methodInfo.IsStatic)
			{
				methodInfo.Invoke(null, new object[] { obj, p1, p2, p3 });
			}
			else
			{
				methodInfo.Invoke(obj, new object[] { p1, p2, p3 });
			}
		}
	}
}