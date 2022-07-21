using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bright.Serialization
{
    public static class ByteBufExtensions
    {
        public static void WriteUnityVector2(this ByteBuf buf, UnityEngine.Vector2 v)
        {
            buf.WriteFloat(v.x);
            buf.WriteFloat(v.y);
        }

        public static UnityEngine.Vector2 ReadUnityVector2(this ByteBuf buf)
        {
            return new UnityEngine.Vector2(buf.ReadFloat(), buf.ReadFloat());
        }

        public static void WriteUnityVector3(this ByteBuf buf, UnityEngine.Vector3 v)
        {
            buf.WriteFloat(v.x);
            buf.WriteFloat(v.y);
            buf.WriteFloat(v.z);
        }

        public static UnityEngine.Vector3 ReadUnityVector3(this ByteBuf buf)
        {
            return new UnityEngine.Vector3(buf.ReadFloat(), buf.ReadFloat(), buf.ReadFloat());
        }

        public static void WriteUnityVector4(this ByteBuf buf, UnityEngine.Vector4 v)
        {
            buf.WriteFloat(v.x);
            buf.WriteFloat(v.y);
            buf.WriteFloat(v.z);
            buf.WriteFloat(v.w);
        }

        public static UnityEngine.Vector4 ReadUnityVector4(this ByteBuf buf)
        {
            return new UnityEngine.Vector4(buf.ReadFloat(), buf.ReadFloat(), buf.ReadFloat(), buf.ReadFloat());
        }
    }
}
