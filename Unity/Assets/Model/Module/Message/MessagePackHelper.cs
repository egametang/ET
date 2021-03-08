using System;
using System.IO;

namespace ET
{
    /*
    public static class MessagePackHelper
    {
        static MessagePackHelper()
        {
            UnityTypeBindings.Register();
        }

        public static void Init()
        {
        }
        
        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(bytes, index, count);
            return MessagePack.MessagePackSerializer.Deserialize(type, memory);
        }
        
        public static byte[] ToBytes(object message)
        {
            return MessagePack.MessagePackSerializer.Serialize(message);
        }
        
        public static void ToStream(object message, MemoryStream stream)
        {
            MessagePack.MessagePackSerializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            return MessagePack.MessagePackSerializer.Deserialize(type, stream);
        }
        
        public static string ToJson(object message)
        {
            return LitJson.JsonMapper.ToJson(message);
        }
        
        public static object FromJson(Type type, string json)
        {
            return LitJson.JsonMapper.ToObject(json, type);
        }
        
        public static T FromJson<T>(string json)
        {
            return LitJson.JsonMapper.ToObject<T>(json);
        }
    }
    */
}