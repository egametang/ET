using System;

namespace Model
{
	public class GameException : Exception
	{
		public GameException(string message): base(message)
		{
		}

		public GameException(string format, params object[] args)
			: base(string.Format(format, args))
		{
		}

		public GameException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public GameException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException)
		{
		}
	}
}
