using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	public interface IActorMessage: IMessage
	{
	}

	[ProtoContract]
	public interface IActorRequest : IRequest
	{
	}

	[ProtoContract]
	public interface IActorResponse : IResponse
	{
	}

	[ProtoContract]
	public interface IFrameMessage : IActorMessage
	{
		long Id { get; set; }
	}

	[Message(Opcode.FrameMessage)]
	[ProtoContract]
	public partial class FrameMessage : MessageObject, IActorMessage
	{
		[ProtoMember(1, IsRequired = true)]
		public int Frame;

		[ProtoMember(2)]
		public List<MessageObject> Messages = new List<MessageObject>();

	}
}