using System.ComponentModel;
using MongoDB.Bson;
using ProtoBuf;

namespace Base
{
	/// <summary>
	/// 服务端回的RPC消息需要继承这个接口
	/// </summary>
	public interface IErrorMessage
	{
		ErrorMessage ErrorMessage { get; }
	}
	
	public class ErrorMessage
	{
		public int errno = 0;
		public byte[] msg = "".ToByteArray();
	}
}
