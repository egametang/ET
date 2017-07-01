/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a buffer for BSON encoded bytes.
    /// </summary>
    public class BsonBuffer : IDisposable
    {
        // private static fields
        private static readonly string[] __asciiStringTable = BuildAsciiStringTable();
        private static readonly bool[] __validBsonTypes = new bool[256];

        // private fields
        private bool _disposed = false;
        private IByteBuffer _byteBuffer;
        private bool _disposeByteBuffer;

        // static constructor
        static BsonBuffer()
        {
            foreach (BsonType bsonType in Enum.GetValues(typeof(BsonType)))
            {
                __validBsonTypes[(byte)bsonType] = true;
            }
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBuffer class.
        /// </summary>
        public BsonBuffer()
            : this(new MultiChunkBuffer(BsonChunkPool.Default), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonBuffer" /> class.
        /// </summary>
        /// <param name="byteBuffer">The buffer.</param>
        /// <param name="disposeByteBuffer">if set to <c>true</c> this BsonBuffer will own the byte buffer and when Dispose is called the byte buffer will be Disposed also.</param>
        public BsonBuffer(IByteBuffer byteBuffer, bool disposeByteBuffer)
        {
            _byteBuffer = byteBuffer;
            _disposeByteBuffer = disposeByteBuffer;
        }

        // public properties
        /// <summary>
        /// Gets the byte buffer.
        /// </summary>
        /// <value>
        /// The byte buffer.
        /// </value>
        public IByteBuffer ByteBuffer
        {
            get { return _byteBuffer; }
        }

        /// <summary>
        /// Gets or sets the length of the data in the buffer.
        /// </summary>
        public int Length
        {
            get
            {
                ThrowIfDisposed();
                return _byteBuffer.Length;
            }
            set
            {
                ThrowIfDisposed();
                _byteBuffer.Length = value;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the buffer.
        /// </summary>
        public int Position
        {
            get
            {
                ThrowIfDisposed();
                return _byteBuffer.Position;
            }
            set
            {
                ThrowIfDisposed();
                _byteBuffer.Position = value;
            }
        }

        // private static methods
        private static string[] BuildAsciiStringTable()
        {
            var asciiStringTable = new string[128];

            for (int i = 0; i < 128; ++i)
            {
                asciiStringTable[i] = new string((char)i, 1);
            }

            return asciiStringTable;
        }

        // public methods
        /// <summary>
        /// Backpatches the length of an object.
        /// </summary>
        /// <param name="position">The start position of the object.</param>
        /// <param name="length">The length of the object.</param>
        public void Backpatch(int position, int length)
        {
            ThrowIfDisposed();
            var savedPosition = _byteBuffer.Position;
            _byteBuffer.Position = position;
            WriteInt32(length);
            _byteBuffer.Position = savedPosition;
        }

        /// <summary>
        /// Clears the data in the buffer.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            _byteBuffer.Clear();
        }

        /// <summary>
        /// Copies data from the buffer to a byte array.
        /// </summary>
        /// <param name="sourceOffset">The source offset in the buffer.</param>
        /// <param name="destination">The destination byte array.</param>
        /// <param name="destinationOffset">The destination offset in the byte array.</param>
        /// <param name="count">The number of bytes to copy.</param>
        [Obsolete("Use ReadBytes instead.")]
        public void CopyTo(int sourceOffset, byte[] destination, int destinationOffset, int count)
        {
            ThrowIfDisposed();
            var savedPosition = _byteBuffer.Position;
            _byteBuffer.Position = sourceOffset;
            _byteBuffer.ReadBytes(destination, destinationOffset, count);
            _byteBuffer.Position = savedPosition;
        }

        /// <summary>
        /// Disposes of any resources held by the buffer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads the buffer from a Stream (the Stream must be positioned at a 4 byte length field).
        /// </summary>
        /// <param name="stream">The Stream.</param>
        public void LoadFrom(Stream stream)
        {
            LoadFrom(stream, 4); // does not advance position
            int length = ReadInt32(); // advances position 4 bytes
            LoadFrom(stream, length - 4); // does not advance position
            Position -= 4; // move back to just before the length field
        }

        /// <summary>
        /// Loads the buffer from a Stream (leaving the position in the buffer unchanged).
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The number of bytes to load.</param>
        public void LoadFrom(Stream stream, int count)
        {
            ThrowIfDisposed();
            _byteBuffer.LoadFrom(stream, count); // does not advance position
        }

        /// <summary>
        /// Peeks at the next byte in the buffer and returns it as a BsonType.
        /// </summary>
        /// <returns>A BsonType.</returns>
        [Obsolete("Use ReadBsonType instead.")]
        public BsonType PeekBsonType()
        {
            ThrowIfDisposed();
            var value = ReadBsonType();
            Position -= 1;
            return value;
        }

        /// <summary>
        /// Peeks at the next byte in the buffer.
        /// </summary>
        /// <returns>A Byte.</returns>
        [Obsolete("Use ReadByte instead.")]
        public byte PeekByte()
        {
            ThrowIfDisposed();
            var value = ReadByte();
            Position -= 1;
            return value;
        }

        /// <summary>
        /// Reads a BSON Boolean from the buffer.
        /// </summary>
        /// <returns>A Boolean.</returns>
        public bool ReadBoolean()
        {
            ThrowIfDisposed();
            return _byteBuffer.ReadByte() != 0;
        }

        /// <summary>
        /// Reads a BSON type from the buffer.
        /// </summary>
        /// <returns>A BsonType.</returns>
        public BsonType ReadBsonType()
        {
            ThrowIfDisposed();
            var bsonType = (int)_byteBuffer.ReadByte();
            if (!__validBsonTypes[bsonType])
            {
                string message = string.Format("Invalid BsonType {0}.", bsonType);
                throw new Exception(message);
            }
            return (BsonType)bsonType;
        }

        /// <summary>
        /// Reads a byte from the buffer.
        /// </summary>
        /// <returns>A Byte.</returns>
        public byte ReadByte()
        {
            ThrowIfDisposed();
            return _byteBuffer.ReadByte();
        }

        /// <summary>
        /// Reads bytes from the buffer.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>A byte array.</returns>
        public byte[] ReadBytes(int count)
        {
            ThrowIfDisposed();
            return _byteBuffer.ReadBytes(count);
        }

        /// <summary>
        /// Reads a BSON Double from the buffer.
        /// </summary>
        /// <returns>A Double.</returns>
        public double ReadDouble()
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.ReadBackingBytes(8);
            if (segment.Count >= 8)
            {
                return BitConverter.ToDouble(segment.Array, segment.Offset);
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(8);
                return BitConverter.ToDouble(bytes, 0);
            }
        }

        /// <summary>
        /// Reads a BSON Int32 from the reader.
        /// </summary>
        /// <returns>An Int32.</returns>
        public int ReadInt32()
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.ReadBackingBytes(4);
            if (segment.Count >= 4)
            {
                // for int only we come out ahead with this code vs using BitConverter
                return
                    ((int)segment.Array[segment.Offset + 0]) +
                    ((int)segment.Array[segment.Offset + 1] << 8) +
                    ((int)segment.Array[segment.Offset + 2] << 16) +
                    ((int)segment.Array[segment.Offset + 3] << 24);
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(4);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        /// <summary>
        /// Reads a BSON Int64 from the reader.
        /// </summary>
        /// <returns>An Int64.</returns>
        public long ReadInt64()
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.ReadBackingBytes(8);
            if (segment.Count >= 8)
            {
                return BitConverter.ToInt64(segment.Array, segment.Offset);
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(8);
                return BitConverter.ToInt64(bytes, 0);
            }
        }

        /// <summary>
        /// Reads a BSON ObjectId from the reader.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        public ObjectId ReadObjectId()
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.ReadBackingBytes(12);
            if (segment.Count >= 12)
            {
                var bytes = segment.Array;
                var offset = segment.Offset;
                var timestamp = (bytes[offset + 0] << 24) + (bytes[offset + 1] << 16) + (bytes[offset + 2] << 8) + bytes[offset + 3];
                var machine = (bytes[offset + 4] << 16) + (bytes[offset + 5] << 8) + bytes[offset + 6];
                var pid = (short)((bytes[offset + 7] << 8) + bytes[offset + 8]);
                var increment = (bytes[offset + 9] << 16) + (bytes[offset + 10] << 8) + bytes[offset + 11];
                return new ObjectId(timestamp, machine, pid, increment);
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(12);
                return new ObjectId(bytes);
            }
        }

        /// <summary>
        /// Reads a BSON ObjectId from the reader.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        [Obsolete("Use ReadObjectId() instead.")]
        public void ReadObjectId(out int timestamp, out int machine, out short pid, out int increment)
        {
            var objectId = ReadObjectId();
            timestamp = objectId.Timestamp;
            machine = objectId.Machine;
            pid = objectId.Pid;
            increment = objectId.Increment;
        }

        /// <summary>
        /// Reads a BSON string from the reader.
        /// </summary>
        /// <returns>A String.</returns>
        public string ReadString(UTF8Encoding encoding)
        {
            ThrowIfDisposed();
            var length = ReadInt32(); // length including the null terminator
            if (length <= 0)
            {
                var message = string.Format("Invalid string length: {0} (the length includes the null terminator so it must be greater than or equal to 1).", length);
                throw new Exception(message);
            }

            string value;
            byte finalByte;

            var segment = _byteBuffer.ReadBackingBytes(length);
            if (segment.Count >= length)
            {
                value = DecodeUtf8String(encoding, segment.Array, segment.Offset, length - 1);
                finalByte = segment.Array[segment.Offset + length - 1];
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(length);
                value = DecodeUtf8String(encoding, bytes, 0, length - 1);
                finalByte = bytes[length - 1];
            }

            if (finalByte != 0)
            {
                throw new Exception("String is missing null terminator.");
            }

            return value;
        }

        /// <summary>
        /// Reads a BSON CString from the reader (a null terminated string).
        /// </summary>
        /// <returns>A string.</returns>
        public string ReadCString(UTF8Encoding encoding)
        {
            ThrowIfDisposed();

            var nullPosition = _byteBuffer.FindNullByte();
            if (nullPosition == -1)
            {
                throw new BsonSerializationException("Missing null terminator.");
            }

            return ReadCString(encoding, nullPosition);
        }

        /// <summary>
        /// Reads an element name.
        /// </summary>
        /// <typeparam name="TValue">The type of the BsonTrie values.</typeparam>
        /// <param name="bsonTrie">An optional BsonTrie to use during decoding.</param>
        /// <param name="found">Set to true if the string was found in the trie.</param>
        /// <param name="value">Set to the value found in the trie; otherwise, null.</param>
        /// <returns>A string.</returns>
        public string ReadName<TValue>(BsonTrie<TValue> bsonTrie, out bool found, out TValue value)
        {
            ThrowIfDisposed();
            found = false;
            value = default(TValue);

            if (bsonTrie == null)
            {
                return ReadCString(new UTF8Encoding(false, true)); // always use strict encoding for names
            }

            var savedPosition = _byteBuffer.Position;
            var bsonTrieNode = bsonTrie.Root;
            while (true)
            {
                var keyByte = _byteBuffer.ReadByte();
                if (keyByte == 0)
                {
                    if (bsonTrieNode.HasValue)
                    {
                        found = true;
                        value = bsonTrieNode.Value;
                        return bsonTrieNode.ElementName;
                    }
                    else
                    {
                        var nullPosition = _byteBuffer.Position - 1;
                        _byteBuffer.Position = savedPosition;
                        return ReadCString(new UTF8Encoding(false, true), nullPosition); // always use strict encoding for names
                    }
                }

                bsonTrieNode = bsonTrieNode.GetChild(keyByte);
                if (bsonTrieNode == null)
                {
                    var nullPosition = _byteBuffer.FindNullByte(); // starting from where we got so far
                    _byteBuffer.Position = savedPosition;
                    return ReadCString(new UTF8Encoding(false, true), nullPosition); // always use strict encoding for names
                }
            }
        }

        /// <summary>
        /// Skips over bytes in the buffer (advances the position).
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        public void Skip(int count)
        {
            _byteBuffer.Position += count;
        }

        /// <summary>
        /// Skips over a CString in the buffer (advances the position).
        /// </summary>
        public void SkipCString()
        {
            ThrowIfDisposed();
            var nullPosition = _byteBuffer.FindNullByte();
            if (nullPosition == -1)
            {
                throw new Exception("String is missing null terminator");
            }
            _byteBuffer.Position = nullPosition + 1;
        }

        /// <summary>
        /// Converts the buffer to a byte array.
        /// </summary>
        /// <returns>A byte array.</returns>
        public byte[] ToByteArray()
        {
            ThrowIfDisposed();
            var savedPosition = _byteBuffer.Position;
            _byteBuffer.Position = 0;
            var byteArray = _byteBuffer.ReadBytes(_byteBuffer.Length);
            _byteBuffer.Position = savedPosition;
            return byteArray;
        }

        /// <summary>
        /// Writes a BSON Boolean to the buffer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        public void WriteBoolean(bool value)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes a byte to the buffer.
        /// </summary>
        /// <param name="value">A byte.</param>
        public void WriteByte(byte value)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteByte(value);
        }

        /// <summary>
        /// Writes bytes to the buffer.
        /// </summary>
        /// <param name="value">A byte array.</param>
        public void WriteBytes(byte[] value)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteBytes(value);
        }

        /// <summary>
        /// Writes a CString to the buffer.
        /// </summary>
        /// <param name="encoding">A UTF8 encoding.</param>
        /// <param name="value">A string.</param>
        public void WriteCString(UTF8Encoding encoding, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.IndexOf('\0') != -1)
            {
                throw new ArgumentException("CStrings cannot contain nulls.", "value");
            }
            ThrowIfDisposed();

            var maxLength = encoding.GetMaxByteCount(value.Length) + 1;
            var segment = _byteBuffer.WriteBackingBytes(maxLength);
            if (segment.Count >= maxLength)
            {
                var length = encoding.GetBytes(value, 0, value.Length, segment.Array, segment.Offset);
                segment.Array[segment.Offset + length] = 0;
                _byteBuffer.Position += length + 1;
            }
            else
            {
                _byteBuffer.WriteBytes(encoding.GetBytes(value));
                _byteBuffer.WriteByte(0);
            }
        }

        /// <summary>
        /// Writes a BSON Double to the buffer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        public void WriteDouble(double value)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a BSON Int32 to the buffer.
        /// </summary>
        /// <param name="value">The Int32 value.</param>
        public void WriteInt32(int value)
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.WriteBackingBytes(4);
            if (segment.Count >= 4)
            {
                segment.Array[segment.Offset + 0] = (byte)(value);
                segment.Array[segment.Offset + 1] = (byte)(value >> 8);
                segment.Array[segment.Offset + 2] = (byte)(value >> 16);
                segment.Array[segment.Offset + 3] = (byte)(value >> 24);
                _byteBuffer.Position += 4;
            }
            else
            {
                _byteBuffer.WriteBytes(BitConverter.GetBytes(value));
            }
        }

        /// <summary>
        /// Writes a BSON Int64 to the buffer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        public void WriteInt64(long value)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a BSON ObjectId to the buffer.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        [Obsolete("Use WriteObjectId(ObjectId objectId) instead.")]
        public void WriteObjectId(int timestamp, int machine, short pid, int increment)
        {
            var objectId = new ObjectId(timestamp, machine, pid, increment);
            WriteObjectId(objectId);
        }

        /// <summary>
        /// Writes a BSON ObjectId to the buffer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        public void WriteObjectId(ObjectId objectId)
        {
            ThrowIfDisposed();

            var segment = _byteBuffer.WriteBackingBytes(12);
            if (segment.Count >= 12)
            {
                var timestamp = objectId.Timestamp;
                var machine = objectId.Machine;
                var pid = objectId.Pid;
                var increment = objectId.Increment;
                segment.Array[segment.Offset + 0] = (byte)(timestamp >> 24);
                segment.Array[segment.Offset + 1] = (byte)(timestamp >> 16);
                segment.Array[segment.Offset + 2] = (byte)(timestamp >> 8);
                segment.Array[segment.Offset + 3] = (byte)(timestamp);
                segment.Array[segment.Offset + 4] = (byte)(machine >> 16);
                segment.Array[segment.Offset + 5] = (byte)(machine >> 8);
                segment.Array[segment.Offset + 6] = (byte)(machine);
                segment.Array[segment.Offset + 7] = (byte)(pid >> 8);
                segment.Array[segment.Offset + 8] = (byte)(pid);
                segment.Array[segment.Offset + 9] = (byte)(increment >> 16);
                segment.Array[segment.Offset + 10] = (byte)(increment >> 8);
                segment.Array[segment.Offset + 11] = (byte)(increment);
                _byteBuffer.Position += 12;
            }
            else
            {
                _byteBuffer.WriteBytes(objectId.ToByteArray());
            }
        }

        /// <summary>
        /// Writes a BSON String to the buffer.
        /// </summary>
        /// <param name="encoding">A UTF8 encoding.</param>
        /// <param name="value">The String value.</param>
        public void WriteString(UTF8Encoding encoding, string value)
        {
            ThrowIfDisposed();

            var maxLength = encoding.GetMaxByteCount(value.Length) + 5;
            var segment = _byteBuffer.WriteBackingBytes(maxLength);
            if (segment.Count >= maxLength)
            {
                var length = encoding.GetBytes(value, 0, value.Length, segment.Array, segment.Offset + 4);
                var lengthPlusOne = length + 1;
                segment.Array[segment.Offset + 0] = (byte)(lengthPlusOne); // now we know the length
                segment.Array[segment.Offset + 1] = (byte)(lengthPlusOne >> 8);
                segment.Array[segment.Offset + 2] = (byte)(lengthPlusOne >> 16);
                segment.Array[segment.Offset + 3] = (byte)(lengthPlusOne >> 24);
                segment.Array[segment.Offset + 4 + length] = 0;
                _byteBuffer.Position += length + 5;
            }
            else
            {
                var bytes = encoding.GetBytes(value);
                WriteInt32(bytes.Length + 1);
                _byteBuffer.WriteBytes(bytes);
                _byteBuffer.WriteByte(0);
            }
        }

        /// <summary>
        /// Writes all the data in the buffer to a Stream.
        /// </summary>
        /// <param name="stream">The Stream.</param>
        public void WriteTo(Stream stream)
        {
            ThrowIfDisposed();
            _byteBuffer.WriteTo(stream);
        }

        /// <summary>
        /// Writes a 32-bit zero the the buffer.
        /// </summary>
        [Obsolete("Use WriteByte or WriteInt32 instead.")]
        public void WriteZero()
        {
            ThrowIfDisposed();
            WriteInt32(0);
        }

        // private static methods
        private static string DecodeUtf8String(UTF8Encoding encoding, byte[] buffer, int index, int count)
        {
            switch (count)
            {
                // special case empty strings
                case 0:
                    return string.Empty;

                // special case single character strings
                case 1:
                    var byte1 = (int)buffer[index];
                    if (byte1 < __asciiStringTable.Length)
                    {
                        return __asciiStringTable[byte1];
                    }
                    break;
            }

            return encoding.GetString(buffer, index, count);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_byteBuffer != null)
                    {
                        if (_disposeByteBuffer)
                        {
                            _byteBuffer.Dispose();
                        }
                        _byteBuffer = null;
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
        
        // private methods
        private string ReadCString(UTF8Encoding encoding, int nullPosition)
        {
            if (nullPosition == -1)
            {
                throw new BsonSerializationException("Missing null terminator.");
            }

            var length = nullPosition - _byteBuffer.Position + 1;
            var segment = _byteBuffer.ReadBackingBytes(length);
            if (segment.Count >= length)
            {
                return DecodeUtf8String(encoding, segment.Array, segment.Offset, length - 1);
            }
            else
            {
                var bytes = _byteBuffer.ReadBytes(length);
                return DecodeUtf8String(encoding, bytes, 0, length - 1);
            }
        }
    }
}
