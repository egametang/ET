using System;
using System.IO;

namespace ET
{
    public static class MessagePackHelper
    {
        public static void SerializeTo(ushort opcode, object obj, MemoryStream stream)
        {
            if (opcode >= 20000)
            {
                ProtobufHelper.ToStream(obj, stream);
                return;
            }

            MongoHelper.ToBson(obj, stream);
        }
        public static object DeserializeFrom(ushort opcode, Type type, byte[] bytes, int index, int count)
        {
            if (opcode >= 20000)
            {
                return ProtobufHelper.FromBytes(type, bytes, index, count);
            }

            return MongoHelper.FromBson(type, bytes, index, count);
        }

        public static object DeserializeFrom(ushort opcode, Type type, MemoryStream stream)
        {
            if (opcode >= 20000)
            {
                return ProtobufHelper.FromStream(type, stream);
            }

            return MongoHelper.FromStream(type, stream);
        }
    }
}