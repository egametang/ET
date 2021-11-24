
using System;

using System.IO;
using System.Text;
using ProtoBuf.Meta;

#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
#endif

#if MF
using EndOfStreamException = System.ApplicationException;
using OverflowException = System.ApplicationException;
#endif

namespace ProtoBuf
{
    /// <summary>
    /// A stateful reader, used to read a protobuf stream. Typical usage would be (sequentially) to call
    /// ReadFieldHeader and (after matching the field) an appropriate Read* method.
    /// </summary>
    public sealed class ProtoReader : IDisposable
    {
        Stream source;
        byte[] ioBuffer;
        TypeModel model;
        int fieldNumber, depth, ioIndex, available;
        long position64, blockEnd64, dataRemaining64;
        WireType wireType;
        bool isFixedLength, internStrings;
        private NetObjectCache netCache;

        // this is how many outstanding objects do not currently have
        // values for the purposes of reference tracking; we'll default
        // to just trapping the root object
        // note: objects are trapped (the ref and key mapped) via NoteObject
        uint trapCount; // uint is so we can use beq/bne more efficiently than bgt


        /// <summary>
        /// Gets the number of the field being processed.
        /// </summary>
        public int FieldNumber { get { return fieldNumber; } }
        /// <summary>
        /// Indicates the underlying proto serialization format on the wire.
        /// </summary>
        public WireType WireType { get { return wireType; } }

        /// <summary>
        /// Creates a new reader against a stream
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="model">The model to use for serialization; this can be null, but this will impair the ability to deserialize sub-objects</param>
        /// <param name="context">Additional context about this serialization operation</param>
        public ProtoReader(Stream source, TypeModel model, SerializationContext context) 
        {
            
            Init(this, source, model, context, TO_EOF);
        }

        internal const long TO_EOF = -1;


        /// <summary>
        /// Gets / sets a flag indicating whether strings should be checked for repetition; if
        /// true, any repeated UTF-8 byte sequence will result in the same String instance, rather
        /// than a second instance of the same string. Enabled by default. Note that this uses
        /// a <i>custom</i> interner - the system-wide string interner is not used.
        /// </summary>
        public bool InternStrings { get { return internStrings; } set { internStrings = value; } }

        /// <summary>
        /// Creates a new reader against a stream
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="model">The model to use for serialization; this can be null, but this will impair the ability to deserialize sub-objects</param>
        /// <param name="context">Additional context about this serialization operation</param>
        /// <param name="length">The number of bytes to read, or -1 to read until the end of the stream</param>
        public ProtoReader(Stream source, TypeModel model, SerializationContext context, int length)
        {
            Init(this, source, model, context, length);
        }
        /// <summary>
        /// Creates a new reader against a stream
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="model">The model to use for serialization; this can be null, but this will impair the ability to deserialize sub-objects</param>
        /// <param name="context">Additional context about this serialization operation</param>
        /// <param name="length">The number of bytes to read, or -1 to read until the end of the stream</param>
        public ProtoReader(Stream source, TypeModel model, SerializationContext context, long length)
        {
            Init(this, source, model, context, length);
        }

        private static void Init(ProtoReader reader, Stream source, TypeModel model, SerializationContext context, long length)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (!source.CanRead) throw new ArgumentException("Cannot read from stream", "source");
            reader.source = source;
            reader.ioBuffer = BufferPool.GetBuffer();
            reader.model = model;
            bool isFixedLength = length >= 0;
            reader.isFixedLength = isFixedLength;
            reader.dataRemaining64 = isFixedLength ? length : 0;

            if (context == null) { context = SerializationContext.Default; }
            else { context.Freeze(); }
            reader.context = context;
            reader.position64 = 0;
            reader.available = reader.depth = reader.fieldNumber = reader.ioIndex = 0;
            reader.blockEnd64 = long.MaxValue;
            reader.internStrings = true;
            reader.wireType = WireType.None;
            reader.trapCount = 1;
            if(reader.netCache == null) reader.netCache = new NetObjectCache();            
        }

        private SerializationContext context;

        /// <summary>
        /// Addition information about this deserialization operation.
        /// </summary>
        public SerializationContext Context { get { return context; } }
        /// <summary>
        /// Releases resources used by the reader, but importantly <b>does not</b> Dispose the 
        /// underlying stream; in many typical use-cases the stream is used for different
        /// processes, so it is assumed that the consumer will Dispose their stream separately.
        /// </summary>
        public void Dispose()
        {
            // importantly, this does **not** own the stream, and does not dispose it
            source = null;
            model = null;
            BufferPool.ReleaseBufferToPool(ref ioBuffer);
            if (stringInterner != null)
            {
                stringInterner.Clear();
                stringInterner = null;
            }
            if(netCache != null) netCache.Clear();
        }
        internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
        {
            if (available < 10) Ensure(10, false);
            if (available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = ioIndex;
            value = ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (available == 1) throw EoF(this);

            uint chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (available == 2) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (available == 3) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (available == 4) throw EoF(this);

            chunk = ioBuffer[readPos];
            value |= chunk << 28; // can only use 4 bits from this chunk
            if ((chunk & 0xF0) == 0) return 5;

            if (trimNegative // allow for -ve values
                && (chunk & 0xF0) == 0xF0
                && available >= 10
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0xFF
                    && ioBuffer[++readPos] == 0x01)
            {
                return 10;
            }
            throw AddErrorData(new OverflowException(), this);
        }
        private uint ReadUInt32Variant(bool trimNegative)
        {
            uint value;
            int read = TryReadUInt32VariantWithoutMoving(trimNegative, out value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                position64 += read;
                return value;
            }
            throw EoF(this);
        }
        private bool TryReadUInt32Variant(out uint value)
        {
            int read = TryReadUInt32VariantWithoutMoving(false, out value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                position64 += read;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public uint ReadUInt32()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return ReadUInt32Variant(false);
                case WireType.Fixed32:
                    if (available < 4) Ensure(4, true);
                    position64 += 4;
                    available -= 4;
                    return ((uint)ioBuffer[ioIndex++])
                        | (((uint)ioBuffer[ioIndex++]) << 8)
                        | (((uint)ioBuffer[ioIndex++]) << 16)
                        | (((uint)ioBuffer[ioIndex++]) << 24);
                case WireType.Fixed64:
                    ulong val = ReadUInt64();
                    checked { return (uint)val; }
                default:
                    throw CreateWireTypeException();
            }
        }
        
        /// <summary>
        /// Returns the position of the current reader (note that this is not necessarily the same as the position
        /// in the underlying stream, if multiple readers are used on the same stream)
        /// </summary>
        public int Position { get { return checked((int)position64); } }

        /// <summary>
        /// Returns the position of the current reader (note that this is not necessarily the same as the position
        /// in the underlying stream, if multiple readers are used on the same stream)
        /// </summary>
        public long LongPosition { get { return position64; } }
        internal void Ensure(int count, bool strict)
        {
            Helpers.DebugAssert(available <= count, "Asking for data without checking first");
            if (count > ioBuffer.Length)
            {
                BufferPool.ResizeAndFlushLeft(ref ioBuffer, count, ioIndex, available);
                ioIndex = 0;
            }
            else if (ioIndex + count >= ioBuffer.Length)
            {
                // need to shift the buffer data to the left to make space
                Helpers.BlockCopy(ioBuffer, ioIndex, ioBuffer, 0, available);
                ioIndex = 0;
            }
            count -= available;
            int writePos = ioIndex + available, bytesRead;
            int canRead = ioBuffer.Length - writePos;
            if (isFixedLength)
            {   // throttle it if needed
                if (dataRemaining64 < canRead) canRead = (int)dataRemaining64;
            }
            while (count > 0 && canRead > 0 && (bytesRead = source.Read(ioBuffer, writePos, canRead)) > 0)
            {
                available += bytesRead;
                count -= bytesRead;
                canRead -= bytesRead;
                writePos += bytesRead;
                if (isFixedLength) { dataRemaining64 -= bytesRead; }
            }
            if (strict && count > 0)
            {
                throw EoF(this);
            }

        }
        /// <summary>
        /// Reads a signed 16-bit integer from the stream: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public short ReadInt16()
        {
            checked { return (short)ReadInt32(); }
        }
        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public ushort ReadUInt16()
        {
            checked { return (ushort)ReadUInt32(); }
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public byte ReadByte()
        {
            checked { return (byte)ReadUInt32(); }
        }

        /// <summary>
        /// Reads a signed 8-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public sbyte ReadSByte()
        {
            checked { return (sbyte)ReadInt32(); }
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public int ReadInt32()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return (int)ReadUInt32Variant(true);
                case WireType.Fixed32:
                    if (available < 4) Ensure(4, true);
                    position64 += 4;
                    available -= 4;
                    return ((int)ioBuffer[ioIndex++])
                        | (((int)ioBuffer[ioIndex++]) << 8)
                        | (((int)ioBuffer[ioIndex++]) << 16)
                        | (((int)ioBuffer[ioIndex++]) << 24);
                case WireType.Fixed64:
                    long l = ReadInt64();
                    checked { return (int)l; }
                case WireType.SignedVariant:
                    return Zag(ReadUInt32Variant(true));
                default:
                    throw CreateWireTypeException();
            }
        }
        private const long Int64Msb = ((long)1) << 63;
        private const int Int32Msb = ((int)1) << 31;
        private static int Zag(uint ziggedValue)
        {
            int value = (int)ziggedValue;
            return (-(value & 0x01)) ^ ((value >> 1) & ~ProtoReader.Int32Msb);
        }

        private static long Zag(ulong ziggedValue)
        {
            long value = (long)ziggedValue;
            return (-(value & 0x01L)) ^ ((value >> 1) & ~ProtoReader.Int64Msb);
        }
        /// <summary>
        /// Reads a signed 64-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64, SignedVariant
        /// </summary>
        public long ReadInt64()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return (long)ReadUInt64Variant();
                case WireType.Fixed32:
                    return ReadInt32();
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    position64 += 8;
                    available -= 8;

                    return ((long)ioBuffer[ioIndex++])
                        | (((long)ioBuffer[ioIndex++]) << 8)
                        | (((long)ioBuffer[ioIndex++]) << 16)
                        | (((long)ioBuffer[ioIndex++]) << 24)
                        | (((long)ioBuffer[ioIndex++]) << 32)
                        | (((long)ioBuffer[ioIndex++]) << 40)
                        | (((long)ioBuffer[ioIndex++]) << 48)
                        | (((long)ioBuffer[ioIndex++]) << 56);

                case WireType.SignedVariant:
                    return Zag(ReadUInt64Variant());
                default:
                    throw CreateWireTypeException();
            }
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            if (available < 10) Ensure(10, false);
            if (available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = ioIndex;
            value = ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (available == 1) throw EoF(this);

            ulong chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (available == 2) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (available == 3) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (available == 4) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;
            if (available == 5) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;
            if (available == 6) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;
            if (available == 7) throw EoF(this);


            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;
            if (available == 8) throw EoF(this);

            chunk = ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;
            if (available == 9) throw EoF(this);

            chunk = ioBuffer[readPos];
            value |= chunk << 63; // can only use 1 bit from this chunk

            if ((chunk & ~(ulong)0x01) != 0) throw AddErrorData(new OverflowException(), this);
            return 10;
        }

        private ulong ReadUInt64Variant()
        {
            ulong value;
            int read = TryReadUInt64VariantWithoutMoving(out value);
            if (read > 0)
            {
                ioIndex += read;
                available -= read;
                position64 += read;
                return value;
            }
            throw EoF(this);
        }

#if NO_GENERICS
        private System.Collections.Hashtable stringInterner;
        private string Intern(string value)
        {
            if (value == null) return null;
            if (value.Length == 0) return "";
            if (stringInterner == null)
            {
                stringInterner = new System.Collections.Hashtable();
                stringInterner.Add(value, value);      
            }
            else if (stringInterner.ContainsKey(value))
            {
                value = (string)stringInterner[value];
            }
            else
            {
                stringInterner.Add(value, value);
            }
            return value;
        }
#else
        private System.Collections.Generic.Dictionary<string,string> stringInterner;
        private string Intern(string value)
        {
            if (value == null) return null;
            if (value.Length == 0) return "";
            string found;
            if (stringInterner == null)
            {
                stringInterner = new System.Collections.Generic.Dictionary<string, string>();
                stringInterner.Add(value, value);        
            }
            else if (stringInterner.TryGetValue(value, out found))
            {
                value = found;
            }
            else
            {
                stringInterner.Add(value, value);
            }
            return value;
        }
#endif

#if COREFX
        static readonly Encoding encoding = Encoding.UTF8;
#else
        static readonly UTF8Encoding encoding = new UTF8Encoding();
#endif
        /// <summary>
        /// Reads a string from the stream (using UTF8); supported wire-types: String
        /// </summary>
        public string ReadString()
        {
            if (wireType == WireType.String)
            {
                int bytes = (int)ReadUInt32Variant(false);
                if (bytes == 0) return "";
                if (available < bytes) Ensure(bytes, true);
#if MF
                byte[] tmp;
                if(ioIndex == 0 && bytes == ioBuffer.Length) {
                    // unlikely, but...
                    tmp = ioBuffer;
                } else {
                    tmp = new byte[bytes];
                    Helpers.BlockCopy(ioBuffer, ioIndex, tmp, 0, bytes);
                }
                string s = new string(encoding.GetChars(tmp));
#else
                string s = encoding.GetString(ioBuffer, ioIndex, bytes);
#endif
                if (internStrings) { s = Intern(s); }
                available -= bytes;
                position64 += bytes;
                ioIndex += bytes;
                return s;
            }
            throw CreateWireTypeException();
        }
        /// <summary>
        /// Throws an exception indication that the given value cannot be mapped to an enum.
        /// </summary>
        public void ThrowEnumException(System.Type type, int value)
        {
            string desc = type == null ? "<null>" : type.FullName;
            throw AddErrorData(new ProtoException("No " + desc + " enum is mapped to the wire-value " + value.ToString()), this);
        }
        private Exception CreateWireTypeException()
        {
            return CreateException("Invalid wire-type; this usually means you have over-written a file without truncating or setting the length; see http://stackoverflow.com/q/2152978/23354");
        }
        private Exception CreateException(string message)
        {
            return AddErrorData(new ProtoException(message), this);
        }
        /// <summary>
        /// Reads a double-precision number from the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public
#if !FEAT_SAFE
 unsafe
#endif
 double ReadDouble()
        {
            switch (wireType)
            {
                case WireType.Fixed32:
                    return ReadSingle();
                case WireType.Fixed64:
                    long value = ReadInt64();
#if FEAT_SAFE
                    return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
#else
                    return *(double*)&value;
#endif
                default:
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Reads (merges) a sub-message from the stream, internally calling StartSubItem and EndSubItem, and (in between)
        /// parsing the message in accordance with the model associated with the reader
        /// </summary>
        public static object ReadObject(object value, int key, ProtoReader reader)
        {
#if FEAT_IKVM
            throw new NotSupportedException();
#else
            return ReadTypedObject(value, key, reader, null);
#endif
        }
#if !FEAT_IKVM
        internal static object ReadTypedObject(object value, int key, ProtoReader reader, Type type)
        {
            if (reader.model == null)
            {
                throw AddErrorData(new InvalidOperationException("Cannot deserialize sub-objects unless a model is provided"), reader);
            }
            SubItemToken token = ProtoReader.StartSubItem(reader);
            if (key >= 0)
            {
                value = reader.model.Deserialize(key, value, reader);
            }
            else if (type != null && reader.model.TryDeserializeAuxiliaryType(reader, DataFormat.Default, Serializer.ListItemTag, type, ref value, true, false, true, false, null))
            {
                // ok
            }
            else
            {
                TypeModel.ThrowUnexpectedType(type);
            }
            ProtoReader.EndSubItem(token, reader);
            return value;
        }
#endif

        /// <summary>
        /// Makes the end of consuming a nested message in the stream; the stream must be either at the correct EndGroup
        /// marker, or all fields of the sub-message must have been consumed (in either case, this means ReadFieldHeader
        /// should return zero)
        /// </summary>
        public static void EndSubItem(SubItemToken token, ProtoReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            long value64 = token.value64;
            switch (reader.wireType)
            {
                case WireType.EndGroup:
                    if (value64 >= 0) throw AddErrorData(new ArgumentException("token"), reader);
                    if (-(int)value64 != reader.fieldNumber) throw reader.CreateException("Wrong group was ended"); // wrong group ended!
                    reader.wireType = WireType.None; // this releases ReadFieldHeader
                    reader.depth--;
                    break;
                // case WireType.None: // TODO reinstate once reads reset the wire-type
                default:
                    if (value64 < reader.position64) throw reader.CreateException($"Sub-message not read entirely; expected {value64}, was {reader.position64}");
                    if (reader.blockEnd64 != reader.position64 && reader.blockEnd64 != long.MaxValue)
                    {
                        throw reader.CreateException("Sub-message not read correctly");
                    }
                    reader.blockEnd64 = value64;
                    reader.depth--;
                    break;
                /*default:
                    throw reader.BorkedIt(); */
            }
        }

        /// <summary>
        /// Begins consuming a nested message in the stream; supported wire-types: StartGroup, String
        /// </summary>
        /// <remarks>The token returned must be help and used when callining EndSubItem</remarks>
        public static SubItemToken StartSubItem(ProtoReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            switch (reader.wireType)
            {
                case WireType.StartGroup:
                    reader.wireType = WireType.None; // to prevent glitches from double-calling
                    reader.depth++;
                    return new SubItemToken((long)(-reader.fieldNumber));
                case WireType.String:
                    long len = (long)reader.ReadUInt64Variant();
                    if (len < 0) throw AddErrorData(new InvalidOperationException(), reader);
                    long lastEnd = reader.blockEnd64;
                    reader.blockEnd64 = reader.position64 + len;
                    reader.depth++;
                    return new SubItemToken(lastEnd);
                default:
                    throw reader.CreateWireTypeException(); // throws
            }
        }

        /// <summary>
        /// Reads a field header from the stream, setting the wire-type and retuning the field number. If no
        /// more fields are available, then 0 is returned. This methods respects sub-messages.
        /// </summary>
        public int ReadFieldHeader()
        {
            // at the end of a group the caller must call EndSubItem to release the
            // reader (which moves the status to Error, since ReadFieldHeader must
            // then be called)
            if (blockEnd64 <= position64 || wireType == WireType.EndGroup) { return 0; }
            uint tag;
            if (TryReadUInt32Variant(out tag) && tag != 0)
            {
                wireType = (WireType)(tag & 7);
                fieldNumber = (int)(tag >> 3);
                if(fieldNumber < 1) throw new ProtoException("Invalid field in source data: " + fieldNumber.ToString());
            }
            else
            {
                wireType = WireType.None;
                fieldNumber = 0;
            }
            if (wireType == ProtoBuf.WireType.EndGroup)
            {
                if (depth > 0) return 0; // spoof an end, but note we still set the field-number
                throw new ProtoException("Unexpected end-group in source data; this usually means the source data is corrupt");
            }
            return fieldNumber;
        }
        /// <summary>
        /// Looks ahead to see whether the next field in the stream is what we expect
        /// (typically; what we've just finished reading - for example ot read successive list items)
        /// </summary>
        public bool TryReadFieldHeader(int field)
        {
            // check for virtual end of stream
            if (blockEnd64 <= position64 || wireType == WireType.EndGroup) { return false; }
            uint tag;
            int read = TryReadUInt32VariantWithoutMoving(false, out tag);
            WireType tmpWireType; // need to catch this to exclude (early) any "end group" tokens
            if (read > 0 && ((int)tag >> 3) == field
                && (tmpWireType = (WireType)(tag & 7)) != WireType.EndGroup)
            {
                wireType = tmpWireType;
                fieldNumber = field;
                position64 += read;
                ioIndex += read;
                available -= read;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the TypeModel associated with this reader
        /// </summary>
        public TypeModel Model { get { return model; } }

        /// <summary>
        /// Compares the streams current wire-type to the hinted wire-type, updating the reader if necessary; for example,
        /// a Variant may be updated to SignedVariant. If the hinted wire-type is unrelated then no change is made.
        /// </summary>
        public void Hint(WireType wireType)
        {
            if (this.wireType == wireType) { }  // fine; everything as we expect
            else if (((int)wireType & 7) == (int)this.wireType)
            {   // the underling type is a match; we're customising it with an extension
                this.wireType = wireType;
            }
            // note no error here; we're OK about using alternative data
        }

        /// <summary>
        /// Verifies that the stream's current wire-type is as expected, or a specialized sub-type (for example,
        /// SignedVariant) - in which case the current wire-type is updated. Otherwise an exception is thrown.
        /// </summary>
        public void Assert(WireType wireType)
        {
            if (this.wireType == wireType) { }  // fine; everything as we expect
            else if (((int)wireType & 7) == (int)this.wireType)
            {   // the underling type is a match; we're customising it with an extension
                this.wireType = wireType;
            }
            else
            {   // nope; that is *not* what we were expecting!
                throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Discards the data for the current field.
        /// </summary>
        public void SkipField()
        {
            switch (wireType)
            {
                case WireType.Fixed32:
                    if(available < 4) Ensure(4, true);
                    available -= 4;
                    ioIndex += 4;
                    position64 += 4;
                    return;
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    available -= 8;
                    ioIndex += 8;
                    position64 += 8;
                    return;
                case WireType.String:
                    long len = (long)ReadUInt64Variant();
                    if (len <= available)
                    { // just jump it!
                        available -= (int)len;
                        ioIndex += (int)len;
                        position64 += len;
                        return;
                    }
                    // everything remaining in the buffer is garbage
                    position64 += len; // assumes success, but if it fails we're screwed anyway
                    len -= available; // discount anything we've got to-hand
                    ioIndex = available = 0; // note that we have no data in the buffer
                    if (isFixedLength)
                    {
                        if (len > dataRemaining64) throw EoF(this);
                        // else assume we're going to be OK
                        dataRemaining64 -= len;
                    }
                    ProtoReader.Seek(source, len, ioBuffer);
                    return;
                case WireType.Variant:
                case WireType.SignedVariant:
                    ReadUInt64Variant(); // and drop it
                    return;
                case WireType.StartGroup:
                    int originalFieldNumber = this.fieldNumber;
                    depth++; // need to satisfy the sanity-checks in ReadFieldHeader
                    while (ReadFieldHeader() > 0) { SkipField(); }
                    depth--;
                    if (wireType == WireType.EndGroup && fieldNumber == originalFieldNumber)
                    { // we expect to exit in a similar state to how we entered
                        wireType = ProtoBuf.WireType.None;
                        return;
                    }
                    throw CreateWireTypeException();
                case WireType.None: // treat as explicit errorr
                case WireType.EndGroup: // treat as explicit error
                default: // treat as implicit error
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        public ulong ReadUInt64()
        {
            switch (wireType)
            {
                case WireType.Variant:
                    return ReadUInt64Variant();
                case WireType.Fixed32:
                    return ReadUInt32();
                case WireType.Fixed64:
                    if (available < 8) Ensure(8, true);
                    position64 += 8;
                    available -= 8;

                    return ((ulong)ioBuffer[ioIndex++])
                        | (((ulong)ioBuffer[ioIndex++]) << 8)
                        | (((ulong)ioBuffer[ioIndex++]) << 16)
                        | (((ulong)ioBuffer[ioIndex++]) << 24)
                        | (((ulong)ioBuffer[ioIndex++]) << 32)
                        | (((ulong)ioBuffer[ioIndex++]) << 40)
                        | (((ulong)ioBuffer[ioIndex++]) << 48)
                        | (((ulong)ioBuffer[ioIndex++]) << 56);
                default:
                    throw CreateWireTypeException();
            }
        }
        /// <summary>
        /// Reads a single-precision number from the stream; supported wire-types: Fixed32, Fixed64
        /// </summary>
        public
#if !FEAT_SAFE
 unsafe
#endif
 float ReadSingle()
        {
            switch (wireType)
            {
                case WireType.Fixed32:
                    {
                        int value = ReadInt32();
#if FEAT_SAFE
                        return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
#else
                        return *(float*)&value;
#endif
                    }
                case WireType.Fixed64:
                    {
                        double value = ReadDouble();
                        float f = (float)value;
                        if (Helpers.IsInfinity(f)
                            && !Helpers.IsInfinity(value))
                        {
                            throw AddErrorData(new OverflowException(), this);
                        }
                        return f;
                    }
                default:
                    throw CreateWireTypeException();
            }
        }

        /// <summary>
        /// Reads a boolean value from the stream; supported wire-types: Variant, Fixed32, Fixed64
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            switch (ReadUInt32())
            {
                case 0: return false;
                case 1: return true;
                default: throw CreateException("Unexpected boolean value");
            }
        }

        private static readonly byte[] EmptyBlob = new byte[0];
        /// <summary>
        /// Reads a byte-sequence from the stream, appending them to an existing byte-sequence (which can be null); supported wire-types: String
        /// </summary>
        public static byte[] AppendBytes(byte[] value, ProtoReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            switch (reader.wireType)
            {
                case WireType.String:
                    int len = (int)reader.ReadUInt32Variant(false);
                    reader.wireType = WireType.None;
                    if (len == 0) return value == null ? EmptyBlob : value;
                    int offset;
                    if (value == null || value.Length == 0)
                    {
                        offset = 0;
                        value = new byte[len];
                    }
                    else
                    {
                        offset = value.Length;
                        byte[] tmp = new byte[value.Length + len];
                        Helpers.BlockCopy(value, 0, tmp, 0, value.Length);
                        value = tmp;
                    }
                    // value is now sized with the final length, and (if necessary)
                    // contains the old data up to "offset"
                    reader.position64 += len; // assume success
                    while (len > reader.available)
                    {
                        if (reader.available > 0)
                        {
                            // copy what we *do* have
                            Helpers.BlockCopy(reader.ioBuffer, reader.ioIndex, value, offset, reader.available);
                            len -= reader.available;
                            offset += reader.available;
                            reader.ioIndex = reader.available = 0; // we've drained the buffer
                        }
                        //  now refill the buffer (without overflowing it)
                        int count = len > reader.ioBuffer.Length ? reader.ioBuffer.Length : len;
                        if (count > 0) reader.Ensure(count, true);
                    }
                    // at this point, we know that len <= available
                    if (len > 0)
                    {   // still need data, but we have enough buffered
                        Helpers.BlockCopy(reader.ioBuffer, reader.ioIndex, value, offset, len);
                        reader.ioIndex += len;
                        reader.available -= len;
                    }
                    return value;
                case WireType.Variant:
                    return new byte[0];
                default:
                    throw reader.CreateWireTypeException();
            }
        }

        //static byte[] ReadBytes(Stream stream, int length)
        //{
        //    if (stream == null) throw new ArgumentNullException("stream");
        //    if (length < 0) throw new ArgumentOutOfRangeException("length");
        //    byte[] buffer = new byte[length];
        //    int offset = 0, read;
        //    while (length > 0 && (read = stream.Read(buffer, offset, length)) > 0)
        //    {
        //        length -= read;
        //    }
        //    if (length > 0) throw EoF(null);
        //    return buffer;
        //}
        private static int ReadByteOrThrow(Stream source)
        {
            int val = source.ReadByte();
            if (val < 0) throw EoF(null);
            return val;
        }
        /// <summary>
        /// Reads the length-prefix of a message from a stream without buffering additional data, allowing a fixed-length
        /// reader to be created.
        /// </summary>
		public static int ReadLengthPrefix(Stream source, bool expectHeader, PrefixStyle style, out int fieldNumber){
			int bytesRead;
            return ReadLengthPrefix(source, expectHeader, style, out fieldNumber, out bytesRead);
		}

        /// <summary>
        /// Reads a little-endian encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadLittleEndianInt32(Stream source)
        {
            return ReadByteOrThrow(source)
                | (ReadByteOrThrow(source) << 8)
                | (ReadByteOrThrow(source) << 16)
                | (ReadByteOrThrow(source) << 24);
        }
        /// <summary>
        /// Reads a big-endian encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadBigEndianInt32(Stream source)
        {
            return (ReadByteOrThrow(source) << 24)
                 | (ReadByteOrThrow(source) << 16)
                 | (ReadByteOrThrow(source) << 8)
                 | ReadByteOrThrow(source);
        }
        /// <summary>
        /// Reads a varint encoded integer. An exception is thrown if the data is not all available.
        /// </summary>
        public static int DirectReadVarintInt32(Stream source)
        {
			ulong val;
            int bytes = TryReadUInt64Variant(source, out val);
            if (bytes <= 0) throw EoF(null);
            return checked((int)val);
        }
        /// <summary>
        /// Reads a string (of a given lenth, in bytes) directly from the source into a pre-existing buffer. An exception is thrown if the data is not all available.
        /// </summary>
        public static void DirectReadBytes(Stream source, byte[] buffer, int offset, int count)
        {
            int read;
            if (source == null) throw new ArgumentNullException("source");
            while(count > 0 && (read = source.Read(buffer, offset, count)) > 0)
            {
                count -= read;
                offset += read;
            }
            if (count > 0) throw EoF(null);
        }
        /// <summary>
        /// Reads a given number of bytes directly from the source. An exception is thrown if the data is not all available.
        /// </summary>
        public static byte[] DirectReadBytes(Stream source, int count)
        {
            byte[] buffer = new byte[count];
            DirectReadBytes(source, buffer, 0, count);
            return buffer;
        }
        /// <summary>
        /// Reads a string (of a given lenth, in bytes) directly from the source. An exception is thrown if the data is not all available.
        /// </summary>
        public static string DirectReadString(Stream source, int length)
        {
            byte[] buffer = new byte[length];
            DirectReadBytes(source, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        /// <summary>
        /// Reads the length-prefix of a message from a stream without buffering additional data, allowing a fixed-length
        /// reader to be created.
        /// </summary>
        public static int ReadLengthPrefix(Stream source, bool expectHeader, PrefixStyle style, out int fieldNumber, out int bytesRead)
        {
            if(style == PrefixStyle.None)
            {
                bytesRead = fieldNumber = 0;
                return int.MaxValue; // avoid the long.maxvalue causing overflow
            }
            long len64 = ReadLongLengthPrefix(source, expectHeader, style, out fieldNumber, out bytesRead);
            return checked((int)len64);
        }
        /// <summary>
        /// Reads the length-prefix of a message from a stream without buffering additional data, allowing a fixed-length
        /// reader to be created.
        /// </summary>
        public static long ReadLongLengthPrefix(Stream source, bool expectHeader, PrefixStyle style, out int fieldNumber, out int bytesRead)
        {
            fieldNumber = 0;
            switch (style)
            {
                case PrefixStyle.None:
                    bytesRead = 0;
                    return long.MaxValue;
                case PrefixStyle.Base128:
                    ulong val;
                    int tmpBytesRead;
                    bytesRead = 0;
                    if (expectHeader)
                    {
                        tmpBytesRead = ProtoReader.TryReadUInt64Variant(source, out val);
                        bytesRead += tmpBytesRead;
                        if (tmpBytesRead > 0)
                        {
                            if ((val & 7) != (uint)WireType.String)
                            { // got a header, but it isn't a string
                                throw new InvalidOperationException();
                            }
                            fieldNumber = (int)(val >> 3);
                            tmpBytesRead = ProtoReader.TryReadUInt64Variant(source, out val);
                            bytesRead += tmpBytesRead;
                            if (bytesRead == 0)
                            { // got a header, but no length
                                throw EoF(null);
                            }
                            return (long)val;
                        }
                        else
                        { // no header
                            bytesRead = 0;
                            return -1;
                        }
                    }
                    // check for a length
                    tmpBytesRead = ProtoReader.TryReadUInt64Variant(source, out val);
                    bytesRead += tmpBytesRead;
                    return bytesRead < 0 ? -1 : (long)val;

                case PrefixStyle.Fixed32:
                    {
                        int b = source.ReadByte();
                        if (b < 0)
                        {
                            bytesRead = 0;
                            return -1;
                        }
                        bytesRead = 4;
                        return b
                             | (ReadByteOrThrow(source) << 8)
                             | (ReadByteOrThrow(source) << 16)
                             | (ReadByteOrThrow(source) << 24);
                    }
                case PrefixStyle.Fixed32BigEndian:
                    {
                        int b = source.ReadByte();
                        if (b < 0)
                        {
                            bytesRead = 0;
                            return -1;
                        }
                        bytesRead = 4;
                        return (b << 24)
                            | (ReadByteOrThrow(source) << 16)
                            | (ReadByteOrThrow(source) << 8)
                            | ReadByteOrThrow(source);
                    }
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }
        /// <returns>The number of bytes consumed; 0 if no data available</returns>
        private static int TryReadUInt64Variant(Stream source, out ulong value)
        {
            value = 0;
            int b = source.ReadByte();
            if (b < 0) { return 0; }
            value = (uint)b;
            if ((value & 0x80) == 0) { return 1; }
            value &= 0x7F;
            int bytesRead = 1, shift = 7;
            while(bytesRead < 9)
            {
                b = source.ReadByte();
                if (b < 0) throw EoF(null);
                value |= ((ulong)b & 0x7F) << shift;
                shift += 7;

                if ((b & 0x80) == 0) return ++bytesRead;
            }
            b = source.ReadByte();
            if (b < 0) throw EoF(null);
            if((b & 1) == 0) // only use 1 bit from the last byte
            {
                value |= ((ulong)b & 0x7F) << shift;
                return ++bytesRead;
            }
            throw new OverflowException();
        }

        internal static void Seek(Stream source, long count, byte[] buffer)
        {
            if (source.CanSeek)
            {
                source.Seek(count, SeekOrigin.Current);
                count = 0;
            }
            else if (buffer != null)
            {
                int bytesRead;
                while (count > buffer.Length && (bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    count -= bytesRead;
                }
                while (count > 0 && (bytesRead = source.Read(buffer, 0, (int)count)) > 0)
                {
                    count -= bytesRead;
                }
            }
            else // borrow a buffer
            {
                buffer = BufferPool.GetBuffer();
                try
                {
                    int bytesRead;
                    while (count > buffer.Length && (bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        count -= bytesRead;
                    }
                    while (count > 0 && (bytesRead = source.Read(buffer, 0, (int)count)) > 0)
                    {
                        count -= bytesRead;
                    }
                }
                finally
                {
                    BufferPool.ReleaseBufferToPool(ref buffer);
                }
            }
            if (count > 0) throw EoF(null);
        }
        internal static Exception AddErrorData(Exception exception, ProtoReader source)
        {
#if !CF && !FX11 && !PORTABLE
            if (exception != null && source != null && !exception.Data.Contains("protoSource"))
            {
                exception.Data.Add("protoSource", string.Format("tag={0}; wire-type={1}; offset={2}; depth={3}",
                    source.fieldNumber, source.wireType, source.position64, source.depth));
            }
#endif
            return exception;

        }
        private static Exception EoF(ProtoReader source)
        {
            return AddErrorData(new EndOfStreamException(), source);
        }

        /// <summary>
        /// Copies the current field into the instance as extension data
        /// </summary>
        public void AppendExtensionData(IExtensible instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            IExtension extn = instance.GetExtensionObject(true);
            bool commit = false;
            // unusually we *don't* want "using" here; the "finally" does that, with
            // the extension object being responsible for disposal etc
            Stream dest = extn.BeginAppend();
            try
            {
                //TODO: replace this with stream-based, buffered raw copying
                using (ProtoWriter writer = new ProtoWriter(dest, model, null))
                {
                    AppendExtensionField(writer);
                    writer.Close();
                }
                commit = true;
            }
            finally { extn.EndAppend(dest, commit); }
        }
        private void AppendExtensionField(ProtoWriter writer)
        {
            //TODO: replace this with stream-based, buffered raw copying
            ProtoWriter.WriteFieldHeader(fieldNumber, wireType, writer);
            switch (wireType)
            {
                case WireType.Fixed32:
                    ProtoWriter.WriteInt32(ReadInt32(), writer);
                    return;
                case WireType.Variant:
                case WireType.SignedVariant:
                case WireType.Fixed64:
                    ProtoWriter.WriteInt64(ReadInt64(), writer);
                    return;
                case WireType.String:
                    ProtoWriter.WriteBytes(AppendBytes(null, this), writer);
                    return;
                case WireType.StartGroup:
                    SubItemToken readerToken = StartSubItem(this),
                        writerToken = ProtoWriter.StartSubItem(null, writer);
                    while (ReadFieldHeader() > 0) { AppendExtensionField(writer); }
                    EndSubItem(readerToken, this);
                    ProtoWriter.EndSubItem(writerToken, writer);
                    return;
                case WireType.None: // treat as explicit errorr
                case WireType.EndGroup: // treat as explicit error
                default: // treat as implicit error
                    throw CreateWireTypeException();
            }
        }
        /// <summary>
        /// Indicates whether the reader still has data remaining in the current sub-item,
        /// additionally setting the wire-type for the next field if there is more data.
        /// This is used when decoding packed data.
        /// </summary>
        public static bool HasSubValue(ProtoBuf.WireType wireType, ProtoReader source)
        {
            if (source == null) throw new ArgumentNullException("source");
            // check for virtual end of stream
            if (source.blockEnd64 <= source.position64 || wireType == WireType.EndGroup) { return false; }
            source.wireType = wireType;
            return true;
        }

        internal int GetTypeKey(ref Type type)
        {
            return model.GetKey(ref type);
        }

        internal NetObjectCache NetCache
        {
            get { return netCache; }
        }

        internal System.Type DeserializeType(string value)
        {
            return TypeModel.DeserializeType(model, value);
        }

        internal void SetRootObject(object value)
        {
            netCache.SetKeyedObject(NetObjectCache.Root, value);
            trapCount--;
        }

        
        /// <summary>
        /// Utility method, not intended for public use; this helps maintain the root object is complex scenarios
        /// </summary>
        public static void NoteObject(object value, ProtoReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if(reader.trapCount != 0)
            {
                reader.netCache.RegisterTrappedObject(value);
                reader.trapCount--;
            }
        }

        /// <summary>
        /// Reads a Type from the stream, using the model's DynamicTypeFormatting if appropriate; supported wire-types: String
        /// </summary>
        public System.Type ReadType()
        {
            return TypeModel.DeserializeType(model, ReadString());
        }

        internal void TrapNextObject(int newObjectKey)
        {
            trapCount++;
            netCache.SetKeyedObject(newObjectKey, null); // use null as a temp
        }

        internal void CheckFullyConsumed()
        {
            if (isFixedLength)
            {
                if (dataRemaining64 != 0) throw new ProtoException("Incorrect number of bytes consumed");
            }
            else
            {
                if (available != 0) throw new ProtoException("Unconsumed data left in the buffer; this suggests corrupt input");
            }
        }

        /// <summary>
        /// Merge two objects using the details from the current reader; this is used to change the type
        /// of objects when an inheritance relationship is discovered later than usual during deserilazation.
        /// </summary>
        public static object Merge(ProtoReader parent, object from, object to)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            TypeModel model = parent.Model;
            SerializationContext ctx = parent.Context;
            if(model == null) throw new InvalidOperationException("Types cannot be merged unless a type-model has been specified");
            using (MemoryStream ms = new MemoryStream())
            {
                model.Serialize(ms, from, ctx);
                ms.Position = 0;
                return model.Deserialize(ms, to, null);
            }
        }

        #region RECYCLER

        internal static ProtoReader Create(Stream source, TypeModel model, SerializationContext context, int len)
            => Create(source, model, context, (long)len);
        internal static ProtoReader Create(Stream source, TypeModel model, SerializationContext context, long len)
        {
            ProtoReader reader = GetRecycled();
            if (reader == null)
            {
                return new ProtoReader(source, model, context, len);
            }
            Init(reader, source, model, context, len);
            return reader;
        }

#if !PLAT_NO_THREADSTATIC
        [ThreadStatic]
        private static ProtoReader lastReader;

        private static ProtoReader GetRecycled()
        {
            ProtoReader tmp = lastReader;
            lastReader = null;
            return tmp;
        }
        internal static void Recycle(ProtoReader reader)
        {
            if(reader != null)
            {
                reader.Dispose();
                lastReader = reader;
            }
        }
#elif !PLAT_NO_INTERLOCKED
        private static object lastReader;
        private static ProtoReader GetRecycled()
        {
            return (ProtoReader)System.Threading.Interlocked.Exchange(ref lastReader, null);
        }
        internal static void Recycle(ProtoReader reader)
        {
            if(reader != null)
            {
                reader.Dispose();
                System.Threading.Interlocked.Exchange(ref lastReader, reader);
            }
        }
#else
        private static readonly object recycleLock = new object();
        private static ProtoReader lastReader;
        private static ProtoReader GetRecycled()
        {
            lock(recycleLock)
            {
                ProtoReader tmp = lastReader;
                lastReader = null;
                return tmp;
            }            
        }
        internal static void Recycle(ProtoReader reader)
        {
            if(reader != null)
            {
                reader.Dispose();
                lock(recycleLock)
                {
                    lastReader = reader;
                }
            }
        }
#endif

#endregion
    }
}
