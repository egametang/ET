using System;
using ETModel;

namespace ETHotfix
{
	public static class ExceptionHelper
	{
		public static string ToStr(this Exception exception)
		{
			if (Define.IsILRuntime)
			{
				return (string) exception.Data["StackTrace"];
			}

			return exception.ToString();
		}
	}
}
