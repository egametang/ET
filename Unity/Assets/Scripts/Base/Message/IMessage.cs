using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	public struct PacketInfo
	{
		public ushort Opcode;
		public uint RpcId;
		public byte[] Bytes;
		public ushort Index;
		public ushort Length;
	}

	[ProtoContract]
	public partial class MessageObject
	{
	}
	
	public interface IMessage
	{
	}
	
	public interface IRequest: IMessage
	{
	}
	
	public interface IResponse: IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
	}
}