using System;
using System.Runtime.Serialization;

namespace LoginClient
{
	[Serializable]
	public class LoginException: Exception
	{
		public LoginException()
		{
		}

		public LoginException(string message): base(message)
		{
		}

		public LoginException(string message, Exception inner): base(message, inner)
		{
		}

		protected LoginException(SerializationInfo info, StreamingContext context)
		{
		}
	}
}