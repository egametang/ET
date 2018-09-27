/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents extension methods on BsonStream.
    /// </summary>
    public static class BsonStreamExtensions
    {
        // static fields
        private static readonly bool[] __validBsonTypes = new bool[256];

        // static constructor
        static BsonStreamExtensions()
        {
            foreach (BsonType bsonType in Enum.GetValues(typeof(BsonType)))
            {
                __validBsonTypes[(byte)bsonType] = true;
            }
        }

        // static methods
        /// <summary>
        /// Backpatches the size.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="startPosition">The start position.</param>
        public static void BackpatchSize(this BsonStream stream, long startPosition)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (startPosition < 0 || startPosition > stream.Length)
            {
                throw new ArgumentOutOfRangeException("startPosition");
            }

            var size = stream.Position - startPosition;
            if (size > int.MaxValue)
            {
                var message = string.Format("Size {0} is larger than {1} (Int32.MaxValue).", size, int.MaxValue);
                throw new FormatException(message);
            }

            var endPosition = stream.Position;
            stream.Position = startPosition;
            stream.WriteInt32((int)size);
            stream.Position = endPosition;
        }

        /// <summary>
        /// Reads the binary sub type.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The binary sub type.</returns>
        public static BsonBinarySubType ReadBinarySubType(this BsonStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            var b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            return (BsonBinarySubType)b;
        }

        /// <summary>
        /// Reads a boolean from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A boolean.</returns>
        public static bool ReadBoolean(this BsonStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            var b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            return b != 0;
        }

        /// <summary>
        /// Reads the BSON type.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The BSON type.</returns>
        public static BsonType ReadBsonType(this BsonStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var b = stream.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException();
            }
            if (!__validBsonTypes[b])
            {
                var message = string.Format("Detected unknown BSON type \"\\x{0:x2}\". Are you using the latest driver version?", b);
                throw new FormatException(message);
            }
            return (BsonType)b;
        }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public static void ReadBytes(this BsonStream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (count == 1)
            {
                var b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }
                buffer[offset] = (byte)b;
            }
            else
            {
                while (count > 0)
                {
                    var bytesRead = stream.Read(buffer, offset, count);
                    if (bytesRead == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += bytesRead;
                    count -= bytesRead;
                }
            }
        }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The count.</param>
        /// <returns>The bytes.</returns>
        public static byte[] ReadBytes(this BsonStream stream, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var bytes = new byte[count];
            stream.ReadBytes(bytes, 0, count);
            return bytes;
        }

        /// <summary>
        /// Writes a binary sub type to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        public static void WriteBinarySubType(this BsonStream stream, BsonBinarySubType value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes a boolean to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        public static void WriteBoolean(this BsonStream stream, bool value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Writes a BsonType to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="value">The value.</param>
        public static void WriteBsonType(this BsonStream stream, BsonType value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes bytes to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public static void WriteBytes(this BsonStream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (count == 1)
            {
                stream.WriteByte(buffer[offset]);
            }
            else
            {
                stream.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// Writes a slice to the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="slice">The slice.</param>
        public static void WriteSlice(this BsonStream stream, IByteBuffer slice)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (slice == null)
            {
                throw new ArgumentNullException("slice");
            }

            var position = 0;
            var count = slice.Length;

            while (count > 0)
            {
                var segment = slice.AccessBackingBytes(position);
                var partialCount = Math.Min(count, segment.Count);
                stream.WriteBytes(segment.Array, segment.Offset, partialCount);
                position += partialCount;
                count -= partialCount;
            }
        }
    }
}
