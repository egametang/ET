using System;

namespace ENet
{
	public class ENetException: Exception
	{
		public ENetException(int code, string message): base(message)
		{
			this.Code = code;
		}

		public int Code { get; private set; }
	}
}