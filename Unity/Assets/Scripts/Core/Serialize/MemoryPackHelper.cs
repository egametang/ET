using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MemoryPack;

namespace ET
{
    public static class MemoryPackHelper
    {
        static MemoryPackHelper()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                if (type.GetCustomAttribute(typeof (MemoryPackableAttribute)) == null)
                {
                    continue;
                }

                Activator.CreateInstance(type);
            }
        }
        
        public static void Init()
        {
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

        public static byte[] Serialize(object message)
        {
            return MemoryPackSerializer.Serialize(message.GetType(), message);
        }

        public static void Serialize(object message, MemoryBuffer stream)
        {
            MemoryPackSerializer.Serialize(message.GetType(), stream, message);
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
    }
}