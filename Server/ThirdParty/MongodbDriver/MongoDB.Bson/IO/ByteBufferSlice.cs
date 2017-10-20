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
    /// Represents a slice of a byte buffer.
    /// </summary>
    public class ByteBufferSlice : IByteBuffer
    {
        private readonly IByteBuffer _buffer;
        private bool _disposed;
        private readonly int _length;
        private readonly int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBufferSlice"/> class.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="offset">The offset of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        public ByteBufferSlice(IByteBuffer buffer, int offset, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (!buffer.IsReadOnly)
            {
                throw new ArgumentException("The buffer is not read only.", "buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0 || offset + length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _buffer = buffer;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public IByteBuffer Buffer
        {
            get
            {
                ThrowIfDisposed();
                return _buffer;
            }
        }

        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return true;
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
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public ArraySegment<byte> AccessBackingBytes(int position)
        {
            EnsureValidPosition(position);
            ThrowIfDisposed();

            var segment = _buffer.AccessBackingBytes(position + _offset);
            var count = Math.Min(segment.Count, _length - position);
            return new ArraySegment<byte>(segment.Array, segment.Offset, count);
        }

        /// <inheritdoc/>
        public void Clear(int position, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.Clear(position + _offset, count);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void EnsureCapacity(int minimumCapacity)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public byte GetByte(int position)
        {
            EnsureValidPosition(position);
            ThrowIfDisposed();

            return _buffer.GetByte(position + _offset);
        }

        /// <inheritdoc/>
        public void GetBytes(int position, byte[] destination, int offset, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.GetBytes(position + _offset, destination, offset, count);
        }

        /// <inheritdoc/>
        public IByteBuffer GetSlice(int position, int length)
        {
            EnsureValidPositionAndLength(position, length);
            ThrowIfDisposed();

            return _buffer.GetSlice(position + _offset, length);
        }

        /// <inheritdoc/>
        public void MakeReadOnly()
        {
            ThrowIfDisposed();
        }

        /// <inheritdoc/>
        public void SetByte(int position, byte value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void SetBytes(int position, byte[] source, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _buffer.Dispose();
            }
            _disposed = true;
        }

        private void EnsureValidPosition(int position)
        {
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
        }

        private void EnsureValidPositionAndCount(int position, int count)
        {
            EnsureValidPosition(position);
            if (count < 0 || position + count > _length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }

        private void EnsureValidPositionAndLength(int position, int length)
        {
            EnsureValidPosition(position);
            if (length < 0 || position + length > _length)
            {
                throw new ArgumentOutOfRangeException("length");
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
