using System;
using System.IO;
using ProtoBuf;

namespace ET
{
    public static class ProtobufHelper
    {
        static ProtobufHelper()
        {
            var types = Game.EventSystem.GetTypes();

            foreach (Type type in types)
            {
                if (type.GetCustomAttributes(typeof (ProtoContractAttribute), false).Length == 0)
                {
                    continue;
                }
                if (!type.IsSubclassOf(typeof (ProtoObject)))
                {
                    continue;
                }

                Serializer.NonGeneric.PrepareSerializer(type);
            }
        }

        public static void Init()
        {
        }
        
        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(bytes, index, count))
            {
                return ProtoBuf.Serializer.Deserialize(type, stream);
            }
        }
        
        public static byte[] ToBytes(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }
        
        public static void ToStream(object message, MemoryStream stream)
        {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            return ProtoBuf.Serializer.Deserialize(type, stream);
        }
    }
}