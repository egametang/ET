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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// An IBsonBuffer that only has a single chunk.
    /// </summary>
    public class SingleChunkBuffer : ByteArrayBuffer
    {
        // private fields
        private BsonChunk _chunk;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <param name="sliceOffset">The slice offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        public SingleChunkBuffer(BsonChunk chunk, int sliceOffset, int length, bool isReadOnly)
            : base(GetChunkBytes(chunk), sliceOffset, length, isReadOnly)
        {
            _chunk = chunk;
            chunk.IncrementReferenceCount();
        }

        // public methods
        /// <summary>
        /// Gets a slice of this buffer.
        /// </summary>
        /// <param name="position">The position of the start of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>
        /// A slice of this buffer.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">SingleChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">GetSlice can only be called for read only buffers.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// position
        /// or
        /// length
        /// </exception>
        public override IByteBuffer GetSlice(int position, int length)
        {
            ThrowIfDisposed();
            EnsureIsReadOnly();
            if (position < 0 || position >= Length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (length <= 0 || length > Length - position)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            return new SingleChunkBuffer(_chunk, SliceOffset + position, length, true);
        }

        // protected methods
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">SingleChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        public override void Clear()
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            Length = 0;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (_chunk != null)
                    {
                        _chunk.DecrementReferenceCount();
                        _chunk = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        // private static methods
        private static byte[] GetChunkBytes(BsonChunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException("chunk");
            }
            return chunk.Bytes;
        }
    }
}
