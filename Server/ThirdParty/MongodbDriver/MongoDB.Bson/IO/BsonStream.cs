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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a Stream has additional methods to suport reading and writing BSON values.
    /// </summary>
    public abstract class BsonStream : Stream
    {
        /// <summary>
        /// Reads a BSON CString from the stream.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A string.</returns>
        public abstract string ReadCString(UTF8Encoding encoding);

        /// <summary>
        /// Reads a BSON CString from the stream.
        /// </summary>
        /// <returns>An ArraySegment containing the CString bytes (without the null byte).</returns>
        public abstract ArraySegment<byte> ReadCStringBytes();

        /// <summary>
        /// Reads a BSON Decimal128 from the stream.
        /// </summary>
        /// <returns>A <see cref="Decimal128"/>.</returns>
        public abstract Decimal128 ReadDecimal128();

        /// <summary>
        /// Reads a BSON double from the stream.
        /// </summary>
        /// <returns>A double.</returns>
        public abstract double ReadDouble();

        /// <summary>
        /// Reads a 32-bit BSON integer from the stream.
        /// </summary>
        /// <returns>An int.</returns>
        public abstract int ReadInt32();

        /// <summary>
        /// Reads a 64-bit BSON integer from the stream.
        /// </summary>
        /// <returns>A long.</returns>
        public abstract long ReadInt64();

        /// <summary>
        /// Reads a BSON ObjectId from the stream.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        public abstract ObjectId ReadObjectId();

        /// <summary>
        /// Reads a raw length prefixed slice from the stream.
        /// </summary>
        /// <returns>A slice.</returns>
        public abstract IByteBuffer ReadSlice();

        /// <summary>
        /// Reads a BSON string from the stream.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A string.</returns>
        public abstract string ReadString(UTF8Encoding encoding);

        /// <summary>
        /// Skips over a BSON CString leaving the stream positioned just after the terminating null byte.
        /// </summary>
        public abstract void SkipCString();

        /// <summary>
        /// Writes a BSON CString to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteCString(string value);

        /// <summary>
        /// Writes the CString bytes to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteCStringBytes(byte[] value);

        /// <summary>
        /// Writes a BSON Decimal128 to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteDecimal128(Decimal128 value);

        /// <summary>
        /// Writes a BSON double to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteDouble(double value);

        /// <summary>
        /// Writes a 32-bit BSON integer to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteInt32(int value);

        /// <summary>
        /// Writes a 64-bit BSON integer to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteInt64(long value);

        /// <summary>
        /// Writes a BSON ObjectId to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        public abstract void WriteObjectId(ObjectId value);

        /// <summary>
        /// Writes a BSON string to the stream.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding.</param>
        public abstract void WriteString(string value, UTF8Encoding encoding);
    }
}
