using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
	public class RealmException: Exception
	{
		public RealmException(string message): base(message)
		{
		}
	}
}
