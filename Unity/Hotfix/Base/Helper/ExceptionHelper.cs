using System;

namespace Hotfix
{
	public static class ExceptionHelper
	{
		public static string ToStr(this Exception exception)
		{
			return (string)exception.Data["StackTrace"];
		}
	}
}
