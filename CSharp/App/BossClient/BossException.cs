using System;
using System.Runtime.Serialization;

namespace BossClient
{
	[Serializable]
	public class BossException: Exception
	{
		public BossException()
		{
		}

		public BossException(string message): base(message)
		{
		}

		public BossException(string message, Exception inner): base(message, inner)
		{
		}

		protected BossException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}