using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message)
        {
            int headOffset = Packet.ActorIdLength;

            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);
            
            stream.GetBuffer().WriteTo(headOffset, opcode);
            
            SerializeHelper.Serialize(message, stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }
    }
}