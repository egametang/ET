#if PLAT_BINARYFORMATTER
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ProtoBuf
{
    [Serializable]
    public readonly partial struct DiscriminatedUnionObject : ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (Discriminator != default) info.AddValue("d", Discriminator);
            if (Object is object) info.AddValue("o", Object);
        }
        private DiscriminatedUnionObject(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": Discriminator = (int)field.Value; break;
                    case "o": Object = field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion128Object : ISerializable
    {
        [FieldOffset(8)] private readonly long _lo;
        [FieldOffset(16)] private readonly long _hi;
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (_lo != default) info.AddValue("l", _lo);
            if (_hi != default) info.AddValue("h", _hi);
            if (Object != null) info.AddValue("o", Object);
        }
        private DiscriminatedUnion128Object(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "l": _lo = (long)field.Value; break;
                    case "h": _hi = (long)field.Value; break;
                    case "o": Object = field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion128 : ISerializable
    {
        [FieldOffset(8)] private readonly long _lo;
        [FieldOffset(16)] private readonly long _hi;
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (_lo != default) info.AddValue("l", _lo);
            if (_hi != default) info.AddValue("h", _hi);
        }
        private DiscriminatedUnion128(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "l": _lo = (long)field.Value; break;
                    case "h": _hi = (long)field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion64 : ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (Int64 != default) info.AddValue("i", Int64);
        }
        private DiscriminatedUnion64(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "i": Int64 = (long)field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion64Object : ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (Int64 != default) info.AddValue("i", Int64);
            if (Object is object) info.AddValue("o", Object);
        }
        private DiscriminatedUnion64Object(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "i": Int64 = (long)field.Value; break;
                    case "o": Object = field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion32 : ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (Int32 != default) info.AddValue("i", Int32);
        }
        private DiscriminatedUnion32(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "i": Int32 = (int)field.Value; break;
                }
            }
        }
    }

    [Serializable]
    public readonly partial struct DiscriminatedUnion32Object : ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_discriminator != default) info.AddValue("d", _discriminator);
            if (Int32 != default) info.AddValue("i", Int32);
            if (Object is object) info.AddValue("o", Object);
        }
        private DiscriminatedUnion32Object(SerializationInfo info, StreamingContext context)
        {
            this = default;
            foreach (var field in info)
            {
                switch (field.Name)
                {
                    case "d": _discriminator = (int)field.Value; break;
                    case "i": Int32 = (int)field.Value; break;
                    case "o": Object = field.Value; break;
                }
            }
        }
    }
}
#endif