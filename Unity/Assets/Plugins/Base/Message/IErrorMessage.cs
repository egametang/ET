namespace Base
{
	/// <summary>
	/// 服务端回的RPC消息需要继承这个接口
	/// </summary>
	public interface IErrorMessage
	{
		ErrorMessage ErrorMessage { get; set; }
	}
	
	public class ErrorMessage
	{
		public int Errno = 0;
		public string Message = "";
	}
}
