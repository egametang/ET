using System.Reflection;

namespace ET
{
	public static class MethodInfoHelper
	{
		public static void Run(this MethodInfo methodInfo, object obj, params object[] param)
		{

			if (methodInfo.IsStatic)
			{
				object[] p = new object[param.Length + 1];
				p[0] = obj;
				for (int i = 0; i < param.Length; ++i)
				{
					p[i + 1] = param[i];
				}
				methodInfo.Invoke(null, p);
			}
			else
			{
				methodInfo.Invoke(obj, param);
			}
		}
	}
}