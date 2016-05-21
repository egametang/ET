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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a byte buffer (backed by various means depending on the implementation).
    /// </summary>
    public interface IByteBuffer : IDisposable
    {
        // properties
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        int Capacity { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        int Length { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        int Position { get; set; }

        // methods
        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Finds the next null byte.
        /// </summary>
        /// <returns>The position of the next null byte.</returns>
        int FindNullByte();

        /// <summary>
        /// Gets a slice of this buffer.
        /// </summary>
        /// <param name="position">The position of the start of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice of this buffer.</returns>
        IByteBuffer GetSlice(int position, int length);

        /// <summary>
        /// Loads the buffer from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The count.</param>
        void LoadFrom(Stream stream, int count);

        /// <summary>
        /// Makes this buffer read only.
        /// </summary>
        void MakeReadOnly();

        /// <summary>
        /// Read directly from the backing bytes. The returned ArraySegment points directly to the backing bytes for
        /// the current position and you can read the bytes directly from there. If the backing bytes happen to span
        /// a chunk boundary shortly after the current position there might not be enough bytes left in the current
        /// chunk in which case the returned ArraySegment will have a Count of zero and you should call ReadBytes instead.
        /// 
        /// When ReadBackingBytes returns the position will have been advanced by count bytes *if and only if* there
        /// were count bytes left in the current chunk.
        /// </summary>
        /// <param name="count">The number of bytes you need to read.</param>
        /// <returns>An ArraySegment pointing directly to the backing bytes for the current position.</returns>
        ArraySegment<byte> ReadBackingBytes(int count);

        /// <summary>
        /// Reads a byte.
        /// </summary>
        /// <returns>A byte.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="destinationOffset">The destination offset.</param>
        /// <param name="count">The count.</param>
        void ReadBytes(byte[] destination, int destinationOffset, int count);

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>The bytes.</returns>
        byte[] ReadBytes(int count);

        /// <summary>
        /// Write directly to the backing bytes. The returned ArraySegment points directly to the backing bytes for
        /// the current position and you can write the bytes directly to there. If the backing bytes happen to span
        /// a chunk boundary shortly after the current position there might not be enough bytes left in the current
        /// chunk in which case the returned ArraySegment will have a Count of zero and you should call WriteBytes instead.
        /// 
        /// When WriteBackingBytes returns the position has not been advanced. After you have written up to count
        /// bytes directly to the backing bytes advance the position by the number of bytes actually written.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>An ArraySegment pointing directly to the backing bytes for the current position.</returns>
        ArraySegment<byte> WriteBackingBytes(int count);
        
        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="source">The byte.</param>
        void WriteByte(byte source);

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="source">The bytes (in the form of a byte array).</param>
        void WriteBytes(byte[] source);

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="source">The bytes (in the form of an IByteBuffer).</param>
        void WriteBytes(IByteBuffer source);

        /// <summary>
        /// Writes Length bytes from this buffer starting at Position 0 to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        void WriteTo(Stream stream);
    }
}
