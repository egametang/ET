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
    /// A BSON buffer that is backed by a byte array.
    /// </summary>
    public class ByteArrayBuffer : IByteBuffer
    {
        // private fields
        private bool _disposed;
        private byte[] _bytes;
        private int _sliceOffset;
        private int _capacity;
        private int _length;
        private int _position;
        private bool _isReadOnly;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayBuffer"/> class.
        /// </summary>
        /// <param name="bytes">The backing bytes.</param>
        /// <param name="sliceOffset">The offset where the slice begins.</param>
        /// <param name="length">The length of the slice.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        /// <exception cref="System.ArgumentNullException">bytes</exception>
        public ByteArrayBuffer(byte[] bytes, int sliceOffset, int length, bool isReadOnly)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            _bytes = bytes;
            _sliceOffset = sliceOffset;
            _capacity = isReadOnly ? length : bytes.Length - _sliceOffset;
            _length = length;
            _isReadOnly = isReadOnly;
            _position = 0;
        }

        // public properties
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.NotSupportedException">The capacity of a ByteArrayBuffer cannot be changed.</exception>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _capacity;
            }
            set
            {
                ThrowIfDisposed();
                throw new NotSupportedException("The capacity of a ByteArrayBuffer cannot be changed.");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return _isReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The length of a read only buffer cannot be changed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Length</exception>
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
                EnsureIsWritable();
                if (value < 0 || value > _capacity)
                {
                    throw new ArgumentOutOfRangeException("length");
                }
                _length = value;
                if (_position > _length)
                {
                    _position = _length;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Position</exception>
        public int Position
        {
            get
            {
                ThrowIfDisposed();
                return _position;
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0 || value > _capacity)
                {
                    throw new ArgumentOutOfRangeException("Position");
                }
                _position = value;
                if (_length < _position)
                {
                    _length = _position;
                }
            }
        }

        // protected properties
        /// <summary>
        /// Gets a value indicating whether this <see cref="ByteArrayBuffer"/> is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>
        /// Gets the slice offset.
        /// </summary>
        /// <value>
        /// The slice offset.
        /// </value>
        protected int SliceOffset
        {
            get { return _sliceOffset; }
        }

        // public methods
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        public virtual void Clear()
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            _position = 0;
            _length = 0;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finds the next null byte.
        /// </summary>
        /// <returns>
        /// The position of the next null byte.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public int FindNullByte()
        {
            ThrowIfDisposed();
            var index = Array.IndexOf<byte>(_bytes, 0, _sliceOffset + _position, _length - _position);
            return (index == -1) ? -1 : index - _sliceOffset;
        }

        /// <summary>
        /// Gets a slice of this buffer.
        /// </summary>
        /// <param name="position">The position of the start of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>
        /// A slice of this buffer.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">GetSlice can only be called for read only buffers.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// position
        /// or
        /// length
        /// </exception>
        public virtual IByteBuffer GetSlice(int position, int length)
        {
            ThrowIfDisposed();
            EnsureIsReadOnly();
            if (position < 0 || position >= _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            if (length <= 0 || length > _length - position)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            return new ByteArrayBuffer(_bytes, _sliceOffset + position, length, true);
        }

        /// <summary>
        /// Loads the buffer from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">count</exception>
        public void LoadFrom(Stream stream, int count)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (count > _capacity - _position)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            EnsureSpaceAvailable(count);
            var position = _position; // don't advance position
            while (count > 0)
            {
                var bytesRead = stream.Read(_bytes, _sliceOffset + position, count);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException();
                }
                position += bytesRead;
                count -= bytesRead;
            }
            if (_length < position)
            {
                _length = position;
            }
        }

        /// <summary>
        /// Makes this buffer read only.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public void MakeReadOnly()
        {
            ThrowIfDisposed();
            _isReadOnly = true;
        }

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
        /// <returns>
        /// An ArraySegment pointing directly to the backing bytes for the current position.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public ArraySegment<byte> ReadBackingBytes(int count)
        {
            ThrowIfDisposed();
            EnsureDataAvailable(count);
            var offset = _sliceOffset + _position;
            _position += count;
            return new ArraySegment<byte>(_bytes, offset, count);
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        /// <returns>
        /// A byte.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public byte ReadByte()
        {
            ThrowIfDisposed();
            EnsureDataAvailable(1);
            return _bytes[_sliceOffset + _position++];
        }

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="destinationOffset">The destination offset.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public void ReadBytes(byte[] destination, int destinationOffset, int count)
        {
            ThrowIfDisposed();
            EnsureDataAvailable(count);
            Buffer.BlockCopy(_bytes, _sliceOffset + _position, destination, destinationOffset, count);
            _position += count;
        }

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The bytes.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public byte[] ReadBytes(int count)
        {
            ThrowIfDisposed();

            var destination = new byte[count];
            ReadBytes(destination, 0, count);

            return destination;
        }

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
        /// <returns>
        /// An ArraySegment pointing directly to the backing bytes for the current position.
        /// </returns>
        public ArraySegment<byte> WriteBackingBytes(int count)
        {
            ThrowIfDisposed();
            EnsureSpaceAvailable(count);
            var offset = _sliceOffset + _position;
            return new ArraySegment<byte>(_bytes, offset, count);
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="source">The byte.</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        public void WriteByte(byte source)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            EnsureSpaceAvailable(1);
            _bytes[_sliceOffset + _position++] = source;
            if (_length < _position)
            {
                _length = _position;
            }
        }

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        public void WriteBytes(byte[] bytes)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            var count = bytes.Length;
            EnsureSpaceAvailable(count);
            Buffer.BlockCopy(bytes, 0, _bytes, _sliceOffset + _position, count);
            _position += count;
            if (_length < _position)
            {
                _length = _position;
            }
        }

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="source">The bytes (in the form of an IByteBuffer).</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        /// <exception cref="System.InvalidOperationException">Write operations are not allowed for read only buffers.</exception>
        public void WriteBytes(IByteBuffer source)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            var count = source.Length;
            EnsureSpaceAvailable(count);
            var savedPosition = source.Position;
            source.Position = 0;
            source.ReadBytes(_bytes, _sliceOffset + _position, count);
            _position += count;
            if (_length < _position)
            {
                _length = _position;
            }
            source.Position = savedPosition;
        }

        /// <summary>
        /// Writes Length bytes from this buffer starting at Position 0 to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer</exception>
        public void WriteTo(Stream stream)
        {
            ThrowIfDisposed();
            stream.Write(_bytes, _sliceOffset, _length);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // subclasses override this method if they have anything to Dispose
            _disposed = true;
        }

        /// <summary>
        /// Ensures the buffer is writable.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer is not writable.</exception>
        protected void EnsureIsWritable()
        {
            if (_isReadOnly)
            {
                var message = string.Format("{0} is not writable.", GetType().Name);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensures the buffer is read only.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">ByteArrayBuffer is not read only.</exception>
        protected void EnsureIsReadOnly()
        {
            if (!_isReadOnly)
            {
                var message = string.Format("{0} is not read only.", GetType().Name);
                throw new InvalidOperationException(message);
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
        private void EnsureDataAvailable(int needed)
        {
            if (needed > _length - _position)
            {
                var available = _length - _position;
                var message = string.Format(
                    "Not enough input bytes available. Needed {0}, but only {1} are available (at position {2}).",
                    needed, available, _position);
                throw new EndOfStreamException(message);
            }
        }

        private void EnsureSpaceAvailable(int needed)
        {
            if (needed > _capacity - _length)
            {
                var available = _capacity - _length;
                var message = string.Format(
                    "Not enough space available. Needed {0}, but only {1} are available (at position {2}).",
                    needed, available, _position);
                throw new EndOfStreamException(message);
            }
        }
    }
}
