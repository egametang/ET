using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        private static MemoryBuffer GetStream(int count = 0)
        {
            MemoryBuffer stream;
            if (count > 0)
            {
                stream = new MemoryBuffer(count);
            }
            else
            {
                stream = new MemoryBuffer();
            }

            return stream;
        }
        
        public static (ushort, MemoryBuffer) MessageToStream(object message)
        {
            int headOffset = Packet.ActorIdLength;
            MemoryBuffer stream = GetStream(headOffset + Packet.OpcodeLength);

            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);
            
            stream.GetBuffer().WriteTo(headOffset, opcode);
            
            SerializeHelper.Serialize(message, stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            return (opcode, stream);
        }
    }
}