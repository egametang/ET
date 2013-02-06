using System;

namespace ENet
{
	public class ENetException: Exception
	{
		public ENetException(string message): base(message)
		{
		}
	}
}