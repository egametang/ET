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
    /// An IByteBuffer that is backed by a contiguous byte array.
    /// </summary>
    public sealed class ByteArrayBuffer : IByteBuffer
    {
        // private fields
        private byte[] _bytes;
        private bool _disposed;
        private bool _isReadOnly;
        private int _length;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayBuffer"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        public ByteArrayBuffer(byte[] bytes, bool isReadOnly = false)
            : this(bytes, bytes == null ? 0 : bytes.Length, isReadOnly)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayBuffer"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        public ByteArrayBuffer(byte[] bytes, int length, bool isReadOnly = false)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (length < 0 || length > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _length = length;
            _bytes = bytes;
            _isReadOnly = isReadOnly;
        }

        // public properties
        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _isReadOnly ? _length : _bytes.Length;
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
                if (value < 0 || value > _bytes.Length)
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

            return new ArraySegment<byte>(_bytes, position, _length - position);
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

            Array.Clear(_bytes, position, count);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
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

            if (minimumCapacity > _bytes.Length)
            {
                var powerOf2 = Math.Max(32, PowerOf2.RoundUpToPowerOf2(minimumCapacity));
                SetCapacity(powerOf2);
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

            return _bytes[position];
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

            Buffer.BlockCopy(_bytes, position, destination, offset, count);
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

            var forkedBuffer = new ByteArrayBuffer(_bytes, _length, isReadOnly: true);
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

            _bytes[position] = value;
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

            Buffer.BlockCopy(source, offset, _bytes, position, count);
        }

        // private methods
        private void EnsureIsReadOnly()
        {
            if (!_isReadOnly)
            {
                var message = string.Format("{0} is not read only.", GetType().Name);
                throw new InvalidOperationException(message);
            }
        }

        private void EnsureIsWritable()
        {
            if (_isReadOnly)
            {
                var message = string.Format("{0} is not writable.", GetType().Name);
                throw new InvalidOperationException(message);
            }
        }

        private void SetCapacity(int capacity)
        {
            var oldBytes = _bytes;
            _bytes = new byte[capacity];
            var bytesToCopy = capacity < oldBytes.Length ? capacity : oldBytes.Length;
            Buffer.BlockCopy(oldBytes, 0, _bytes, 0, bytesToCopy);
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
