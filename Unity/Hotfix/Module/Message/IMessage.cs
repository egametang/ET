using ProtoBuf;

namespace Hotfix
{
	public partial class MessageObject
	{
	}

	public interface IMessage
	{
	}
	
	public interface IRequest
	{
	}

	public interface IResponse : IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
	}
}