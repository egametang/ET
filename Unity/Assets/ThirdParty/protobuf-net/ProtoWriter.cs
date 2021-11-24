using System;

using System.IO;
using System.Text;
using ProtoBuf.Meta;
#if MF
using OverflowException = System.ApplicationException;
#endif

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
#endif

namespace ProtoBuf
{
    /// <summary>
    /// Represents an output stream for writing protobuf data.
    /// 
    /// Why is the API backwards (static methods with writer arguments)?
    /// See: http://marcgravell.blogspot.com/2010/03/last-will-be-first-and-first-will-be.html
    /// </summary>
    public sealed class ProtoWriter : IDisposable
    {
        private Stream dest;
        TypeModel model;
        /// <summary>
        /// Write an encapsulated sub-object, using the supplied unique key (reprasenting a type).
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="key">The key that uniquely identifies the type within the model.</param>
        /// <param name="writer">The destination.</param>
        public static void WriteObject(object value, int key, ProtoWriter writer)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (writer == null) throw new ArgumentNullException("writer");
            if (writer.model == null)
            {
                throw new InvalidOperationException("Cannot serialize sub-objects unless a model is provided");
            }

            SubItemToken token = StartSubItem(value, writer);
            if (key >= 0)
            {
                writer.model.Serialize(key, value, writer);
            }
            else if (writer.model != null && writer.model.TrySerializeAuxiliaryType(writer, value.GetType(), DataFormat.Default, Serializer.ListItemTag, value, false, null))
            {
                // all ok
            }
            else
            {
                TypeModel.ThrowUnexpectedType(value.GetType());
            }
            EndSubItem(token, writer);
#endif 
        }
        /// <summary>
        /// Write an encapsulated sub-object, using the supplied unique key (reprasenting a type) - but the
        /// caller is asserting that this relationship is non-recursive; no recursion check will be
        /// performed.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="key">The key that uniquely identifies the type within the model.</param>
        /// <param name="writer">The destination.</param>
        public static void WriteRecursionSafeObject(object value, int key, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (writer.model == null)
            {
                throw new InvalidOperationException("Cannot serialize sub-objects unless a model is provided");
            }
            SubItemToken token = StartSubItem(null, writer);
            writer.model.Serialize(key, value, writer);
            EndSubItem(token, writer);
        }
        internal static void WriteObject(object value, int key, ProtoWriter writer, PrefixStyle style, int fieldNumber)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            if (writer.model == null)
            {
                throw new InvalidOperationException("Cannot serialize sub-objects unless a model is provided");
            }
            if (writer.wireType != WireType.None) throw ProtoWriter.CreateException(writer);

            switch (style)
            {
                case PrefixStyle.Base128:
                    writer.wireType = WireType.String;
                    writer.fieldNumber = fieldNumber;
                    if (fieldNumber > 0) WriteHeaderCore(fieldNumber, WireType.String, writer);
                    break;
                case PrefixStyle.Fixed32:
                case PrefixStyle.Fixed32BigEndian:
                    writer.fieldNumber = 0;
                    writer.wireType = WireType.Fixed32;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
            SubItemToken token = StartSubItem(value, writer, true);
            if (key < 0)
            {
                if (!writer.model.TrySerializeAuxiliaryType(writer, value.GetType(), DataFormat.Default, Serializer.ListItemTag, value, false, null))
                {
                    TypeModel.ThrowUnexpectedType(value.GetType());
                }
            }
            else
            {
                writer.model.Serialize(key, value, writer);
            }
            EndSubItem(token, writer, style);
#endif       
        }

        internal int GetTypeKey(ref Type type)
        {
            return model.GetKey(ref type);
        }
        
        private readonly NetObjectCache netCache = new NetObjectCache();
        internal NetObjectCache NetCache
        {
            get { return netCache;}
        }

        private int fieldNumber, flushLock;
        WireType wireType;
        internal WireType WireType { get { return wireType; } }
        /// <summary>
        /// Writes a field-header, indicating the format of the next data we plan to write.
        /// </summary>
        public static void WriteFieldHeader(int fieldNumber, WireType wireType, ProtoWriter writer) {
            if (writer == null) throw new ArgumentNullException("writer");
            if (writer.wireType != WireType.None) throw new InvalidOperationException("Cannot write a " + wireType.ToString()
                + " header until the " + writer.wireType.ToString() + " data has been written");
            if(fieldNumber < 0) throw new ArgumentOutOfRangeException("fieldNumber");
#if DEBUG
            switch (wireType)
            {   // validate requested header-type
                case WireType.Fixed32:
                case WireType.Fixed64:
                case WireType.String:
                case WireType.StartGroup:
                case WireType.SignedVariant:
                case WireType.Variant:
                    break; // fine
                case WireType.None:
                case WireType.EndGroup:
                default:
                    throw new ArgumentException("Invalid wire-type: " + wireType.ToString(), "wireType");                
            }
#endif
            if (writer.packedFieldNumber == 0) {
                writer.fieldNumber = fieldNumber;
                writer.wireType = wireType;
                WriteHeaderCore(fieldNumber, wireType, writer);
            }
            else if (writer.packedFieldNumber == fieldNumber)
            { // we'll set things up, but note we *don't* actually write the header here
                switch (wireType)
                {
                    case WireType.Fixed32:
                    case WireType.Fixed64:
                    case WireType.Variant:
                    case WireType.SignedVariant:
                        break; // fine
                    default:
                        throw new InvalidOperationException("Wire-type cannot be encoded as packed: " + wireType.ToString());
                }
                writer.fieldNumber = fieldNumber;
                writer.wireType = wireType;
            }
            else
            {
                throw new InvalidOperationException("Field mismatch during packed encoding; expected " + writer.packedFieldNumber.ToString() + " but received " + fieldNumber.ToString());
            }
        }
        internal static void WriteHeaderCore(int fieldNumber, WireType wireType, ProtoWriter writer)
        {
            uint header = (((uint)fieldNumber) << 3)
                | (((uint)wireType) & 7);
            WriteUInt32Variant(header, writer);
        }

        /// <summary>
        /// Writes a byte-array to the stream; supported wire-types: String
        /// </summary>
        public static void WriteBytes(byte[] data, ProtoWriter writer)
        {
            if (data == null) throw new ArgumentNullException("data");
            ProtoWriter.WriteBytes(data, 0, data.Length, writer);
        }
        /// <summary>
        /// Writes a byte-array to the stream; supported wire-types: String
        /// </summary>
        public static void WriteBytes(byte[] data, int offset, int length, ProtoWriter writer)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed32:
                    if (length != 4) throw new ArgumentException("length");
                    goto CopyFixedLength;  // ugly but effective
                case WireType.Fixed64:
                    if (length != 8) throw new ArgumentException("length");
                    goto CopyFixedLength;  // ugly but effective
                case WireType.String:
                    WriteUInt32Variant((uint)length, writer);
                    writer.wireType = WireType.None;
                    if (length == 0) return;
                    if (writer.flushLock != 0 || length <= writer.ioBuffer.Length) // write to the buffer
                    {
                        goto CopyFixedLength; // ugly but effective
                    }
                    // writing data that is bigger than the buffer (and the buffer
                    // isn't currently locked due to a sub-object needing the size backfilled)
                    Flush(writer); // commit any existing data from the buffer
                    // now just write directly to the underlying stream
                    writer.dest.Write(data, offset, length);
                    writer.position64 += length; // since we've flushed offset etc is 0, and remains
                                        // zero since we're writing directly to the stream
                    return;
            }
            throw CreateException(writer);
        CopyFixedLength: // no point duplicating this lots of times, and don't really want another stackframe
            DemandSpace(length, writer);
            Helpers.BlockCopy(data, offset, writer.ioBuffer, writer.ioIndex, length);
            IncrementedAndReset(length, writer);
        }
        private static void CopyRawFromStream(Stream source, ProtoWriter writer)
        {
            byte[] buffer = writer.ioBuffer;
            int space = buffer.Length - writer.ioIndex, bytesRead = 1; // 1 here to spoof case where already full
            
            // try filling the buffer first   
            while (space > 0 && (bytesRead = source.Read(buffer, writer.ioIndex, space)) > 0)
            {
                writer.ioIndex += bytesRead;
                writer.position64 += bytesRead;
                space -= bytesRead;                
            }
            if (bytesRead <= 0) return; // all done using just the buffer; stream exhausted

            // at this point the stream still has data, but buffer is full; 
            if (writer.flushLock == 0)
            {
                // flush the buffer and write to the underlying stream instead
                Flush(writer);
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.dest.Write(buffer, 0, bytesRead);
                    writer.position64 += bytesRead;
                }
            }
            else
            {
                do
                {
                    // need more space; resize (double) as necessary,
                    // requesting a reasonable minimum chunk each time
                    // (128 is the minimum; there may actually be much
                    // more space than this in the buffer)
                    DemandSpace(128, writer);
                    if((bytesRead = source.Read(writer.ioBuffer, writer.ioIndex,
                        writer.ioBuffer.Length - writer.ioIndex)) <= 0) break;
                    writer.position64 += bytesRead;
                    writer.ioIndex += bytesRead;
                } while (true);
            }

        }
        private static void IncrementedAndReset(int length, ProtoWriter writer)
        {
            Helpers.DebugAssert(length >= 0);
            writer.ioIndex += length;
            writer.position64 += length;
            writer.wireType = WireType.None;
        }
        int depth = 0;
        const int RecursionCheckDepth = 25;
        /// <summary>
        /// Indicates the start of a nested record.
        /// </summary>
        /// <param name="instance">The instance to write.</param>
        /// <param name="writer">The destination.</param>
        /// <returns>A token representing the state of the stream; this token is given to EndSubItem.</returns>
        public static SubItemToken StartSubItem(object instance, ProtoWriter writer)
        {
            return StartSubItem(instance, writer, false);
        }

        MutableList recursionStack;
        private void CheckRecursionStackAndPush(object instance)
        {
            int hitLevel;
            if (recursionStack == null) { recursionStack = new MutableList(); }
            else if (instance != null && (hitLevel = recursionStack.IndexOfReference(instance)) >= 0)
            {
#if DEBUG
                Helpers.DebugWriteLine("Stack:");
                foreach(object obj in recursionStack)
                {
                    Helpers.DebugWriteLine(obj == null ? "<null>" : obj.ToString());
                }
                Helpers.DebugWriteLine(instance == null ? "<null>" : instance.ToString());
#endif
                throw new ProtoException("Possible recursion detected (offset: " + (recursionStack.Count - hitLevel).ToString() + " level(s)): " + instance.ToString());
            }
            recursionStack.Add(instance);
        }
        private void PopRecursionStack() { recursionStack.RemoveLast(); }

        private static SubItemToken StartSubItem(object instance, ProtoWriter writer, bool allowFixed)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (++writer.depth > RecursionCheckDepth)
            {
                writer.CheckRecursionStackAndPush(instance);
            }
            if(writer.packedFieldNumber != 0) throw new InvalidOperationException("Cannot begin a sub-item while performing packed encoding");
            switch (writer.wireType)
            {
                case WireType.StartGroup:
                    writer.wireType = WireType.None;
                    return new SubItemToken((long)(-writer.fieldNumber));
                case WireType.String:
#if DEBUG
                    if(writer.model != null && writer.model.ForwardsOnly)
                    {
                        throw new ProtoException("Should not be buffering data: " + instance ?? "(null)");
                    }
#endif
                    writer.wireType = WireType.None;
                    DemandSpace(32, writer); // make some space in anticipation...
                    writer.flushLock++;
                    writer.position64++;
                    return new SubItemToken((long)(writer.ioIndex++)); // leave 1 space (optimistic) for length
                case WireType.Fixed32:
                    {
                        if (!allowFixed) throw CreateException(writer);
                        DemandSpace(32, writer); // make some space in anticipation...
                        writer.flushLock++;
                        SubItemToken token = new SubItemToken((long)writer.ioIndex);
                        ProtoWriter.IncrementedAndReset(4, writer); // leave 4 space (rigid) for length
                        return token;
                    }
                default:
                    throw CreateException(writer);
            }
        }

        /// <summary>
        /// Indicates the end of a nested record.
        /// </summary>
        /// <param name="token">The token obtained from StartubItem.</param>
        /// <param name="writer">The destination.</param>
        public static void EndSubItem(SubItemToken token, ProtoWriter writer)
        {
            EndSubItem(token, writer, PrefixStyle.Base128);
        }
        private static void EndSubItem(SubItemToken token, ProtoWriter writer, PrefixStyle style)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (writer.wireType != WireType.None) { throw CreateException(writer); }
            int value = (int)token.value64;
            if (writer.depth <= 0) throw CreateException(writer);
            if (writer.depth-- > RecursionCheckDepth)
            {
                writer.PopRecursionStack();
            }
            writer.packedFieldNumber = 0; // ending the sub-item always wipes packed encoding
            if (value < 0)
            {   // group - very simple append
                WriteHeaderCore(-value, WireType.EndGroup, writer);
                writer.wireType = WireType.None;
                return;
            }

            // so we're backfilling the length into an existing sequence
            int len;
            switch(style)
            {
                case PrefixStyle.Fixed32:
                    len = (int)((writer.ioIndex - value) - 4);
                    ProtoWriter.WriteInt32ToBuffer(len, writer.ioBuffer, value);
                    break;
                case PrefixStyle.Fixed32BigEndian:
                    len = (int)((writer.ioIndex - value) - 4);
                    byte[] buffer = writer.ioBuffer;
                    ProtoWriter.WriteInt32ToBuffer(len, buffer, value);
                    // and swap the byte order
                    byte b = buffer[value];
                    buffer[value] = buffer[value + 3];
                    buffer[value + 3] = b;
                    b = buffer[value + 1];
                    buffer[value + 1] = buffer[value + 2];
                    buffer[value + 2] = b;
                    break;
                case PrefixStyle.Base128:
                    // string - complicated because we only reserved one byte;
                    // if the prefix turns out to need more than this then
                    // we need to shuffle the existing data
                    len = (int)((writer.ioIndex - value) - 1);
                    int offset = 0;
                    uint tmp = (uint)len;
                    while ((tmp >>= 7) != 0) offset++;
                    if (offset == 0)
                    {
                        writer.ioBuffer[value] = (byte)(len & 0x7F);
                    }
                    else
                    {
                        DemandSpace(offset, writer);
                        byte[] blob = writer.ioBuffer;
                        Helpers.BlockCopy(blob, value + 1, blob, value + 1 + offset, len);
                        tmp = (uint)len;
                        do
                        {
                            blob[value++] = (byte)((tmp & 0x7F) | 0x80);
                        } while ((tmp >>= 7) != 0);
                        blob[value - 1] = (byte)(blob[value - 1] & ~0x80);
                        writer.position64 += offset;
                        writer.ioIndex += offset;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
            // and this object is no longer a blockage - also flush if sensible
            const int ADVISORY_FLUSH_SIZE = 1024;
            if (--writer.flushLock == 0 && writer.ioIndex >= ADVISORY_FLUSH_SIZE)
            {
                ProtoWriter.Flush(writer);
            }
            
        }

        /// <summary>
        /// Creates a new writer against a stream
        /// </summary>
        /// <param name="dest">The destination stream</param>
        /// <param name="model">The model to use for serialization; this can be null, but this will impair the ability to serialize sub-objects</param>
        /// <param name="context">Additional context about this serialization operation</param>
        public ProtoWriter(Stream dest, TypeModel model, SerializationContext context)
        {
            if (dest == null) throw new ArgumentNullException("dest");
            if (!dest.CanWrite) throw new ArgumentException("Cannot write to stream", "dest");
            //if (model == null) throw new ArgumentNullException("model");
            this.dest = dest;
            this.ioBuffer = BufferPool.GetBuffer();
            this.model = model;
            this.wireType = WireType.None;
            if (context == null) { context = SerializationContext.Default; }
            else { context.Freeze(); }
            this.context = context;
            
        }

        private readonly SerializationContext context;
        /// <summary>
        /// Addition information about this serialization operation.
        /// </summary>
        public SerializationContext Context { get { return context; } }
        void IDisposable.Dispose()
        {
            Dispose();
        }
        private void Dispose()
        {   // importantly, this does **not** own the stream, and does not dispose it
            if (dest != null)
            {
                Flush(this);
                dest = null;
            }
            model = null;
            BufferPool.ReleaseBufferToPool(ref ioBuffer);
        }

        private byte[] ioBuffer;
        private int ioIndex;
        // note that this is used by some of the unit tests and should not be removed
        internal static long GetLongPosition(ProtoWriter writer) { return writer.position64; }
        internal static int GetPosition(ProtoWriter writer) { return checked((int) writer.position64); }
        private long position64;
        private static void DemandSpace(int required, ProtoWriter writer)
        {
            // check for enough space
            if ((writer.ioBuffer.Length - writer.ioIndex) < required)
            {
                if (writer.flushLock == 0)
                {
                    Flush(writer); // try emptying the buffer
                    if ((writer.ioBuffer.Length - writer.ioIndex) >= required) return;
                }
                // either can't empty the buffer, or that didn't help; need more space
                BufferPool.ResizeAndFlushLeft(ref writer.ioBuffer, required + writer.ioIndex, 0, writer.ioIndex);
            }
        }
        /// <summary>
        /// Flushes data to the underlying stream, and releases any resources. The underlying stream is *not* disposed
        /// by this operation.
        /// </summary>
        public void Close()
        {
            if (depth != 0 || flushLock != 0) throw new InvalidOperationException("Unable to close stream in an incomplete state");
            Dispose();
        }

        internal void CheckDepthFlushlock()
        {
            if (depth != 0 || flushLock != 0) throw new InvalidOperationException("The writer is in an incomplete state");
        }

        /// <summary>
        /// Get the TypeModel associated with this writer
        /// </summary>
        public TypeModel Model { get { return model; } }

        /// <summary>
        /// Writes any buffered data (if possible) to the underlying stream.
        /// </summary>
        /// <param name="writer">The writer to flush</param>
        /// <remarks>It is not always possible to fully flush, since some sequences
        /// may require values to be back-filled into the byte-stream.</remarks>
        internal static void Flush(ProtoWriter writer)
        {
            if (writer.flushLock == 0 && writer.ioIndex != 0)
            {
                writer.dest.Write(writer.ioBuffer, 0, writer.ioIndex);
                writer.ioIndex = 0;                
            }
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        private static void WriteUInt32Variant(uint value, ProtoWriter writer)
        {
            DemandSpace(5, writer);
            int count = 0;
            do {
                writer.ioBuffer[writer.ioIndex++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            writer.ioBuffer[writer.ioIndex - 1] &= 0x7F;
            writer.position64 += count;
        }

#if COREFX
        static readonly Encoding encoding = Encoding.UTF8;
#else
        static readonly UTF8Encoding encoding = new UTF8Encoding();
#endif

        internal static uint Zig(int value)
        {        
            return (uint)((value << 1) ^ (value >> 31));
        }
        internal static ulong Zig(long value)
        {
            return (ulong)((value << 1) ^ (value >> 63));
        }
        private static void WriteUInt64Variant(ulong value, ProtoWriter writer)
        {
            DemandSpace(10, writer);
            int count = 0;
            do
            {
                writer.ioBuffer[writer.ioIndex++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            writer.ioBuffer[writer.ioIndex - 1] &= 0x7F;
            writer.position64 += count;
        }
        /// <summary>
        /// Writes a string to the stream; supported wire-types: String
        /// </summary>
        public static void WriteString(string value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (writer.wireType != WireType.String) throw CreateException(writer);
            if (value == null) throw new ArgumentNullException("value"); // written header; now what?
            int len = value.Length;
            if (len == 0)
            {
                WriteUInt32Variant(0, writer);
                writer.wireType = WireType.None;
                return; // just a header
            }
#if MF
            byte[] bytes = encoding.GetBytes(value);
            int actual = bytes.Length;
            writer.WriteUInt32Variant((uint)actual);
            writer.Ensure(actual);
            Helpers.BlockCopy(bytes, 0, writer.ioBuffer, writer.ioIndex, actual);
#else
            int predicted = encoding.GetByteCount(value);
            WriteUInt32Variant((uint)predicted, writer);
            DemandSpace(predicted, writer);
            int actual = encoding.GetBytes(value, 0, value.Length, writer.ioBuffer, writer.ioIndex);
            Helpers.DebugAssert(predicted == actual);
#endif
            IncrementedAndReset(actual, writer);
        }
        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public static void WriteUInt64(ulong value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed64:
                    ProtoWriter.WriteInt64((long)value, writer);
                    return;
                case WireType.Variant:
                    WriteUInt64Variant(value, writer);
                    writer.wireType = WireType.None;
                    return;
                case WireType.Fixed32:
                    checked { ProtoWriter.WriteUInt32((uint)value, writer); }
                    return;
                default:
                    throw CreateException(writer);
            }
        }

        /// <summary>
        /// Writes a signed 64-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public static void WriteInt64(long value, ProtoWriter writer)
        {
            byte[] buffer;
            int index;
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed64:
                    DemandSpace(8, writer);
                    buffer = writer.ioBuffer;
                    index = writer.ioIndex;
                    buffer[index] = (byte)value;
                    buffer[index + 1] = (byte)(value >> 8);
                    buffer[index + 2] = (byte)(value >> 16);
                    buffer[index + 3] = (byte)(value >> 24);
                    buffer[index + 4] = (byte)(value >> 32);
                    buffer[index + 5] = (byte)(value >> 40);
                    buffer[index + 6] = (byte)(value >> 48);
                    buffer[index + 7] = (byte)(value >> 56);
                    IncrementedAndReset(8, writer);
                    return;
                case WireType.SignedVariant:
                    WriteUInt64Variant(Zig(value), writer);
                    writer.wireType = WireType.None;
                    return;
                case WireType.Variant:
                    if (value >= 0)
                    {
                        WriteUInt64Variant((ulong)value, writer);
                        writer.wireType = WireType.None;
                    }
                    else
                    {
                        DemandSpace(10, writer);
                        buffer = writer.ioBuffer;
                        index = writer.ioIndex;
                        buffer[index] = (byte)(value | 0x80);
                        buffer[index + 1] = (byte)((int)(value >> 7) | 0x80);
                        buffer[index + 2] = (byte)((int)(value >> 14) | 0x80);
                        buffer[index + 3] = (byte)((int)(value >> 21) | 0x80);
                        buffer[index + 4] = (byte)((int)(value >> 28) | 0x80);
                        buffer[index + 5] = (byte)((int)(value >> 35) | 0x80);
                        buffer[index + 6] = (byte)((int)(value >> 42) | 0x80);
                        buffer[index + 7] = (byte)((int)(value >> 49) | 0x80);
                        buffer[index + 8] = (byte)((int)(value >> 56) | 0x80);
                        buffer[index + 9] = 0x01; // sign bit
                        IncrementedAndReset(10, writer);
                    }
                    return;
                case WireType.Fixed32:
                    checked { WriteInt32((int)value, writer); }
                    return;
                default:
                    throw CreateException(writer);
            }
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public static void WriteUInt32(uint value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed32:
                    ProtoWriter.WriteInt32((int)value, writer);
                    return;
                case WireType.Fixed64:
                    ProtoWriter.WriteInt64((int)value, writer);
                    return;
                case WireType.Variant:
                    WriteUInt32Variant(value, writer);
                    writer.wireType = WireType.None;
                    return;
                default:
                    throw CreateException(writer);
            }
        }


        /// <summary>
        /// Writes a signed 16-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public static void WriteInt16(short value, ProtoWriter writer)
        {
            ProtoWriter.WriteInt32(value, writer);
        }
        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public static void WriteUInt16(ushort value, ProtoWriter writer)
        {
            ProtoWriter.WriteUInt32(value, writer);
        }
        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public static void WriteByte(byte value, ProtoWriter writer)
        {
            ProtoWriter.WriteUInt32(value, writer);
        }
        /// <summary>
        /// Writes a signed 8-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public static void WriteSByte(sbyte value, ProtoWriter writer)
        {
            ProtoWriter.WriteInt32(value, writer);
        }
        private static void WriteInt32ToBuffer(int value, byte[] buffer, int index)
        {
            buffer[index] = (byte)value;
            buffer[index + 1] = (byte)(value >> 8);
            buffer[index + 2] = (byte)(value >> 16);
            buffer[index + 3] = (byte)(value >> 24);
        }

        /// <summary>
        /// Writes a signed 32-bit integer to the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public static void WriteInt32(int value, ProtoWriter writer)
        {
            byte[] buffer;
            int index;
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed32:
                    DemandSpace(4, writer);
                    WriteInt32ToBuffer(value, writer.ioBuffer, writer.ioIndex);                    
                    IncrementedAndReset(4, writer);
                    return;
                case WireType.Fixed64:
                    DemandSpace(8, writer);
                    buffer = writer.ioBuffer;
                    index = writer.ioIndex;
                    buffer[index] = (byte)value;
                    buffer[index + 1] = (byte)(value >> 8);
                    buffer[index + 2] = (byte)(value >> 16);
                    buffer[index + 3] = (byte)(value >> 24);
                    buffer[index + 4] = buffer[index + 5] =
                        buffer[index + 6] = buffer[index + 7] = 0;
                    IncrementedAndReset(8, writer);
                    return;
                case WireType.SignedVariant:
                    WriteUInt32Variant(Zig(value), writer);
                    writer.wireType = WireType.None;
                    return;
                case WireType.Variant:
                    if (value >= 0)
                    {
                        WriteUInt32Variant((uint)value, writer);
                        writer.wireType = WireType.None;
                    }
                    else
                    {
                        DemandSpace(10, writer);
                        buffer = writer.ioBuffer;
                        index = writer.ioIndex;
                        buffer[index] = (byte)(value | 0x80);
                        buffer[index + 1] = (byte)((value >> 7) | 0x80);
                        buffer[index + 2] = (byte)((value >> 14) | 0x80);
                        buffer[index + 3] = (byte)((value >> 21) | 0x80);
                        buffer[index + 4] = (byte)((value >> 28) | 0x80);
                        buffer[index + 5] = buffer[index + 6] =
                            buffer[index + 7] = buffer[index + 8] = (byte)0xFF;
                        buffer[index + 9] = (byte)0x01;
                        IncrementedAndReset(10, writer);
                    }
                    return;
                default:
                    throw CreateException(writer);
            }
            
        }
        /// <summary>
        /// Writes a double-precision number to the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public
#if !FEAT_SAFE
            unsafe
#endif

                static void WriteDouble(double value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed32:
                    float f = (float)value;
                    if (Helpers.IsInfinity(f)
                        && !Helpers.IsInfinity(value))
                    {
                        throw new OverflowException();
                    }
                    ProtoWriter.WriteSingle(f, writer);
                    return;
                case WireType.Fixed64:
#if FEAT_SAFE
                    ProtoWriter.WriteInt64(BitConverter.ToInt64(BitConverter.GetBytes(value), 0), writer);
#else
                    ProtoWriter.WriteInt64(*(long*)&value, writer);
#endif
                    return;
                default:
                    throw CreateException(writer);
            }
        }
        /// <summary>
        /// Writes a single-precision number to the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public 
#if !FEAT_SAFE
            unsafe
#endif
            static void WriteSingle(float value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            switch (writer.wireType)
            {
                case WireType.Fixed32:
#if FEAT_SAFE
                    ProtoWriter.WriteInt32(BitConverter.ToInt32(BitConverter.GetBytes(value), 0), writer);
#else
                    ProtoWriter.WriteInt32(*(int*)&value, writer);
#endif
                    return;
                case WireType.Fixed64:
                    ProtoWriter.WriteDouble((double)value, writer);
                    return;
                default:
                    throw CreateException(writer);
            }
        }
        /// <summary>
        /// Throws an exception indicating that the given enum cannot be mapped to a serialized value.
        /// </summary>
        public static void ThrowEnumException(ProtoWriter writer, object enumValue)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            string rhs = enumValue == null ? "<null>" : (enumValue.GetType().FullName + "." + enumValue.ToString());
            throw new ProtoException("No wire-value is mapped to the enum " + rhs + " at position " + writer.position64.ToString());
        }
        // general purpose serialization exception message
        internal static Exception CreateException(ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            return new ProtoException("Invalid serialization operation with wire-type " + writer.wireType.ToString() + " at position " + writer.position64.ToString());
        }

        /// <summary>
        /// Writes a boolean to the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public static void WriteBoolean(bool value, ProtoWriter writer)
        {
            ProtoWriter.WriteUInt32(value ? (uint)1 : (uint)0, writer);
        }

        /// <summary>
        /// Copies any extension data stored for the instance to the underlying stream
        /// </summary>
        public static void AppendExtensionData(IExtensible instance, ProtoWriter writer)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (writer == null) throw new ArgumentNullException("writer");
            // we expect the writer to be raw here; the extension data will have the
            // header detail, so we'll copy it implicitly
            if(writer.wireType != WireType.None) throw CreateException(writer);

            IExtension extn = instance.GetExtensionObject(false);
            if (extn != null)
            {
                // unusually we *don't* want "using" here; the "finally" does that, with
                // the extension object being responsible for disposal etc
                Stream source = extn.BeginQuery();
                try
                {
                    CopyRawFromStream(source, writer);
                }
                finally { extn.EndQuery(source); }
            }
        }


        private int packedFieldNumber;
        /// <summary>
        /// Used for packed encoding; indicates that the next field should be skipped rather than
        /// a field header written. Note that the field number must match, else an exception is thrown
        /// when the attempt is made to write the (incorrect) field. The wire-type is taken from the
        /// subsequent call to WriteFieldHeader. Only primitive types can be packed.
        /// </summary>
        public static void SetPackedField(int fieldNumber, ProtoWriter writer)
        {
            if (fieldNumber <= 0) throw new ArgumentOutOfRangeException("fieldNumber");
            if (writer == null) throw new ArgumentNullException("writer");
            writer.packedFieldNumber = fieldNumber;
        }

        /// <summary>
        /// Used for packed encoding; explicitly reset the packed field marker; this is not required
        /// if using StartSubItem/EndSubItem
        /// </summary>
        public static void ClearPackedField(int fieldNumber, ProtoWriter writer)
        {
            if (fieldNumber != writer.packedFieldNumber)
                throw new InvalidOperationException("Field mismatch during packed encoding; expected " + writer.packedFieldNumber.ToString() + " but received " + fieldNumber.ToString());
            writer.packedFieldNumber = 0;
        }
        /// <summary>
        /// Used for packed encoding; writes the length prefix using fixed sizes rather than using
        /// buffering. Only valid for fixed-32 and fixed-64 encoding.
        /// </summary>
        public static void WritePackedPrefix(int elementCount, WireType wireType, ProtoWriter writer)
        {
            if (writer.WireType != WireType.String) throw new InvalidOperationException("Invalid wire-type: " + writer.WireType);
            if (elementCount < 0) throw new ArgumentOutOfRangeException(nameof(elementCount));
            ulong bytes;
            switch(wireType)
            {
                // use long in case very large arrays are enabled
                case WireType.Fixed32: bytes = ((ulong)elementCount) << 2; break; // x4
                case WireType.Fixed64: bytes = ((ulong)elementCount) << 3; break; // x8
                default:
                    throw new ArgumentOutOfRangeException(nameof(wireType), "Invalid wire-type: " + wireType);
            }
            WriteUInt64Variant(bytes, writer);
            writer.wireType = WireType.None;
        }

        internal string SerializeType(System.Type type)
        {
            return TypeModel.SerializeType(model, type);
        }
        /// <summary>
        /// Specifies a known root object to use during reference-tracked serialization
        /// </summary>
        public void SetRootObject(object value)
        {
            NetCache.SetKeyedObject(NetObjectCache.Root, value);
        }

        /// <summary>
        /// Writes a Type to the stream, using the model's DynamicTypeFormatting if appropriate; supported wire-types: String
        /// </summary>
        public static void WriteType(System.Type value, ProtoWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            WriteString(writer.SerializeType(value), writer);
        }
    }
}
