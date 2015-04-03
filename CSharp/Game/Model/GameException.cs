using System;

namespace Model
{
	public class GameException: Exception
	{
		public int ErrorCode { get; private set; }

		public string ErrorInfo { get; private set; }

		public GameException(int errorCode, string errorInfo): base(errorInfo)
		{
			this.ErrorCode = errorCode;
		}
	}
}
