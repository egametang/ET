using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        public static byte[] Serialize(MessageObject message)
        {
            return MemoryPackHelper.Serialize(message);
        }

        public static void Serialize(MessageObject message, MemoryBuffer stream)
        {
            MemoryPackHelper.Serialize(message, stream);
        }
		
        public static MessageObject Deserialize(Type type, byte[] bytes, int index, int count)
        {
            object o = ObjectPool.Instance.Fetch(type);
            MemoryPackHelper.Deserialize(type, bytes, index, count, ref o);
            return o as MessageObject;
        }

        public static MessageObject Deserialize(Type type, MemoryBuffer stream)
        {
            object o = ObjectPool.Instance.Fetch(type);
            MemoryPackHelper.Deserialize(type, stream, ref o);
            return o as MessageObject;
        }
        
        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message)
        {
            int headOffset = Packet.ActorIdLength;

            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);
            
            stream.GetBuffer().WriteTo(headOffset, opcode);
            
            MessageSerializeHelper.Serialize(message, stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }
    }
}