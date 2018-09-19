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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// An IByteBuffer that is backed by a single chunk.
    /// </summary>
    public sealed class SingleChunkBuffer : IByteBuffer
    {
        // private fields
        private IBsonChunk _chunk;
        private bool _disposed;
        private bool _isReadOnly;
        private int _length;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunk">The chuns.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        public SingleChunkBuffer(IBsonChunk chunk, int length, bool isReadOnly = false)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException("chunk");
            }
            if (length < 0 || length > chunk.Bytes.Count)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _chunk = chunk;
            _length = length;
            _isReadOnly = isReadOnly;
        }

        // public properties
        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _isReadOnly ? _length : _chunk.Bytes.Count;
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return _isReadOnly;
            }
        }

        /// <inheritdoc/>
        public int Length
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0 || value > _chunk.Bytes.Count)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                EnsureIsWritable();

                _length = value;
            }
        }

        // public methods
        /// <inheritdoc/>
        public ArraySegment<byte> AccessBackingBytes(int position)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }

            var segment = _chunk.Bytes;
            return new ArraySegment<byte>(segment.Array, segment.Offset + position, _length - position);
        }

        /// <inheritdoc/>
        public void Clear(int position, int count)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (count < 0 || position + count > _length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            EnsureIsWritable();

            var segment = _chunk.Bytes;
            Array.Clear(segment.Array, segment.Offset + position, count);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _chunk.Dispose();
                _chunk = null;
            }
        }

        /// <inheritdoc/>
        public void EnsureCapacity(int minimumCapacity)
        {
            if (minimumCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("minimumCapacity");
            }
            ThrowIfDisposed();
            EnsureIsWritable();

            if (_chunk.Bytes.Count < minimumCapacity)
            {
                throw new NotSupportedException("Capacity cannot be expanded for a SingleChunkBuffer.");
            }
        }

        /// <inheritdoc/>
        public byte GetByte(int position)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }

            var segment = _chunk.Bytes;
            return segment.Array[segment.Offset + position];
        }

        /// <inheritdoc/>
        public void GetBytes(int position, byte[] destination, int offset, int count)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (offset < 0 || offset > destination.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || position + count > _length || offset + count > destination.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            var segment = _chunk.Bytes;
            Buffer.BlockCopy(segment.Array, segment.Offset + position, destination, offset, count);
        }

        /// <inheritdoc/>
        public IByteBuffer GetSlice(int position, int length)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (length < 0 || position + length > _length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            EnsureIsReadOnly();

            var forkedBuffer = new SingleChunkBuffer(_chunk.Fork(), _length, isReadOnly: true);
            return new ByteBufferSlice(forkedBuffer, position, length);
        }

        /// <inheritdoc/>
        public void MakeReadOnly()
        {
            ThrowIfDisposed();
            _isReadOnly = true;
        }

        /// <inheritdoc/>
        public void SetByte(int position, byte value)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            EnsureIsWritable();

            var segment = _chunk.Bytes;
            segment.Array[segment.Offset + position] = value;
        }

        /// <inheritdoc/>
        public void SetBytes(int position, byte[] source, int offset, int count)
        {
            ThrowIfDisposed();
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (offset < 0 || offset > source.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || position + count > _length || offset + count > source.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            EnsureIsWritable();

            var segment = _chunk.Bytes;
            Buffer.BlockCopy(source, offset, segment.Array, segment.Offset + position, count);
        }

        // private methods
        private void EnsureIsReadOnly()
        {
            if (!_isReadOnly)
            {
                throw new InvalidOperationException("MultiChunkBuffer is not read only.");
            }
        }

        private void EnsureIsWritable()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException("MultiChunkBuffer is not writable.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
