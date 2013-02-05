using System;

namespace LoginClient
{
	public class LoginException: Exception
	{
		public LoginException(string message): base(message)
		{
		}
	}
}