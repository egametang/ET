using System;
using System.IO;

namespace ProtoBuf.Meta
{
    partial class TypeModel :
        IProtoInput<Stream>,
        IProtoInput<ArraySegment<byte>>,
        IProtoInput<byte[]>,
        IProtoOutput<Stream>
    {
        static SerializationContext CreateContext(object userState)
        {
            if (userState == null)
                return SerializationContext.Default;
            if (userState is SerializationContext ctx)
                return ctx;

            var obj = new SerializationContext { Context = userState };
            obj.Freeze();
            return obj;
        }
        T IProtoInput<Stream>.Deserialize<T>(Stream source, T value, object userState)
            => (T)Deserialize(source, value, typeof(T), CreateContext(userState));

        T IProtoInput<ArraySegment<byte>>.Deserialize<T>(ArraySegment<byte> source, T value, object userState)
        {
            using (var ms = new MemoryStream(source.Array, source.Offset, source.Count))
            {
                return (T)Deserialize(ms, value, typeof(T), CreateContext(userState));
            }
        }

        T IProtoInput<byte[]>.Deserialize<T>(byte[] source, T value, object userState)
        {
            using (var ms = new MemoryStream(source))
            {
                return (T)Deserialize(ms, value, typeof(T), CreateContext(userState));
            }
        }

        void IProtoOutput<Stream>.Serialize<T>(Stream destination, T value, object userState)
            => Serialize(destination, value, CreateContext(userState));
    }
}
