using System;
using System.Reflection;
namespace ProtoBuf
{
    internal enum TimeSpanScale
    {
        Days = 0,
        Hours = 1,
        Minutes = 2,
        Seconds = 3,
        Milliseconds = 4,
        Ticks = 5,

        MinMax = 15
    }

    /// <summary>
    /// Provides support for common .NET types that do not have a direct representation
    /// in protobuf, using the definitions from bcl.proto
    /// </summary>
    public
#if FX11
    sealed
#else
    static
#endif
        class BclHelpers
    {
        /// <summary>
        /// Creates a new instance of the specified type, bypassing the constructor.
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <returns>The new instance</returns>
        /// <exception cref="NotSupportedException">If the platform does not support constructor-skipping</exception>
        public static object GetUninitializedObject(Type type)
        {
#if PLAT_BINARYFORMATTER && !(WINRT || PHONE8)
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#else
            throw new NotSupportedException("Constructor-skipping is not supported on this platform");
#endif
        }
#if FX11
        private BclHelpers() { } // not a static class for C# 1.2 reasons
#endif
        const int FieldTimeSpanValue = 0x01, FieldTimeSpanScale = 0x02;
        
        internal static readonly DateTime EpochOrigin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Writes a TimeSpan to a protobuf stream
        /// </summary>
        public static void WriteTimeSpan(TimeSpan timeSpan, ProtoWriter dest)
        {
            if (dest == null) throw new ArgumentNullException("dest");
            long value;
            switch(dest.WireType)
            {
                case WireType.String:
                case WireType.StartGroup:
                    TimeSpanScale scale;
                    value = timeSpan.Ticks;
                    if (timeSpan == TimeSpan.MaxValue)
                    {
                        value = 1;
                        scale = TimeSpanScale.MinMax;
                    }
                    else if (timeSpan == TimeSpan.MinValue)
                    {
                        value = -1;
                        scale = TimeSpanScale.MinMax;
                    }
                    else if (value % TimeSpan.TicksPerDay == 0)
                    {
                        scale = TimeSpanScale.Days;
                        value /= TimeSpan.TicksPerDay;
                    }
                    else if (value % TimeSpan.TicksPerHour == 0)
                    {
                        scale = TimeSpanScale.Hours;
                        value /= TimeSpan.TicksPerHour;
                    }
                    else if (value % TimeSpan.TicksPerMinute == 0)
                    {
                        scale = TimeSpanScale.Minutes;
                        value /= TimeSpan.TicksPerMinute;
                    }
                    else if (value % TimeSpan.TicksPerSecond == 0)
                    {
                        scale = TimeSpanScale.Seconds;
                        value /= TimeSpan.TicksPerSecond;
                    }
                    else if (value % TimeSpan.TicksPerMillisecond == 0)
                    {
                        scale = TimeSpanScale.Milliseconds;
                        value /= TimeSpan.TicksPerMillisecond;
                    }
                    else
                    {
                        scale = TimeSpanScale.Ticks;
                    }

                    SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            
                    if(value != 0) {
                        ProtoWriter.WriteFieldHeader(FieldTimeSpanValue, WireType.SignedVariant, dest);
                        ProtoWriter.WriteInt64(value, dest);
                    }
                    if(scale != TimeSpanScale.Days) {
                        ProtoWriter.WriteFieldHeader(FieldTimeSpanScale, WireType.Variant, dest);
                        ProtoWriter.WriteInt32((int)scale, dest);
                    }
                    ProtoWriter.EndSubItem(token, dest);
                    break;
                case WireType.Fixed64:
                    ProtoWriter.WriteInt64(timeSpan.Ticks, dest);
                    break;
                default:
                    throw new ProtoException("Unexpected wire-type: " + dest.WireType.ToString());
            }
        }
        /// <summary>
        /// Parses a TimeSpan from a protobuf stream
        /// </summary>        
        public static TimeSpan ReadTimeSpan(ProtoReader source)
        {
            long ticks = ReadTimeSpanTicks(source);
            if (ticks == long.MinValue) return TimeSpan.MinValue;
            if (ticks == long.MaxValue) return TimeSpan.MaxValue;
            return TimeSpan.FromTicks(ticks);
        }
        /// <summary>
        /// Parses a DateTime from a protobuf stream
        /// </summary>
        public static DateTime ReadDateTime(ProtoReader source)
        {
            long ticks = ReadTimeSpanTicks(source);
            if (ticks == long.MinValue) return DateTime.MinValue;
            if (ticks == long.MaxValue) return DateTime.MaxValue;
            return EpochOrigin.AddTicks(ticks);
        }
        /// <summary>
        /// Writes a DateTime to a protobuf stream
        /// </summary>
        public static void WriteDateTime(DateTime value, ProtoWriter dest)
        {
            if (dest == null) throw new ArgumentNullException("dest");
            TimeSpan delta;
            switch (dest.WireType)
            {
                case WireType.StartGroup:
                case WireType.String:
                    if (value == DateTime.MaxValue)
                    {
                        delta = TimeSpan.MaxValue;
                    }
                    else if (value == DateTime.MinValue)
                    {
                        delta = TimeSpan.MinValue;
                    }
                    else
                    {
                        delta = value - EpochOrigin;
                    }
                    break;
                default:
                    delta = value - EpochOrigin;
                    break;
            }
            WriteTimeSpan(delta, dest);
        }

        private static long ReadTimeSpanTicks(ProtoReader source) {
            switch (source.WireType)
            {
                case WireType.String:
                case WireType.StartGroup:
                    SubItemToken token = ProtoReader.StartSubItem(source);
                    int fieldNumber;
                    TimeSpanScale scale = TimeSpanScale.Days;
                    long value = 0;
                    while ((fieldNumber = source.ReadFieldHeader()) > 0)
                    {
                        switch (fieldNumber)
                        {
                            case FieldTimeSpanScale:
                                scale = (TimeSpanScale)source.ReadInt32();
                                break;
                            case FieldTimeSpanValue:
                                source.Assert(WireType.SignedVariant);
                                value = source.ReadInt64();
                                break;
                            default:
                                source.SkipField();
                                break;
                        }
                    }
                    ProtoReader.EndSubItem(token, source);
                    switch (scale)
                    {
                        case TimeSpanScale.Days:
                            return value * TimeSpan.TicksPerDay;
                        case TimeSpanScale.Hours:
                            return value * TimeSpan.TicksPerHour;
                        case TimeSpanScale.Minutes:
                            return value * TimeSpan.TicksPerMinute;
                        case TimeSpanScale.Seconds:
                            return value * TimeSpan.TicksPerSecond;
                        case TimeSpanScale.Milliseconds:
                            return value * TimeSpan.TicksPerMillisecond;
                        case TimeSpanScale.Ticks:
                            return value;
                        case TimeSpanScale.MinMax:
                            switch (value)
                            {
                                case 1: return long.MaxValue;
                                case -1: return long.MinValue;
                                default: throw new ProtoException("Unknown min/max value: " + value.ToString());
                            }
                        default:
                            throw new ProtoException("Unknown timescale: " + scale.ToString());
                    }
                case WireType.Fixed64:
                    return source.ReadInt64();
                default:
                    throw new ProtoException("Unexpected wire-type: " + source.WireType.ToString());
            }
        }

        const int FieldDecimalLow = 0x01, FieldDecimalHigh = 0x02, FieldDecimalSignScale = 0x03;

        /// <summary>
        /// Parses a decimal from a protobuf stream
        /// </summary>
        public static decimal ReadDecimal(ProtoReader reader)
        {
            ulong low = 0;
            uint high = 0;
            uint signScale = 0;

            int fieldNumber;
            SubItemToken token = ProtoReader.StartSubItem(reader);
            while ((fieldNumber = reader.ReadFieldHeader()) > 0)
            {
                switch (fieldNumber)
                {
                    case FieldDecimalLow: low = reader.ReadUInt64(); break;
                    case FieldDecimalHigh: high = reader.ReadUInt32(); break;
                    case FieldDecimalSignScale: signScale = reader.ReadUInt32(); break;
                    default: reader.SkipField(); break;
                }
                
            }
            ProtoReader.EndSubItem(token, reader);

            if (low == 0 && high == 0) return decimal.Zero;

            int lo = (int)(low & 0xFFFFFFFFL),
                mid = (int)((low >> 32) & 0xFFFFFFFFL),
                hi = (int)high;
            bool isNeg = (signScale & 0x0001) == 0x0001;
            byte scale = (byte)((signScale & 0x01FE) >> 1);
            return new decimal(lo, mid, hi, isNeg, scale);
        }
        /// <summary>
        /// Writes a decimal to a protobuf stream
        /// </summary>
        public static void WriteDecimal(decimal value, ProtoWriter writer)
        {
            int[] bits = decimal.GetBits(value);
            ulong a = ((ulong)bits[1]) << 32, b = ((ulong)bits[0]) & 0xFFFFFFFFL;
            ulong low = a | b;
            uint high = (uint)bits[2];
            uint signScale = (uint)(((bits[3] >> 15) & 0x01FE) | ((bits[3] >> 31) & 0x0001));

            SubItemToken token = ProtoWriter.StartSubItem(null, writer);
            if (low != 0) {
                ProtoWriter.WriteFieldHeader(FieldDecimalLow, WireType.Variant, writer);
                ProtoWriter.WriteUInt64(low, writer);
            }
            if (high != 0)
            {
                ProtoWriter.WriteFieldHeader(FieldDecimalHigh, WireType.Variant, writer);
                ProtoWriter.WriteUInt32(high, writer);
            }
            if (signScale != 0)
            {
                ProtoWriter.WriteFieldHeader(FieldDecimalSignScale, WireType.Variant, writer);
                ProtoWriter.WriteUInt32(signScale, writer);
            }
            ProtoWriter.EndSubItem(token, writer);
        }

        const int FieldGuidLow = 1, FieldGuidHigh = 2;
        /// <summary>
        /// Writes a Guid to a protobuf stream
        /// </summary>        
        public static void WriteGuid(Guid value, ProtoWriter dest)
        {
            byte[] blob = value.ToByteArray();

            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            if (value != Guid.Empty)
            {
                ProtoWriter.WriteFieldHeader(FieldGuidLow, WireType.Fixed64, dest);
                ProtoWriter.WriteBytes(blob, 0, 8, dest);
                ProtoWriter.WriteFieldHeader(FieldGuidHigh, WireType.Fixed64, dest);
                ProtoWriter.WriteBytes(blob, 8, 8, dest);
            }
            ProtoWriter.EndSubItem(token, dest);
        }
        /// <summary>
        /// Parses a Guid from a protobuf stream
        /// </summary>
        public static Guid ReadGuid(ProtoReader source)
        {
            ulong low = 0, high = 0;
            int fieldNumber;
            SubItemToken token = ProtoReader.StartSubItem(source);
            while ((fieldNumber = source.ReadFieldHeader()) > 0)
            {
                switch (fieldNumber)
                {
                    case FieldGuidLow: low = source.ReadUInt64(); break;
                    case FieldGuidHigh: high = source.ReadUInt64(); break;
                    default: source.SkipField(); break;
                }
            }
            ProtoReader.EndSubItem(token, source);
            if(low == 0 && high == 0) return Guid.Empty;
            uint a = (uint)(low >> 32), b = (uint)low, c = (uint)(high >> 32), d= (uint)high;
            return new Guid((int)b, (short)a, (short)(a >> 16), 
                (byte)d, (byte)(d >> 8), (byte)(d >> 16), (byte)(d >> 24),
                (byte)c, (byte)(c >> 8), (byte)(c >> 16), (byte)(c >> 24));
            
        }


        private const int
            FieldExistingObjectKey = 1,
            FieldNewObjectKey = 2,
            FieldExistingTypeKey = 3,
            FieldNewTypeKey = 4,
            FieldTypeName = 8,
            FieldObject = 10;
        /// <summary>
        /// Optional behaviours that introduce .NET-specific functionality
        /// </summary>
        [Flags]
        public enum NetObjectOptions : byte
        {
            /// <summary>
            /// No special behaviour
            /// </summary>
            None = 0,
            /// <summary>
            /// Enables full object-tracking/full-graph support.
            /// </summary>
            AsReference = 1,
            /// <summary>
            /// Embeds the type information into the stream, allowing usage with types not known in advance.
            /// </summary>
            DynamicType = 2,
            /// <summary>
            /// If false, the constructor for the type is bypassed during deserialization, meaning any field initializers
            /// or other initialization code is skipped.
            /// </summary>
            UseConstructor = 4,
            /// <summary>
            /// Should the object index be reserved, rather than creating an object promptly
            /// </summary>
            LateSet = 8
        }
        /// <summary>
        /// Reads an *implementation specific* bundled .NET object, including (as options) type-metadata, identity/re-use, etc.
        /// </summary>
        public static object ReadNetObject(object value, ProtoReader source, int key, Type type, NetObjectOptions options)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            SubItemToken token = ProtoReader.StartSubItem(source);
            int fieldNumber;
            int newObjectKey = -1, newTypeKey = -1, tmp;
            while ((fieldNumber = source.ReadFieldHeader()) > 0)
            {
                switch (fieldNumber)
                {
                    case FieldExistingObjectKey:
                        tmp = source.ReadInt32();
                        value = source.NetCache.GetKeyedObject(tmp);
                        break;
                    case FieldNewObjectKey:
                        newObjectKey = source.ReadInt32();
                        break;
                    case FieldExistingTypeKey:
                        tmp = source.ReadInt32();
                        type = (Type)source.NetCache.GetKeyedObject(tmp);
                        key = source.GetTypeKey(ref type);
                        break;
                    case FieldNewTypeKey:
                        newTypeKey = source.ReadInt32();
                        break;
                    case FieldTypeName:
                        string typeName = source.ReadString();
                        type = source.DeserializeType(typeName);
                        if(type == null)
                        {
                            throw new ProtoException("Unable to resolve type: " + typeName + " (you can use the TypeModel.DynamicTypeFormatting event to provide a custom mapping)");
                        }
                        if (type == typeof(string))
                        {
                            key = -1;
                        }
                        else
                        {
                            key = source.GetTypeKey(ref type);
                            if (key < 0)
                                throw new InvalidOperationException("Dynamic type is not a contract-type: " + type.Name);
                        }
                        break;
                    case FieldObject:
                        bool isString = type == typeof(string);
                        bool wasNull = value == null;
                        bool lateSet = wasNull && (isString || ((options & NetObjectOptions.LateSet) != 0));
                        
                        if (newObjectKey >= 0 && !lateSet)
                        {
                            if (value == null)
                            {
                                source.TrapNextObject(newObjectKey);
                            }
                            else
                            {
                                source.NetCache.SetKeyedObject(newObjectKey, value);
                            }
                            if (newTypeKey >= 0) source.NetCache.SetKeyedObject(newTypeKey, type);
                        }
                        object oldValue = value;
                        if (isString)
                        {
                            value = source.ReadString();
                        }
                        else
                        {
                            value = ProtoReader.ReadTypedObject(oldValue, key, source, type);
                        }
                        
                        if (newObjectKey >= 0)
                        {
                            if(wasNull && !lateSet)
                            { // this both ensures (via exception) that it *was* set, and makes sure we don't shout
                                // about changed references
                                oldValue = source.NetCache.GetKeyedObject(newObjectKey);
                            }
                            if (lateSet)
                            {
                                source.NetCache.SetKeyedObject(newObjectKey, value);
                                if (newTypeKey >= 0) source.NetCache.SetKeyedObject(newTypeKey, type);
                            }
                        }
                        if (newObjectKey >= 0 && !lateSet && !ReferenceEquals(oldValue, value))
                        {
                            throw new ProtoException("A reference-tracked object changed reference during deserialization");
                        }
                        if (newObjectKey < 0 && newTypeKey >= 0)
                        {  // have a new type, but not a new object
                            source.NetCache.SetKeyedObject(newTypeKey, type);
                        }
                        break;
                    default:
                        source.SkipField();
                        break;
                }
            }
            if(newObjectKey >= 0 && (options & NetObjectOptions.AsReference) == 0)
            {
                throw new ProtoException("Object key in input stream, but reference-tracking was not expected");
            }
            ProtoReader.EndSubItem(token, source);

            return value;
#endif
        }
        /// <summary>
        /// Writes an *implementation specific* bundled .NET object, including (as options) type-metadata, identity/re-use, etc.
        /// </summary>
        public static void WriteNetObject(object value, ProtoWriter dest, int key, NetObjectOptions options)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (dest == null) throw new ArgumentNullException("dest");
            bool dynamicType = (options & NetObjectOptions.DynamicType) != 0,
                 asReference = (options & NetObjectOptions.AsReference) != 0;
            WireType wireType = dest.WireType;
            SubItemToken token = ProtoWriter.StartSubItem(null, dest);
            bool writeObject = true;
            if (asReference)
            {
                bool existing;
                int objectKey = dest.NetCache.AddObjectKey(value, out existing);
                ProtoWriter.WriteFieldHeader(existing ? FieldExistingObjectKey : FieldNewObjectKey, WireType.Variant, dest);
                ProtoWriter.WriteInt32(objectKey, dest);
                if (existing)
                {
                    writeObject = false;
                }
            }

            if (writeObject)
            {
                if (dynamicType)
                {
                    bool existing;
                    Type type = value.GetType();

                    if (!(value is string))
                    {
                        key = dest.GetTypeKey(ref type);
                        if (key < 0) throw new InvalidOperationException("Dynamic type is not a contract-type: " + type.Name);
                    }
                    int typeKey = dest.NetCache.AddObjectKey(type, out existing);
                    ProtoWriter.WriteFieldHeader(existing ? FieldExistingTypeKey : FieldNewTypeKey, WireType.Variant, dest);
                    ProtoWriter.WriteInt32(typeKey, dest);
                    if (!existing)
                    {
                        ProtoWriter.WriteFieldHeader(FieldTypeName, WireType.String, dest);
                        ProtoWriter.WriteString(dest.SerializeType(type), dest);
                    }
                    
                }
                ProtoWriter.WriteFieldHeader(FieldObject, wireType, dest);
                if (value is string)
                {
                    ProtoWriter.WriteString((string)value, dest);
                }
                else { 
                    ProtoWriter.WriteObject(value, key, dest);
                }
            }
            ProtoWriter.EndSubItem(token, dest);
#endif
        }
    }
}
