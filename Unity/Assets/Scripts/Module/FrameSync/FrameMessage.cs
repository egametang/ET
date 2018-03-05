using System.Collections.Generic;
using ProtoBuf;

namespace ETModel
{
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
