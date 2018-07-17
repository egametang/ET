using System.Collections.Generic;

namespace ETModel
{

    [Message(Opcode.OneFrameMessage)]
    public partial class OneFrameMessage: IActorMessage
    {
        //[ProtoMember(90, IsRequired = true)]
        //public int RpcId { get; set; }
		//
        //[ProtoMember(93, IsRequired = true)]
        //public long ActorId { get; set; }
		//
        //[ProtoMember(1, IsRequired = true)]
        //public ushort Op;
		//
        //[ProtoMember(2, IsRequired = true)]
        //public byte[] AMessage;

    }

    [Message(Opcode.FrameMessage)]
    public partial class FrameMessage: IActorMessage
    {
        //[ProtoMember(90, IsRequired = true)]
        //public int RpcId { get; set; }
		//
        //[ProtoMember(93, IsRequired = true)]
        //public long ActorId { get; set; }
		//
        //[ProtoMember(1, IsRequired = true)]
        //public int Frame;
		//
        //[ProtoMember(2)]
        //public List<OneFrameMessage> Messages = new List<OneFrameMessage>();

    }
}