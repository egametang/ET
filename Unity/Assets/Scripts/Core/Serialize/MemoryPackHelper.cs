using System;
using System.ComponentModel;
using MemoryPack;

namespace ET
{
    public static class MemoryPackHelper
    {
        public static byte[] Serialize(object message)
        {
            return MemoryPackSerializer.Serialize(message.GetType(), message);
        }

        public static void Serialize(object message, MemoryBuffer stream)
        {
            MemoryPackSerializer.Serialize(message.GetType(), stream, message);
        }
        
        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            object o = MemoryPackSerializer.Deserialize(type, bytes.AsSpan(index, count));
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count, ref object o)
        {
            MemoryPackSerializer.Deserialize(type, bytes.AsSpan(index, count), ref o);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, MemoryBuffer stream)
        {
            object o = MemoryPackSerializer.Deserialize(type, stream.GetSpan());
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static object Deserialize(Type type, MemoryBuffer stream, ref object o)
        {
            MemoryPackSerializer.Deserialize(type, stream.GetSpan(), ref o);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}