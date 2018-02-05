using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	public struct PacketInfo
	{
		public Header Header;
	
		public byte[] Bytes;
		public ushort Index;
		public ushort Length;
	}

	[BsonIgnoreExtraElements]
	[ProtoContract]
	public class Header
	{
		[BsonElement("a")]
		[ProtoMember(1)]
		public byte Flag;

		[BsonElement("b")]
		[ProtoMember(2)]
		public ushort Opcode;

		[BsonElement("c")]
		[BsonIgnoreIfDefault]
		[ProtoMember(3)]
		public uint RpcId;
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