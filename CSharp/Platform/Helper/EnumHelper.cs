using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
	public static class EnumHelper
	{
		public static int EnumIndex<T>(int value)
		{
			int i = 0;
			foreach (var v in Enum.GetValues(typeof (T)))
			{
				if ((int) v != value)
				{
					++i;
					continue;
				}
				return i;
			}
			return -1;
		}
	}
}
