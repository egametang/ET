/* Copyright 2010-2015 MongoDB Inc.
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
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        int Capacity { get; }

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

        // methods
        /// <summary>
        /// Access the backing bytes directly. The returned ArraySegment will point to the desired position and contain
        /// as many bytes as possible up to the next chunk boundary (if any). If the returned ArraySegment does not
        /// contain enough bytes for your needs you will have to call ReadBytes instead.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>
        /// An ArraySegment pointing directly to the backing bytes for the position.
        /// </returns>
        ArraySegment<byte> AccessBackingBytes(int position);

        /// <summary>
        /// Clears the specified bytes.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="count">The count.</param>
        void Clear(int position, int count);

        /// <summary>
        /// Ensure that the buffer has a minimum capacity. Depending on the buffer allocation strategy
        /// calling this method may result in a higher capacity than the minimum (but never lower).
        /// </summary>
        /// <param name="minimumCapacity">The minimum capacity.</param>
        void EnsureCapacity(int minimumCapacity);

        /// <summary>
        /// Gets a slice of this buffer.
        /// </summary>
        /// <param name="position">The position of the start of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice of this buffer.</returns>
        IByteBuffer GetSlice(int position, int length);

        /// <summary>
        /// Makes this buffer read only.
        /// </summary>
        void MakeReadOnly();

        /// <summary>
        /// Gets a byte.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>A byte.</returns>
        byte GetByte(int position);

        /// <summary>
        /// Gets bytes.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="offset">The destination offset.</param>
        /// <param name="count">The count.</param>
        void GetBytes(int position, byte[] destination, int offset, int count);

        /// <summary>
        /// Sets a byte.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        void SetByte(int position, byte value);

        /// <summary>
        /// Sets bytes.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="source">The bytes.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        void SetBytes(int position, byte[] source, int offset, int count);
    }
}
