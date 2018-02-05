using ProtoBuf;

namespace Hotfix
{
#if ILRuntime
	public interface IMessage
	{
	}
	
	public interface IRequest
	{
	}
	
	public interface IResponse
	{
	}

	[ProtoContract]
	public class MessageObject
	{
	}
#endif
}