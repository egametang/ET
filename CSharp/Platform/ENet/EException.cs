using System;
using System.Runtime.Serialization;

namespace ENet
{
	[Serializable]
	public class EException: Exception
	{
		public EException()
		{
		}

		public EException(string message): base(message)
		{
		}

		public EException(string message, Exception inner): base(message, inner)
		{
		}

		protected EException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}