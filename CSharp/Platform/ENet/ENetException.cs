using System;
using System.Runtime.Serialization;

namespace ENet
{
	[Serializable]
	public class ENetException: Exception
	{
		public ENetException()
		{
		}

		public ENetException(string message): base(message)
		{
		}

		public ENetException(string message, Exception inner): base(message, inner)
		{
		}

		protected ENetException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}