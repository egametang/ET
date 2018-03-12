using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace ETModel
{
	public interface IMessage
	{
	}
	
	public interface IRequest: IMessage
	{
		int RpcId { get; set; }
	}
	
	public interface IResponse: IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
		int RpcId { get; set; }
	}

	public class ResponseMessage: IResponse
	{
		public int Error { get; set; }
		public string Message { get; set; }
		public int RpcId { get; set; }
	}
}