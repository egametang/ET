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
        
        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message, int headOffset = 0)
        {
            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
            
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);
            
            stream.GetBuffer().WriteTo(headOffset, opcode);
            
            MessageSerializeHelper.Serialize(message, stream);
            
            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }
        
        public static (ushort, MemoryBuffer) ToMemoryBuffer(AService service, ActorId actorId, object message)
        {
            MemoryBuffer memoryBuffer = service.Fetch();
            ushort opcode = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message, Packet.ActorIdLength);
                    memoryBuffer.GetBuffer().WriteTo(0, actorId);
                    break;
                }
                case ServiceType.Outer:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message);
                    break;
                }
            }
            return (opcode, memoryBuffer);
        }
        
        public static (ActorId, object) ToMessage(AService service, MemoryBuffer memoryStream)
        {
            object message = null;
            ActorId actorId = default;
            switch (service.ServiceType)
            {
                case ServiceType.Outer:
                {
                    memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                    ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), 0);
                    Type type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
                case ServiceType.Inner:
                {
                    memoryStream.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
                    byte[] buffer = memoryStream.GetBuffer();
                    actorId.Process = BitConverter.ToInt32(buffer, Packet.ActorIdIndex);
                    actorId.Fiber = BitConverter.ToInt32(buffer, Packet.ActorIdIndex + 4);
                    actorId.InstanceId = BitConverter.ToInt64(buffer, Packet.ActorIdIndex + 8);
                    ushort opcode = BitConverter.ToUInt16(buffer, Packet.ActorIdLength);
                    Type type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
            }
            
            return (actorId, message);
        }
    }
}