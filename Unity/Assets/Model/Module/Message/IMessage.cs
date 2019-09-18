namespace ETModel
{
	public interface IMessage
	{
	}
	
	public interface IRequest: IMessage
	{
		int RpcId { get; set; }
	}

	public interface IResponse : IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
		int RpcId { get; set; }
	}

	public class ErrorResponse : IResponse
	{
		public int Error { get; set; }
		public string Message { get; set; }
		public int RpcId { get; set; }
	}
}