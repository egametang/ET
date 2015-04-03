using System;
using System.Runtime.Serialization;

namespace Model
{
	[Serializable]
	public class GameException : Exception
	{
		public int ErrorCode { get; private set; }

		public string ErrorInfo { get; private set; }

		public GameException(int errorCode, string message): base(message)
		{
			this.ErrorCode = errorCode;
		}

		public GameException(int errorCode, string format, params object[] args)
			: base(string.Format(format, args))
		{
			this.ErrorCode = errorCode;
		}

		public GameException(int errorCode, string message, Exception innerException)
			: base(message, innerException)
		{
			this.ErrorCode = errorCode;
		}

		public GameException(int errorCode, string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException)
		{
			this.ErrorCode = errorCode;
		}

		protected GameException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override string ToString()
		{
			return string.Format("error code: {0}, {1}", this.ErrorCode, base.ToString());
		}
	}
}
