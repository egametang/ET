using System;

namespace Base
{
	/// <summary>
	/// RPC异常,带ErrorCode
	/// </summary>
	[Serializable]
	public class RpcException : Exception
	{
		public int Error { get; private set; }

		public RpcException(int error, string message) : base($"{error} : {message}")
		{
			this.Error = error;
		}

		public RpcException(int error, string message, Exception e) : base($"{error} : {message}", e)
		{
			this.Error = error;
		}
	}
}
