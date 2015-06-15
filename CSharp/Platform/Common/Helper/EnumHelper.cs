using System;

namespace Common.Helper
{
	public static class EnumHelper
	{
		public static int EnumIndex<T>(int value)
		{
			int i = 0;
			foreach (object v in Enum.GetValues(typeof (T)))
			{
				if ((int) v == value)
				{
					return i;
				}
				++i;
			}
			return -1;
		}

		public static T FromString<T>(string str)
		{
			return (T) Enum.Parse(typeof (T), str);
		}
	}
}