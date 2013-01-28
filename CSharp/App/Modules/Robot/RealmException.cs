using System;

namespace Robot
{
	public class RealmException: Exception
	{
		public RealmException(string message): base(message)
		{
		}
	}
}