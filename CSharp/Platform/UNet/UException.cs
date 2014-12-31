using System;
using System.Runtime.Serialization;

namespace UNet
{
	[Serializable]
	public class UException: Exception
	{
		public UException()
		{
		}

		public UException(string message): base(message)
		{
		}

		public UException(string message, Exception inner): base(message, inner)
		{
		}

		protected UException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}