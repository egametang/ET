using System;
using System.IO;

using System.Collections;
#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
using IKVM.Reflection;
#else
using System.Reflection;
#endif

namespace ProtoBuf.Meta
{
    /// <summary>
    /// Provides protobuf serialization support for a number of types
    /// </summary>
    public abstract class TypeModel
    {
#if WINRT
        internal TypeInfo MapType(TypeInfo type)
        {
            return type;
        }
#endif

        /// <summary>
        /// Should the <c>Kind</c> be included on date/time values?
        /// </summary>
        protected internal virtual bool SerializeDateTimeKind() { return false; }

        /// <summary>
        /// Resolve a System.Type to the compiler-specific type
        /// </summary>
        protected internal Type MapType(System.Type type)
        {
            return MapType(type, true);
        }
        /// <summary>
        /// Resolve a System.Type to the compiler-specific type
        /// </summary>
        protected internal virtual Type MapType(System.Type type, bool demand)
        {
#if FEAT_IKVM
            throw new NotImplementedException(); // this should come from RuntimeTypeModel!
#else
            return type;
#endif
        }
        private WireType GetWireType(ProtoTypeCode code, DataFormat format, ref Type type, out int modelKey)
        {
            modelKey = -1;
            if (Helpers.IsEnum(type))
            {
                modelKey = GetKey(ref type);
                return WireType.Variant;
            }
            switch (code)
            {
                case ProtoTypeCode.Int64:
                case ProtoTypeCode.UInt64:
                    return format == DataFormat.FixedSize ? WireType.Fixed64 : WireType.Variant;
                case ProtoTypeCode.Int16:
                case ProtoTypeCode.Int32:
                case ProtoTypeCode.UInt16:
                case ProtoTypeCode.UInt32:
                case ProtoTypeCode.Boolean:
                case ProtoTypeCode.SByte:
                case ProtoTypeCode.Byte:
                case ProtoTypeCode.Char:
                    return format == DataFormat.FixedSize ? WireType.Fixed32 : WireType.Variant;
                case ProtoTypeCode.Double:
                    return WireType.Fixed64;
                case ProtoTypeCode.Single:
                    return WireType.Fixed32;
                case ProtoTypeCode.String:
                case ProtoTypeCode.DateTime:
                case ProtoTypeCode.Decimal:
                case ProtoTypeCode.ByteArray:
                case ProtoTypeCode.TimeSpan:
                case ProtoTypeCode.Guid:
                case ProtoTypeCode.Uri:
                    return WireType.String;
            }
            
            if ((modelKey = GetKey(ref type)) >= 0)
            {
                return WireType.String;
            }
            return WireType.None;
        }

#if !FEAT_IKVM
        /// <summary>
        /// This is the more "complete" version of Serialize, which handles single instances of mapped types.
        /// The value is written as a complete field, including field-header and (for sub-objects) a
        /// length-prefix
        /// In addition to that, this provides support for:
        ///  - basic values; individual int / string / Guid / etc
        ///  - IEnumerable sequences of any type handled by TrySerializeAuxiliaryType
        ///  
        /// </summary>
        internal bool TrySerializeAuxiliaryType(ProtoWriter writer,  Type type, DataFormat format, int tag, object value, bool isInsideList)
        {
            if (type == null) { type = value.GetType(); }

            ProtoTypeCode typecode = Helpers.GetTypeCode(type);
            int modelKey;
            // note the "ref type" here normalizes against proxies
            WireType wireType = GetWireType(typecode, format, ref type, out modelKey);


            if (modelKey >= 0)
            {   // write the header, but defer to the model
                if (Helpers.IsEnum(type))
                { // no header
                    Serialize(modelKey, value, writer);
                    return true;
                }
                else
                {
                    ProtoWriter.WriteFieldHeader(tag, wireType, writer);
                    switch (wireType)
                    {
                        case WireType.None:
                            throw ProtoWriter.CreateException(writer);
                        case WireType.StartGroup:
                        case WireType.String:
                            // needs a wrapping length etc
                            SubItemToken token = ProtoWriter.StartSubItem(value, writer);
                            Serialize(modelKey, value, writer);
                            ProtoWriter.EndSubItem(token, writer);
                            return true;
                        default:
                            Serialize(modelKey, value, writer);
                            return true;
                    }
                }
            }
            
            if(wireType != WireType.None) {
                ProtoWriter.WriteFieldHeader(tag, wireType, writer);
            }
            switch(typecode) {
                case ProtoTypeCode.Int16: ProtoWriter.WriteInt16((short)value, writer); return true;
                case ProtoTypeCode.Int32: ProtoWriter.WriteInt32((int)value, writer); return true;
                case ProtoTypeCode.Int64: ProtoWriter.WriteInt64((long)value, writer); return true;
                case ProtoTypeCode.UInt16: ProtoWriter.WriteUInt16((ushort)value, writer); return true;
                case ProtoTypeCode.UInt32: ProtoWriter.WriteUInt32((uint)value, writer); return true;
                case ProtoTypeCode.UInt64: ProtoWriter.WriteUInt64((ulong)value, writer); return true;
                case ProtoTypeCode.Boolean: ProtoWriter.WriteBoolean((bool)value, writer); return true;
                case ProtoTypeCode.SByte: ProtoWriter.WriteSByte((sbyte)value, writer); return true;
                case ProtoTypeCode.Byte: ProtoWriter.WriteByte((byte)value, writer); return true;
                case ProtoTypeCode.Char: ProtoWriter.WriteUInt16((ushort)(char)value, writer); return true;
                case ProtoTypeCode.Double: ProtoWriter.WriteDouble((double)value, writer); return true;
                case ProtoTypeCode.Single: ProtoWriter.WriteSingle((float)value, writer); return true;
                case ProtoTypeCode.DateTime:
                    if (SerializeDateTimeKind())
                        BclHelpers.WriteDateTimeWithKind((DateTime)value, writer);
                    else
                        BclHelpers.WriteDateTime((DateTime)value, writer);
                    return true;
                case ProtoTypeCode.Decimal: BclHelpers.WriteDecimal((decimal)value, writer); return true;
                case ProtoTypeCode.String: ProtoWriter.WriteString((string)value, writer); return true;
                case ProtoTypeCode.ByteArray: ProtoWriter.WriteBytes((byte[])value, writer); return true;
                case ProtoTypeCode.TimeSpan: BclHelpers.WriteTimeSpan((TimeSpan)value, writer); return true;
                case ProtoTypeCode.Guid: BclHelpers.WriteGuid((Guid)value, writer); return true;
                case ProtoTypeCode.Uri: ProtoWriter.WriteString(((Uri)value).AbsoluteUri, writer); return true;
            }

            // by now, we should have covered all the simple cases; if we wrote a field-header, we have
            // forgotten something!
            Helpers.DebugAssert(wireType == WireType.None);

            // now attempt to handle sequences (including arrays and lists)
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                if (isInsideList) throw CreateNestedListsNotSupported();
                foreach (object item in sequence) {
                    if (item == null) { throw new NullReferenceException(); }
                    if (!TrySerializeAuxiliaryType(writer, null, format, tag, item, true))
                    {
                        ThrowUnexpectedType(item.GetType());
                    }
                }
                return true;
            }
            return false;
        }
        private void SerializeCore(ProtoWriter writer, object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            Type type = value.GetType();
            int key = GetKey(ref type);
            if (key >= 0)
            {
                Serialize(key, value, writer);
            }
            else if (!TrySerializeAuxiliaryType(writer, type, DataFormat.Default, Serializer.ListItemTag, value, false))
            {
                ThrowUnexpectedType(type);
            }
        }
#endif
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream.
        /// </summary>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="dest">The destination stream to write to.</param>
        public void Serialize(Stream dest, object value)
        {
            Serialize(dest, value, null);
        }
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream.
        /// </summary>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="dest">The destination stream to write to.</param>
        /// <param name="context">Additional information about this serialization operation.</param>
        public void Serialize(Stream dest, object value, SerializationContext context)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            using (ProtoWriter writer = new ProtoWriter(dest, this, context))
            {
                writer.SetRootObject(value);
                SerializeCore(writer, value);
                writer.Close();
            }
#endif
        }
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied writer.
        /// </summary>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="dest">The destination writer to write to.</param>
        public void Serialize(ProtoWriter dest, object value)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (dest == null) throw new ArgumentNullException("dest");
            dest.CheckDepthFlushlock();
            dest.SetRootObject(value);
            SerializeCore(dest, value);
            dest.CheckDepthFlushlock();
            ProtoWriter.Flush(dest);
#endif
        }

        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (or null), using length-prefixed
        /// data - useful with network IO.
        /// </summary>
        /// <param name="type">The type being merged.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="fieldNumber">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int fieldNumber)
        {
            int bytesRead;
            return DeserializeWithLengthPrefix(source, value, type, style, fieldNumber, null, out bytesRead);
        }


        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (or null), using length-prefixed
        /// data - useful with network IO.
        /// </summary>
        /// <param name="type">The type being merged.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="expectedField">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        /// <param name="resolver">Used to resolve types on a per-field basis.</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver)
        {
            int bytesRead;
            return DeserializeWithLengthPrefix(source, value, type, style, expectedField, resolver, out bytesRead);
        }

        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (or null), using length-prefixed
        /// data - useful with network IO.
        /// </summary>
        /// <param name="type">The type being merged.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="expectedField">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        /// <param name="resolver">Used to resolve types on a per-field basis.</param>
        /// <param name="bytesRead">Returns the number of bytes consumed by this operation (includes length-prefix overheads and any skipped data).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, out int bytesRead)
        {
            bool haveObject;
            return DeserializeWithLengthPrefix(source, value, type, style, expectedField, resolver, out bytesRead, out haveObject, null);
        }

        private object DeserializeWithLengthPrefix(Stream source, object value, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, out int bytesRead, out bool haveObject, SerializationContext context)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            haveObject = false;
            bool skip;
            int len;
            int tmpBytesRead;
            bytesRead = 0;
            if (type == null && (style != PrefixStyle.Base128 || resolver == null))
            {
                throw new InvalidOperationException("A type must be provided unless base-128 prefixing is being used in combination with a resolver");
            }
            int actualField;
            do
            {
                
                bool expectPrefix = expectedField > 0 || resolver != null;
                len = ProtoReader.ReadLengthPrefix(source, expectPrefix, style, out actualField, out tmpBytesRead);
                if (tmpBytesRead == 0) return value;
                bytesRead += tmpBytesRead;
                if (len < 0) return value;

                switch (style)
                {
                    case PrefixStyle.Base128:
                        if (expectPrefix && expectedField == 0 && type == null && resolver != null)
                        {
                            type = resolver(actualField);
                            skip = type == null;
                        }
                        else { skip = expectedField != actualField; }
                        break;
                    default:
                        skip = false;
                        break;
                }

                if (skip)
                {
                    if (len == int.MaxValue) throw new InvalidOperationException();
                    ProtoReader.Seek(source, len, null);
                    bytesRead += len;
                }
            } while (skip);

            ProtoReader reader = null;
            try
            {
                reader = ProtoReader.Create(source, this, context, len);
                int key = GetKey(ref type);
                if (key >= 0 && !Helpers.IsEnum(type))
                {
                    value = Deserialize(key, value, reader);
                }
                else
                {
                    if (!(TryDeserializeAuxiliaryType(reader, DataFormat.Default, Serializer.ListItemTag, type, ref value, true, false, true, false) || len == 0))
                    {
                        TypeModel.ThrowUnexpectedType(type); // throws
                    }
                }
                bytesRead += reader.Position;
                haveObject = true;
                return value;
            }
            finally
            {
                ProtoReader.Recycle(reader);
            }
#endif
        }
        /// <summary>
        /// Reads a sequence of consecutive length-prefixed items from a stream, using
        /// either base-128 or fixed-length prefixes. Base-128 prefixes with a tag
        /// are directly comparable to serializing multiple items in succession
        /// (use the <see cref="Serializer.ListItemTag"/> tag to emulate the implicit behavior
        /// when serializing a list/array). When a tag is
        /// specified, any records with different tags are silently omitted. The
        /// tag is ignored. The tag is ignores for fixed-length prefixes.
        /// </summary>
        /// <param name="source">The binary stream containing the serialized records.</param>
        /// <param name="style">The prefix style used in the data.</param>
        /// <param name="expectedField">The tag of records to return (if non-positive, then no tag is
        /// expected and all records are returned).</param>
        /// <param name="resolver">On a field-by-field basis, the type of object to deserialize (can be null if "type" is specified). </param>
        /// <param name="type">The type of object to deserialize (can be null if "resolver" is specified).</param>
        /// <returns>The sequence of deserialized objects.</returns>
        public System.Collections.IEnumerable DeserializeItems(System.IO.Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver)
        {
            return DeserializeItems(source, type, style, expectedField, resolver, null);
        }
        /// <summary>
        /// Reads a sequence of consecutive length-prefixed items from a stream, using
        /// either base-128 or fixed-length prefixes. Base-128 prefixes with a tag
        /// are directly comparable to serializing multiple items in succession
        /// (use the <see cref="Serializer.ListItemTag"/> tag to emulate the implicit behavior
        /// when serializing a list/array). When a tag is
        /// specified, any records with different tags are silently omitted. The
        /// tag is ignored. The tag is ignores for fixed-length prefixes.
        /// </summary>
        /// <param name="source">The binary stream containing the serialized records.</param>
        /// <param name="style">The prefix style used in the data.</param>
        /// <param name="expectedField">The tag of records to return (if non-positive, then no tag is
        /// expected and all records are returned).</param>
        /// <param name="resolver">On a field-by-field basis, the type of object to deserialize (can be null if "type" is specified). </param>
        /// <param name="type">The type of object to deserialize (can be null if "resolver" is specified).</param>
        /// <returns>The sequence of deserialized objects.</returns>
        /// <param name="context">Additional information about this serialization operation.</param>
        public System.Collections.IEnumerable DeserializeItems(System.IO.Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, SerializationContext context)
        {
            return new DeserializeItemsIterator(this, source, type, style, expectedField, resolver, context);
        }

#if !NO_GENERICS
        /// <summary>
        /// Reads a sequence of consecutive length-prefixed items from a stream, using
        /// either base-128 or fixed-length prefixes. Base-128 prefixes with a tag
        /// are directly comparable to serializing multiple items in succession
        /// (use the <see cref="Serializer.ListItemTag"/> tag to emulate the implicit behavior
        /// when serializing a list/array). When a tag is
        /// specified, any records with different tags are silently omitted. The
        /// tag is ignored. The tag is ignores for fixed-length prefixes.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="source">The binary stream containing the serialized records.</param>
        /// <param name="style">The prefix style used in the data.</param>
        /// <param name="expectedField">The tag of records to return (if non-positive, then no tag is
        /// expected and all records are returned).</param>
        /// <returns>The sequence of deserialized objects.</returns>
        public System.Collections.Generic.IEnumerable<T> DeserializeItems<T>(Stream source, PrefixStyle style, int expectedField)
        {
            return DeserializeItems<T>(source, style, expectedField, null);
        }
        /// <summary>
        /// Reads a sequence of consecutive length-prefixed items from a stream, using
        /// either base-128 or fixed-length prefixes. Base-128 prefixes with a tag
        /// are directly comparable to serializing multiple items in succession
        /// (use the <see cref="Serializer.ListItemTag"/> tag to emulate the implicit behavior
        /// when serializing a list/array). When a tag is
        /// specified, any records with different tags are silently omitted. The
        /// tag is ignored. The tag is ignores for fixed-length prefixes.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="source">The binary stream containing the serialized records.</param>
        /// <param name="style">The prefix style used in the data.</param>
        /// <param name="expectedField">The tag of records to return (if non-positive, then no tag is
        /// expected and all records are returned).</param>
        /// <returns>The sequence of deserialized objects.</returns>
        /// <param name="context">Additional information about this serialization operation.</param>
        public System.Collections.Generic.IEnumerable<T> DeserializeItems<T>(Stream source, PrefixStyle style, int expectedField, SerializationContext context)
        {
            return new DeserializeItemsIterator<T>(this, source, style, expectedField, context);
        }

        private sealed class DeserializeItemsIterator<T> : DeserializeItemsIterator,
            System.Collections.Generic.IEnumerator<T>,
            System.Collections.Generic.IEnumerable<T>
        {
            System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator() { return this; }
            public new T Current { get { return (T)base.Current; } }
            void IDisposable.Dispose() { }
            public DeserializeItemsIterator(TypeModel model, Stream source, PrefixStyle style, int expectedField, SerializationContext context)
                : base(model, source, model.MapType(typeof(T)), style, expectedField, null, context) { }
        }
#endif
        private class DeserializeItemsIterator : IEnumerator, IEnumerable
        {
            IEnumerator IEnumerable.GetEnumerator() { return this; }
            private bool haveObject;
            private object current;
            public bool MoveNext()
            {
                if (haveObject)
                {
                    int bytesRead;
                    current = model.DeserializeWithLengthPrefix(source, null, type, style, expectedField, resolver, out bytesRead, out haveObject, context);
                }
                return haveObject;
            }
            void IEnumerator.Reset() { throw new NotSupportedException(); }
            public object Current { get { return current; } }
            private readonly Stream source;
            private readonly Type type;
            private readonly PrefixStyle style;
            private readonly int expectedField;
            private readonly Serializer.TypeResolver resolver;
            private readonly TypeModel model;
            private readonly SerializationContext context;
            public DeserializeItemsIterator(TypeModel model, Stream source, Type type, PrefixStyle style, int expectedField, Serializer.TypeResolver resolver, SerializationContext context)
            {
                haveObject = true;
                this.source = source;
                this.type = type;
                this.style = style;
                this.expectedField = expectedField;
                this.resolver = resolver;
                this.model = model;
                this.context = context;
            }
        }

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream,
        /// with a length-prefix. This is useful for socket programming,
        /// as DeserializeWithLengthPrefix can be used to read the single object back
        /// from an ongoing stream.
        /// </summary>
        /// <param name="type">The type being serialized.</param>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="dest">The destination stream to write to.</param>
        /// <param name="fieldNumber">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        public void SerializeWithLengthPrefix(Stream dest, object value, Type type, PrefixStyle style, int fieldNumber)
        {
            SerializeWithLengthPrefix(dest, value, type, style, fieldNumber, null);
        }
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream,
        /// with a length-prefix. This is useful for socket programming,
        /// as DeserializeWithLengthPrefix can be used to read the single object back
        /// from an ongoing stream.
        /// </summary>
        /// <param name="type">The type being serialized.</param>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="dest">The destination stream to write to.</param>
        /// <param name="fieldNumber">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        /// <param name="context">Additional information about this serialization operation.</param>
        public void SerializeWithLengthPrefix(Stream dest, object value, Type type, PrefixStyle style, int fieldNumber, SerializationContext context)
        {
            if (type == null)
            {
                if(value == null) throw new ArgumentNullException("value");
                type = MapType(value.GetType());
            }
            int key = GetKey(ref type);
            using (ProtoWriter writer = new ProtoWriter(dest, this, context))
            {
                switch (style)
                {
                    case PrefixStyle.None:
                        Serialize(key, value, writer);
                        break;
                    case PrefixStyle.Base128:
                    case PrefixStyle.Fixed32:
                    case PrefixStyle.Fixed32BigEndian:
                        ProtoWriter.WriteObject(value, key, writer, style, fieldNumber);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("style");
                }
                writer.Close();
            }
        }
        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="type">The type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object Deserialize(Stream source, object value, System.Type type)
        {
            return Deserialize(source, value, type, null);
        }
        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="type">The type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        /// <param name="context">Additional information about this serialization operation.</param>
        public object Deserialize(Stream source, object value, System.Type type, SerializationContext context)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            bool autoCreate = PrepareDeserialize(value, ref type);
            ProtoReader reader = null;
            try
            {
                reader = ProtoReader.Create(source, this, context, ProtoReader.TO_EOF);
                if (value != null) reader.SetRootObject(value);
                object obj = DeserializeCore(reader, type, value, autoCreate);
                reader.CheckFullyConsumed();
                return obj;
            }
            finally
            {
                ProtoReader.Recycle(reader);
            }
#endif
        }

        private bool PrepareDeserialize(object value, ref Type type)
        {
            if (type == null)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("type");
                }
                else
                {
                    type = MapType(value.GetType());
                }
            }
            bool autoCreate = true;
#if !NO_GENERICS
            Type underlyingType = Helpers.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
                autoCreate = false;
            }
#endif
            return autoCreate;
        }

        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="type">The type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <param name="length">The number of bytes to consume.</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object Deserialize(Stream source, object value, System.Type type, int length)
        {
            return Deserialize(source, value, type, length, null);
        }
        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="type">The type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <param name="length">The number of bytes to consume (or -1 to read to the end of the stream).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        /// <param name="context">Additional information about this serialization operation.</param>
        public object Deserialize(Stream source, object value, System.Type type, int length, SerializationContext context)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            bool autoCreate = PrepareDeserialize(value, ref type);
            ProtoReader reader = null;
            try
            {
                reader = ProtoReader.Create(source, this, context, length);
                if (value != null) reader.SetRootObject(value);
                object obj = DeserializeCore(reader, type, value, autoCreate);
                reader.CheckFullyConsumed();
                return obj;
            }
            finally
            {
                ProtoReader.Recycle(reader);
            }
#endif
        }
        /// <summary>
        /// Applies a protocol-buffer reader to an existing instance (which may be null).
        /// </summary>
        /// <param name="type">The type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The reader to apply to the instance (cannot be null).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        public object Deserialize(ProtoReader source, object value, System.Type type)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (source == null) throw new ArgumentNullException("source");
            bool autoCreate = PrepareDeserialize(value, ref type);
            if (value != null) source.SetRootObject(value);
            object obj = DeserializeCore(source, type, value, autoCreate);
            source.CheckFullyConsumed();
            return obj;
#endif
        }

#if !FEAT_IKVM
        private object DeserializeCore(ProtoReader reader, Type type, object value, bool noAutoCreate)
        {
            int key = GetKey(ref type);
            if (key >= 0 && !Helpers.IsEnum(type))
            {
                return Deserialize(key, value, reader);
            }
            // this returns true to say we actively found something, but a value is assigned either way (or throws)
            TryDeserializeAuxiliaryType(reader, DataFormat.Default, Serializer.ListItemTag, type, ref value, true, false, noAutoCreate, false);
            return value;
        }
#endif
#if WINRT
        private static readonly System.Reflection.TypeInfo ilist = typeof(IList).GetTypeInfo();
#else
        private static readonly System.Type ilist = typeof(IList);
#endif
        internal static MethodInfo ResolveListAdd(TypeModel model, Type listType, Type itemType, out bool isList)
        {
#if WINRT
            TypeInfo listTypeInfo = listType.GetTypeInfo();
#else
            Type listTypeInfo = listType;
#endif
            isList = model.MapType(ilist).IsAssignableFrom(listTypeInfo);

            Type[] types = { itemType };
            MethodInfo add = Helpers.GetInstanceMethod(listTypeInfo, "Add", types);

#if !NO_GENERICS
            if (add == null)
            {   // fallback: look for ICollection<T>'s Add(typedObject) method

                bool forceList = listTypeInfo.IsInterface &&
                    listTypeInfo == model.MapType(typeof(System.Collections.Generic.IEnumerable<>)).MakeGenericType(types)
#if WINRT
                    .GetTypeInfo()
#endif
                    ;

#if WINRT
                TypeInfo constuctedListType = typeof(System.Collections.Generic.ICollection<>).MakeGenericType(types).GetTypeInfo();
#else
                Type constuctedListType = model.MapType(typeof(System.Collections.Generic.ICollection<>)).MakeGenericType(types);
#endif
                if (forceList || constuctedListType.IsAssignableFrom(listTypeInfo))
                {
                    add = Helpers.GetInstanceMethod(constuctedListType, "Add", types);
                }
            }

            if (add == null)
            {
                
#if WINRT
                foreach (Type tmpType in listTypeInfo.ImplementedInterfaces)
#else
                foreach (Type interfaceType in listTypeInfo.GetInterfaces())
#endif
                {
#if WINRT
                    TypeInfo interfaceType = tmpType.GetTypeInfo();
#endif
                    if (interfaceType.Name == "IProducerConsumerCollection`1" && interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition().FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1")
                    {
                        add = Helpers.GetInstanceMethod(interfaceType, "TryAdd", types);
                        if (add != null) break;
                    }
                }
            }
#endif

            if (add == null)
            {   // fallback: look for a public list.Add(object) method
                types[0] = model.MapType(typeof(object));
                add = Helpers.GetInstanceMethod(listTypeInfo, "Add", types);
            }
            if (add == null && isList)
            {   // fallback: look for IList's Add(object) method
                add = Helpers.GetInstanceMethod(model.MapType(ilist), "Add", types);
            }
            return add;
        }
        internal static Type GetListItemType(TypeModel model, Type listType)
        {
            Helpers.DebugAssert(listType != null);

#if WINRT
            TypeInfo listTypeInfo = listType.GetTypeInfo();
            if (listType == typeof(string) || listType.IsArray
                || !typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(listTypeInfo)) return null;
#else
            if (listType == model.MapType(typeof(string)) || listType.IsArray
                || !model.MapType(typeof(IEnumerable)).IsAssignableFrom(listType)) return null;
#endif
            
            BasicList candidates = new BasicList();
#if WINRT
            foreach (MethodInfo method in listType.GetRuntimeMethods())
#else
            foreach (MethodInfo method in listType.GetMethods())
#endif
            {
                if (method.IsStatic || method.Name != "Add") continue;
                ParameterInfo[] parameters = method.GetParameters();
                Type paramType;
                if (parameters.Length == 1 && !candidates.Contains(paramType = parameters[0].ParameterType))
                {
                    candidates.Add(paramType);
                }
            }

            string name = listType.Name;
            bool isQueueStack = name != null && (name.IndexOf("Queue") >= 0 || name.IndexOf("Stack") >= 0);
#if !NO_GENERICS
            if(!isQueueStack)
            {
                TestEnumerableListPatterns(model, candidates, listType);
#if WINRT
                foreach (Type iType in listTypeInfo.ImplementedInterfaces)
                {
                    TestEnumerableListPatterns(model, candidates, iType);
                }
#else
                foreach (Type iType in listType.GetInterfaces())
                {
                    TestEnumerableListPatterns(model, candidates, iType);
                }
#endif
            }
#endif

#if WINRT
            // more convenient GetProperty overload not supported on all platforms
            foreach (PropertyInfo indexer in listType.GetRuntimeProperties())
            {
                if (indexer.Name != "Item" || candidates.Contains(indexer.PropertyType)) continue;
                ParameterInfo[] args = indexer.GetIndexParameters();
                if (args.Length != 1 || args[0].ParameterType != typeof(int)) continue;
                MethodInfo getter = indexer.GetMethod;
                if (getter == null || getter.IsStatic) continue;
                candidates.Add(indexer.PropertyType);
            }
#else
            // more convenient GetProperty overload not supported on all platforms
            foreach (PropertyInfo indexer in listType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (indexer.Name != "Item" || candidates.Contains(indexer.PropertyType)) continue;
                ParameterInfo[] args = indexer.GetIndexParameters();
                if (args.Length != 1 || args[0].ParameterType != model.MapType(typeof(int))) continue;
                candidates.Add(indexer.PropertyType);
            }
#endif

            switch (candidates.Count)
            {
                case 0:
                    return null;
                case 1:
                    return (Type)candidates[0];
                case 2:
                    if (CheckDictionaryAccessors(model, (Type)candidates[0], (Type)candidates[1])) return (Type)candidates[0];
                    if (CheckDictionaryAccessors(model, (Type)candidates[1], (Type)candidates[0])) return (Type)candidates[1];
                    break;
            }

            return null;
        }

        private static void TestEnumerableListPatterns(TypeModel model, BasicList candidates, Type iType)
        {

#if WINRT
            TypeInfo iTypeInfo = iType.GetTypeInfo();
            if (iTypeInfo.IsGenericType)
            {
                Type typeDef = iTypeInfo.GetGenericTypeDefinition();
                if(typeDef == typeof(System.Collections.Generic.ICollection<>) || typeDef.GetTypeInfo().FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1")
                {
                        
                    Type[] iTypeArgs = iTypeInfo.GenericTypeArguments;
                    if (!candidates.Contains(iTypeArgs[0]))
                    {
                        candidates.Add(iTypeArgs[0]);
                    }
                }
            }
#elif !NO_GENERICS
            if (iType.IsGenericType)
            {
                Type typeDef = iType.GetGenericTypeDefinition();
                if (typeDef == model.MapType(typeof(System.Collections.Generic.IEnumerable<>))
                    || typeDef == model.MapType(typeof(System.Collections.Generic.ICollection<>))
                    || typeDef.FullName == "System.Collections.Concurrent.IProducerConsumerCollection`1")
                {
                    Type[] iTypeArgs = iType.GetGenericArguments();
                    if (!candidates.Contains(iTypeArgs[0]))
                    {
                        candidates.Add(iTypeArgs[0]);
                    }
                }
            }
#endif
        }

        private static bool CheckDictionaryAccessors(TypeModel model, Type pair, Type value)
        {
            
#if NO_GENERICS
            return false;
#elif WINRT
            TypeInfo finalType = pair.GetTypeInfo();
            return finalType.IsGenericType && finalType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.KeyValuePair<,>)
                && finalType.GenericTypeArguments[1] == value;
#else
            return pair.IsGenericType && pair.GetGenericTypeDefinition() == model.MapType(typeof(System.Collections.Generic.KeyValuePair<,>))
                && pair.GetGenericArguments()[1] == value;
#endif
        }

#if !FEAT_IKVM
        private bool TryDeserializeList(TypeModel model, ProtoReader reader, DataFormat format, int tag, Type listType, Type itemType, ref object value)
        {
            bool isList;
            MethodInfo addMethod = TypeModel.ResolveListAdd(model, listType, itemType, out isList);
            if (addMethod == null) throw new NotSupportedException("Unknown list variant: " + listType.FullName);
            bool found = false;
            object nextItem = null;
            IList list = value as IList;
            object[] args = isList ? null : new object[1];
            BasicList arraySurrogate = listType.IsArray ? new BasicList() : null;

            while (TryDeserializeAuxiliaryType(reader, format, tag, itemType, ref nextItem, true, true, true, true))
            {
                found = true;
                if (value == null && arraySurrogate == null)
                {
                    value = CreateListInstance(listType, itemType);
                    list = value as IList;
                }
                if (list != null)
                {
                    list.Add(nextItem);
                }
                else if (arraySurrogate != null)
                {
                    arraySurrogate.Add(nextItem);
                }
                else
                {
                    args[0] = nextItem;
                    addMethod.Invoke(value, args);
                }
                nextItem = null;
            }
            if (arraySurrogate != null)
            {
                Array newArray;
                if (value != null)
                {
                    if (arraySurrogate.Count == 0)
                    {   // we'll stay with what we had, thanks
                    }
                    else
                    { 
                        Array existing = (Array)value;
                        newArray = Array.CreateInstance(itemType, existing.Length + arraySurrogate.Count);
                        Array.Copy(existing, newArray, existing.Length);
                        arraySurrogate.CopyTo(newArray, existing.Length);
                        value = newArray;
                    }
                }
                else
                {
                    newArray = Array.CreateInstance(itemType, arraySurrogate.Count);
                    arraySurrogate.CopyTo(newArray, 0);
                    value = newArray;
                }
            }
            return found;
        }

        private static object CreateListInstance(Type listType, Type itemType)
        {
            Type concreteListType = listType;

            if (listType.IsArray)
            {
                return Array.CreateInstance(itemType, 0);
            }

#if WINRT
            TypeInfo listTypeInfo = listType.GetTypeInfo();
            if (!listTypeInfo.IsClass || listTypeInfo.IsAbstract ||
                Helpers.GetConstructor(listTypeInfo, Helpers.EmptyTypes, true) == null)
#else
            if (!listType.IsClass || listType.IsAbstract ||
                Helpers.GetConstructor(listType, Helpers.EmptyTypes, true) == null)
#endif
            {
                string fullName;
                bool handled = false;
#if WINRT
                if (listTypeInfo.IsInterface &&
#else
                if (listType.IsInterface &&
#endif
                    (fullName = listType.FullName) != null && fullName.IndexOf("Dictionary") >= 0) // have to try to be frugal here...
                {
#if !NO_GENERICS
#if WINRT
                    TypeInfo finalType = listType.GetTypeInfo();
                    if (finalType.IsGenericType && finalType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
                    {
                        Type[] genericTypes = listType.GenericTypeArguments;
                        concreteListType = typeof(System.Collections.Generic.Dictionary<,>).MakeGenericType(genericTypes);
                        handled = true;
                    }
#else
                    if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IDictionary<,>))
                    {
                        Type[] genericTypes = listType.GetGenericArguments();
                        concreteListType = typeof(System.Collections.Generic.Dictionary<,>).MakeGenericType(genericTypes);
                        handled = true;
                    }
#endif
#endif
#if !SILVERLIGHT && !WINRT && !PORTABLE
                    if (!handled && listType == typeof(IDictionary))
                    {
                        concreteListType = typeof(Hashtable);
                        handled = true;
                    }
#endif
                }
#if !NO_GENERICS
                if (!handled)
                {
                    concreteListType = typeof(System.Collections.Generic.List<>).MakeGenericType(itemType);
                    handled = true;
                }
#endif

#if !SILVERLIGHT && !WINRT && !PORTABLE
                if (!handled)
                {
                    concreteListType = typeof(ArrayList);
                    handled = true;
                }
#endif
            }
            return Activator.CreateInstance(concreteListType);
        }

        /// <summary>
        /// This is the more "complete" version of Deserialize, which handles single instances of mapped types.
        /// The value is read as a complete field, including field-header and (for sub-objects) a
        /// length-prefix..kmc  
        /// 
        /// In addition to that, this provides support for:
        ///  - basic values; individual int / string / Guid / etc
        ///  - IList sets of any type handled by TryDeserializeAuxiliaryType
        /// </summary>
        internal bool TryDeserializeAuxiliaryType(ProtoReader reader, DataFormat format, int tag, Type type, ref object value, bool skipOtherFields, bool asListItem, bool autoCreate, bool insideList)
        {
            if (type == null) throw new ArgumentNullException("type");
            Type itemType = null;
            ProtoTypeCode typecode = Helpers.GetTypeCode(type);
            int modelKey;
            WireType wiretype = GetWireType(typecode, format, ref type, out modelKey);

            bool found = false;
            if (wiretype == WireType.None)
            {
                itemType = GetListItemType(this, type);
                if (itemType == null && type.IsArray && type.GetArrayRank() == 1 && type != typeof(byte[]))
                {
                    itemType = type.GetElementType();
                }
                if (itemType != null)
                {
                    if (insideList) throw TypeModel.CreateNestedListsNotSupported();
                    found = TryDeserializeList(this, reader, format, tag, type, itemType, ref value);
                    if (!found && autoCreate)
                    {
                        value = CreateListInstance(type, itemType);
                    }
                    return found;
                }

                // otherwise, not a happy bunny...
                ThrowUnexpectedType(type);
            }
            
            // to treat correctly, should read all values

            while (true)
            {
                // for convenience (re complex exit conditions), additional exit test here:
                // if we've got the value, are only looking for one, and we aren't a list - then exit
                if (found && asListItem) break;


                // read the next item
                int fieldNumber = reader.ReadFieldHeader();
                if (fieldNumber <= 0) break;
                if (fieldNumber != tag)
                {
                    if (skipOtherFields)
                    {
                        reader.SkipField();
                        continue;
                    }
                    throw ProtoReader.AddErrorData(new InvalidOperationException(
                        "Expected field " + tag.ToString() + ", but found " + fieldNumber.ToString()), reader);
                }
                found = true;
                reader.Hint(wiretype); // handle signed data etc

                if (modelKey >= 0)
                {
                    switch (wiretype)
                    {
                        case WireType.String:
                        case WireType.StartGroup:
                            SubItemToken token = ProtoReader.StartSubItem(reader);
                            value = Deserialize(modelKey, value, reader);
                            ProtoReader.EndSubItem(token, reader);
                            continue;
                        default:
                            value = Deserialize(modelKey, value, reader);
                            continue;
                    }
                }
                switch (typecode)
                {
                    case ProtoTypeCode.Int16: value = reader.ReadInt16(); continue;
                    case ProtoTypeCode.Int32: value = reader.ReadInt32(); continue;
                    case ProtoTypeCode.Int64: value = reader.ReadInt64(); continue;
                    case ProtoTypeCode.UInt16: value = reader.ReadUInt16(); continue;
                    case ProtoTypeCode.UInt32: value = reader.ReadUInt32(); continue;
                    case ProtoTypeCode.UInt64: value = reader.ReadUInt64(); continue;
                    case ProtoTypeCode.Boolean: value = reader.ReadBoolean(); continue;
                    case ProtoTypeCode.SByte: value = reader.ReadSByte(); continue;
                    case ProtoTypeCode.Byte: value = reader.ReadByte(); continue;
                    case ProtoTypeCode.Char: value = (char)reader.ReadUInt16(); continue;
                    case ProtoTypeCode.Double: value = reader.ReadDouble(); continue;
                    case ProtoTypeCode.Single: value = reader.ReadSingle(); continue;
                    case ProtoTypeCode.DateTime: value = BclHelpers.ReadDateTime(reader); continue;
                    case ProtoTypeCode.Decimal: value = BclHelpers.ReadDecimal(reader); continue;
                    case ProtoTypeCode.String: value = reader.ReadString(); continue;
                    case ProtoTypeCode.ByteArray: value = ProtoReader.AppendBytes((byte[])value, reader); continue;
                    case ProtoTypeCode.TimeSpan: value = BclHelpers.ReadTimeSpan(reader); continue;
                    case ProtoTypeCode.Guid: value = BclHelpers.ReadGuid(reader); continue;
                    case ProtoTypeCode.Uri: value = new Uri(reader.ReadString()); continue; 
                }

            }
            if (!found && !asListItem && autoCreate)
            {
                if (type != typeof(string))
                {
                    value = Activator.CreateInstance(type);
                }
            }
            return found;
        }
#endif

#if !NO_RUNTIME
        /// <summary>
        /// Creates a new runtime model, to which the caller
        /// can add support for a range of types. A model
        /// can be used "as is", or can be compiled for
        /// optimal performance.
        /// </summary>
        public static RuntimeTypeModel Create()
        {
            return new RuntimeTypeModel(false);
        }
#endif

        /// <summary>
        /// Applies common proxy scenarios, resolving the actual type to consider
        /// </summary>
        protected internal static Type ResolveProxies(Type type)
        {
            if (type == null) return null;
#if !NO_GENERICS            
            if (type.IsGenericParameter) return null;
            // Nullable<T>
            Type tmp =  Helpers.GetUnderlyingType(type);
            if (tmp != null) return tmp;
#endif

#if !(WINRT || CF)
            // EF POCO
            string fullName = type.FullName;
            if (fullName != null && fullName.StartsWith("System.Data.Entity.DynamicProxies.")) return type.BaseType;

            // NHibernate
            Type[] interfaces = type.GetInterfaces();
            for(int i = 0 ; i < interfaces.Length ; i++)
            {
                switch(interfaces[i].FullName)
                {
                    case "NHibernate.Proxy.INHibernateProxy":
                    case "NHibernate.Proxy.DynamicProxy.IProxy":
                    case "NHibernate.Intercept.IFieldInterceptorAccessor":
                        return type.BaseType;
                }
            }
#endif
            return null;
        }
        /// <summary>
        /// Indicates whether the supplied type is explicitly modelled by the model
        /// </summary>
        public bool IsDefined(Type type)
        {
            return GetKey(ref type) >= 0;
        }
        /// <summary>
        /// Provides the key that represents a given type in the current model.
        /// The type is also normalized for proxies at the same time.
        /// </summary>
        protected internal int GetKey(ref Type type)
        {
            if (type == null) return -1;
            int key = GetKeyImpl(type);
            if (key < 0)
            {
                Type normalized = ResolveProxies(type);
                if (normalized != null) {
                    type = normalized; // hence ref
                    key = GetKeyImpl(type);
                }
            }
            return key;
        }

        /// <summary>
        /// Provides the key that represents a given type in the current model.
        /// </summary>
        protected abstract int GetKeyImpl(Type type);
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream.
        /// </summary>
        /// <param name="key">Represents the type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be serialized (cannot be null).</param>
        /// <param name="dest">The destination stream to write to.</param>
        protected internal abstract void Serialize(int key, object value, ProtoWriter dest);
        /// <summary>
        /// Applies a protocol-buffer stream to an existing instance (which may be null).
        /// </summary>
        /// <param name="key">Represents the type (including inheritance) to consider.</param>
        /// <param name="value">The existing instance to be modified (can be null).</param>
        /// <param name="source">The binary stream to apply to the instance (cannot be null).</param>
        /// <returns>The updated instance; this may be different to the instance argument if
        /// either the original instance was null, or the stream defines a known sub-type of the
        /// original instance.</returns>
        protected internal abstract object Deserialize(int key, object value, ProtoReader source);
        
        //internal ProtoSerializer Create(IProtoSerializer head)
        //{
        //    return new RuntimeSerializer(head, this);
        //}
        //internal ProtoSerializer Compile

        /// <summary>
        /// Indicates the type of callback to be used
        /// </summary>
        protected internal enum CallbackType
        {
            /// <summary>
            /// Invoked before an object is serialized
            /// </summary>
            BeforeSerialize,
            /// <summary>
            /// Invoked after an object is serialized
            /// </summary>
            AfterSerialize,
            /// <summary>
            /// Invoked before an object is deserialized (or when a new instance is created)
            /// </summary>            
            BeforeDeserialize,
            /// <summary>
            /// Invoked after an object is deserialized
            /// </summary>
            AfterDeserialize
        }
        /// <summary>
        /// Create a deep clone of the supplied instance; any sub-items are also cloned.
        /// </summary>
        public object DeepClone(object value)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (value == null) return null;
            Type type = value.GetType();
            int key = GetKey(ref type);

            if (key >= 0 && !Helpers.IsEnum(type))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using(ProtoWriter writer = new ProtoWriter(ms, this, null))
                    {
                        writer.SetRootObject(value);
                        Serialize(key, value, writer);
                        writer.Close();
                    }
                    ms.Position = 0;
                    ProtoReader reader = null;
                    try
                    {
                        reader = ProtoReader.Create(ms, this, null, ProtoReader.TO_EOF);
                        return Deserialize(key, null, reader);
                    }
                    finally
                    {
                        ProtoReader.Recycle(reader);
                    }
                }
            }
            int modelKey;
            if (type == typeof(byte[])) {
                byte[] orig = (byte[])value, clone = new byte[orig.Length];
                Helpers.BlockCopy(orig, 0, clone, 0, orig.Length);
                return clone;
            }
            else if (GetWireType(Helpers.GetTypeCode(type), DataFormat.Default, ref type, out modelKey) != WireType.None && modelKey < 0)
            {   // immutable; just return the original value
                return value;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                using (ProtoWriter writer = new ProtoWriter(ms, this, null))
                {
                    if (!TrySerializeAuxiliaryType(writer, type, DataFormat.Default, Serializer.ListItemTag, value, false)) ThrowUnexpectedType(type);
                    writer.Close();
                }
                ms.Position = 0;
                ProtoReader reader = null;
                try
                {
                    reader = ProtoReader.Create(ms, this, null, ProtoReader.TO_EOF);
                    value = null; // start from scratch!
                    TryDeserializeAuxiliaryType(reader, DataFormat.Default, Serializer.ListItemTag, type, ref value, true, false, true, false);
                    return value;
                }
                finally
                {
                    ProtoReader.Recycle(reader);
                }
            }
#endif       

        }

        /// <summary>
        /// Indicates that while an inheritance tree exists, the exact type encountered was not
        /// specified in that hierarchy and cannot be processed.
        /// </summary>
        protected internal static void ThrowUnexpectedSubtype(Type expected, Type actual)
        {
            if (expected != TypeModel.ResolveProxies(actual))
            {
                throw new InvalidOperationException("Unexpected sub-type: " + actual.FullName);
            }
        }

        /// <summary>
        /// Indicates that the given type was not expected, and cannot be processed.
        /// </summary>
        protected internal static void ThrowUnexpectedType(Type type)
        {
            string fullName = type == null ? "(unknown)" : type.FullName;
#if !NO_GENERICS && !WINRT
            if (type != null)
            {
                Type baseType = type.BaseType;
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "GeneratedMessage`2")
                {
                    throw new InvalidOperationException(
                        "Are you mixing protobuf-net and protobuf-csharp-port? See http://stackoverflow.com/q/11564914; type: " + fullName);
                }
            }
#endif
            throw new InvalidOperationException("Type is not expected, and no contract can be inferred: " + fullName);
        }
        internal static Exception CreateNestedListsNotSupported()
        {
            return new NotSupportedException("Nested or jagged lists and arrays are not supported");
        }
        /// <summary>
        /// Indicates that the given type cannot be constructed; it may still be possible to 
        /// deserialize into existing instances.
        /// </summary>
        public static void ThrowCannotCreateInstance(Type type)
        {
            throw new ProtoException("No parameterless constructor found for " + (type == null ? "(null)" : type.Name));
        }

        internal static string SerializeType(TypeModel model, System.Type type)
        {
            if (model != null)
            {
                TypeFormatEventHandler handler = model.DynamicTypeFormatting;
                if (handler != null)
                {
                    TypeFormatEventArgs args = new TypeFormatEventArgs(type);
                    handler(model, args);
                    if (!Helpers.IsNullOrEmpty(args.FormattedName)) return args.FormattedName;
                }
            }
            return type.AssemblyQualifiedName;
        }

        internal static System.Type DeserializeType(TypeModel model, string value)
        {
            
            if (model != null)
            {
                TypeFormatEventHandler handler = model.DynamicTypeFormatting;
                if (handler != null)
                {
                    TypeFormatEventArgs args = new TypeFormatEventArgs(value);
                    handler(model, args);
                    if (args.Type != null) return args.Type;
                }
            }
            return System.Type.GetType(value);
        }

        /// <summary>
        /// Returns true if the type supplied is either a recognised contract type,
        /// or a *list* of a recognised contract type. 
        /// </summary>
        /// <remarks>Note that primitives always return false, even though the engine
        /// will, if forced, try to serialize such</remarks>
        /// <returns>True if this type is recognised as a serializable entity, else false</returns>
        public bool CanSerializeContractType(Type type)
        {
            return CanSerialize(type, false, true, true);
        }

        /// <summary>
        /// Returns true if the type supplied is a basic type with inbuilt handling,
        /// a recognised contract type, or a *list* of a basic / contract type. 
        /// </summary>
        public bool CanSerialize(Type type)
        {
            return CanSerialize(type, true, true, true);
        }

        /// <summary>
        /// Returns true if the type supplied is a basic type with inbuilt handling,
        /// or a *list* of a basic type with inbuilt handling
        /// </summary>
        public bool CanSerializeBasicType(Type type)
        {
            return CanSerialize(type, true, false, true);
        }
        private bool CanSerialize(Type type, bool allowBasic, bool allowContract, bool allowLists)
        {
            if (type == null) throw new ArgumentNullException("type");
            Type tmp = Helpers.GetUnderlyingType(type);
            if (tmp != null) type = tmp;

            // is it a basic type?
            ProtoTypeCode typeCode = Helpers.GetTypeCode(type);
            switch(typeCode)
            {
                case ProtoTypeCode.Empty:
                case ProtoTypeCode.Unknown:
                    break;
                default:
                    return allowBasic; // well-known basic type
            }
            int modelKey = GetKey(ref type);
            if (modelKey >= 0) return allowContract; // known contract type

            // is it a list?
            if (allowLists)
            {
                Type itemType = null;
                if (type.IsArray)
                {   // note we don't need to exclude byte[], as that is handled by GetTypeCode already
                    if (type.GetArrayRank() == 1) itemType = type.GetElementType();
                }
                else
                {
                    itemType = GetListItemType(this, type);
                }
                if (itemType != null) return CanSerialize(itemType, allowBasic, allowContract, false);
            }
            return false;
        }

        /// <summary>
        /// Suggest a .proto definition for the given type
        /// </summary>
        /// <param name="type">The type to generate a .proto definition for, or <c>null</c> to generate a .proto that represents the entire model</param>
        /// <returns>The .proto definition as a string</returns>
        public virtual string GetSchema(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Used to provide custom services for writing and parsing type names when using dynamic types. Both parsing and formatting
        /// are provided on a single API as it is essential that both are mapped identically at all times.
        /// </summary>
        public event TypeFormatEventHandler DynamicTypeFormatting;

#if PLAT_BINARYFORMATTER && !(WINRT || PHONE8)
        /// <summary>
        /// Creates a new IFormatter that uses protocol-buffer [de]serialization.
        /// </summary>
        /// <returns>A new IFormatter to be used during [de]serialization.</returns>
        /// <param name="type">The type of object to be [de]deserialized by the formatter.</param>
        public System.Runtime.Serialization.IFormatter CreateFormatter(Type type)
        {
            return new Formatter(this, type);
        }

        internal sealed class Formatter : System.Runtime.Serialization.IFormatter
        {
            private readonly TypeModel model;
            private readonly Type type;
            internal Formatter(TypeModel model, Type type)
            {
                if (model == null) throw new ArgumentNullException("model");
                if (type == null) throw new ArgumentNullException("type");
                this.model = model;
                this.type = type;
            }
            private System.Runtime.Serialization.SerializationBinder binder;
            public System.Runtime.Serialization.SerializationBinder Binder
            {
                get { return binder; }
                set { binder = value; }
            }

            private System.Runtime.Serialization.StreamingContext context;
            public System.Runtime.Serialization.StreamingContext Context
            {
                get { return context; }
                set { context = value; }
            }

            public object Deserialize(Stream source)
            {
#if FEAT_IKVM
                throw new NotSupportedException();
#else
                return model.Deserialize(source, null, type, -1, Context);
#endif
            }

            public void Serialize(Stream destination, object graph)
            {
                model.Serialize(destination, graph, Context);
            }

            private System.Runtime.Serialization.ISurrogateSelector surrogateSelector;
            public System.Runtime.Serialization.ISurrogateSelector SurrogateSelector
            {
                get { return surrogateSelector; }
                set { surrogateSelector = value; }
            }
        }
#endif

#if DEBUG // this is used by some unit tests only, to ensure no buffering when buffering is disabled
        private bool forwardsOnly;
        /// <summary>
        /// If true, buffering of nested objects is disabled
        /// </summary>
        public bool ForwardsOnly
        {
            get { return forwardsOnly; }
            set { forwardsOnly = value; }
        }
#endif

        internal virtual Type GetType(string fullName, Assembly context)
        {
#if FEAT_IKVM
            throw new NotImplementedException();
#else
            return ResolveKnownType(fullName, this, context);
#endif
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal static Type ResolveKnownType(string name, TypeModel model, Assembly assembly)
        {
            if (Helpers.IsNullOrEmpty(name)) return null;
            try
            {
#if FEAT_IKVM
                // looks like a NullReferenceException, but this should call into RuntimeTypeModel's version
                Type type = model == null ? null : model.GetType(name, assembly);
#else
                Type type = Type.GetType(name);
#endif
                if (type != null) return type;
            }
            catch { }
            try
            {
                int i = name.IndexOf(',');
                string fullName = (i > 0 ? name.Substring(0, i) : name).Trim();
#if !(WINRT || FEAT_IKVM)
                if (assembly == null) assembly = Assembly.GetCallingAssembly();
#endif
                Type type = assembly == null ? null : assembly.GetType(fullName);
                if (type != null) return type;
            }
            catch { }
            return null;
        }

    }

}

