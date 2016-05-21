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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// An IBsonBuffer that has multiple chunks.
    /// </summary>
    public class MultiChunkBuffer : IByteBuffer
    {
        // private fields
        private readonly BsonChunkPool _chunkPool;
        private readonly int _chunkSize;
        private readonly int _sliceOffset;

        private int _capacity;
        private List<BsonChunk> _chunks;
        private bool _disposed;
        private bool _isReadOnly;
        private int _length;
        private int _position;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunkPool">The chunk pool.</param>
        /// <exception cref="System.ArgumentNullException">chunkPool</exception>
        public MultiChunkBuffer(BsonChunkPool chunkPool)
        {
            if (chunkPool == null)
            {
                throw new ArgumentNullException("chunkPool");
            }

            _chunkPool = chunkPool;
            _chunks = new List<BsonChunk>();
            _chunkSize = chunkPool.ChunkSize;
            _sliceOffset = 0;

            _capacity = 0; // EnsureSpaceAvailable will add capacity as needed
            _length = 0;
            _position = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="sliceOffset">The slice offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        /// <exception cref="System.ArgumentNullException">chunks</exception>
        internal MultiChunkBuffer(IEnumerable<BsonChunk> chunks, int sliceOffset, int length, bool isReadOnly)
        {
            if (chunks == null)
            {
                throw new ArgumentNullException("chunks");
            }

            _chunks = new List<BsonChunk>(chunks);
            if (_chunks.Count == 0)
            {
                throw new ArgumentException("No chunks where provided.", "chunks");
            }

            _chunkSize = _chunks[0].Bytes.Length;
            foreach (var chunk in _chunks)
            {
                if (chunk.Bytes.Length != _chunkSize) { throw new ArgumentException("The chunks are not all the same size."); }
            }

            if (sliceOffset < 0)
            {
                throw new ArgumentOutOfRangeException("sliceOffset");
            }
            _sliceOffset = sliceOffset;

            var maxCapacity = _chunks.Count * _chunkSize - _sliceOffset;
            if (length < 0 || length > maxCapacity)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            _capacity =  isReadOnly ? length : maxCapacity; // the capacity is fixed
            _length = length;

            _chunkPool = null;
            _isReadOnly = isReadOnly;
            _position = 0;

            foreach (var chunk in _chunks)
            {
                chunk.IncrementReferenceCount();
            }
        }

        // public properties
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.NotSupportedException">The capacity of a MultiChunkBuffer cannot be changed.</exception>
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
                EnsureIsWritable();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Capacity");
                }

                if (value < _capacity)
                {
                    ShrinkCapacity(value);
                }
                else if (value > _capacity)
                {
                    ExpandCapacity(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Length</exception>
        /// <exception cref="System.InvalidOperationException">The length of a read only buffer cannot be changed.</exception>
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
                if (value < 0 || value > _capacity)
                {
                    throw new ArgumentOutOfRangeException("Length");
                }
                EnsureIsWritable();

                EnsureSpaceAvailable(value - _position);
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
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

                EnsureSpaceAvailable(value - _position);
                _position = value;
                if (_length < _position)
                {
                    _length = _position;
                }
            }
        }

        // public methods
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The MultiChunkBuffer is read only.</exception>
        public void Clear()
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            ShrinkCapacity(0);
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public int FindNullByte()
        {
            ThrowIfDisposed();

            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var remaining = _length - _position;
            while (remaining > 0)
            {
                var chunkRemaining = _chunkSize - chunkOffset;
                var index = Array.IndexOf<byte>(_chunks[chunkIndex].Bytes, 0, chunkOffset, chunkRemaining);
                if (index != -1)
                {
                    return (chunkIndex * _chunkSize + index) - _sliceOffset;
                }
                chunkIndex += 1;
                chunkOffset = 0;
                remaining -= chunkRemaining;
            }

            return -1;
        }

        /// <summary>
        /// Gets a slice of this buffer.
        /// </summary>
        /// <param name="position">The position of the start of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>
        /// A slice of this buffer.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">GetSlice can only be called for read only buffers.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// position
        /// or
        /// length
        /// </exception>
        public IByteBuffer GetSlice(int position, int length)
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

            var firstChunk = (_sliceOffset + position) / _chunkSize;
            var lastChunk = (_sliceOffset + position + length - 1) / _chunkSize;
            var sliceOffset = (_sliceOffset + position) - (firstChunk * _chunkSize);

            if (firstChunk == lastChunk)
            {
                return new SingleChunkBuffer(_chunks[firstChunk], sliceOffset, length, true);
            }
            else
            {
                var chunks = _chunks.Skip(firstChunk).Take(lastChunk - firstChunk + 1);
                return new MultiChunkBuffer(chunks, sliceOffset, length, true);
            }
        }

        /// <summary>
        /// Loads the buffer from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The MultiChunkBuffer is read only.</exception>
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

            EnsureSpaceAvailable(count);
            var position = _position; // don't advance position
            while (count > 0)
            {
                var chunkIndex = (_sliceOffset + position) / _chunkSize;
                var chunkOffset = (_sliceOffset + position) % _chunkSize;
                var chunkRemaining = _chunkSize - chunkOffset;
                var bytesToRead = (count <= chunkRemaining) ? count : chunkRemaining;
                var bytesRead = stream.Read(_chunks[chunkIndex].Bytes, chunkOffset, bytesToRead);
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public ArraySegment<byte> ReadBackingBytes(int count)
        {
            ThrowIfDisposed();
            EnsureDataAvailable(count);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var chunkRemaining = _chunkSize - chunkOffset;
            if (count <= chunkRemaining)
            {
                _position += count;
                return new ArraySegment<byte>(_chunks[chunkIndex].Bytes, chunkOffset, count);
            }
            else
            {
                return new ArraySegment<byte>();
            }
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        /// <returns>
        /// A byte.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public byte ReadByte()
        {
            ThrowIfDisposed();
            EnsureDataAvailable(1);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var value = _chunks[chunkIndex].Bytes[chunkOffset];
            _position += 1;
            return value;
        }

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="destinationOffset">The destination offset.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public void ReadBytes(byte[] destination, int destinationOffset, int count)
        {
            ThrowIfDisposed();
            EnsureDataAvailable(count);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            while (count > 0)
            {
                var chunkRemaining = _chunkSize - chunkOffset;
                var bytesToCopy = (count < chunkRemaining) ? count : chunkRemaining;
                Buffer.BlockCopy(_chunks[chunkIndex].Bytes, chunkOffset, destination, destinationOffset, bytesToCopy);
                chunkIndex += 1;
                chunkOffset = 0;
                count -= bytesToCopy;
                destinationOffset += bytesToCopy;
                _position += bytesToCopy;
            }
        }

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The bytes.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public ArraySegment<byte> WriteBackingBytes(int count)
        {
            ThrowIfDisposed();
            EnsureSpaceAvailable(count);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var chunkRemaining = _chunkSize - chunkOffset;
            if (count <= chunkRemaining)
            {
                return new ArraySegment<byte>(_chunks[chunkIndex].Bytes, chunkOffset, count);
            }
            else
            {
                return new ArraySegment<byte>();
            }
        }

        /// <summary>
        /// Writes a byte.
        /// </summary>
        /// <param name="source">The byte.</param>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The MultiChunkBuffer is read only.</exception>
        public void WriteByte(byte source)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            EnsureSpaceAvailable(1);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            _chunks[chunkIndex].Bytes[chunkOffset] = source;
            _position += 1;
            if (_length < _position)
            {
                _length = _position;
            }
        }

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="source">The bytes (in the form of a byte array).</param>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The MultiChunkBuffer is read only.</exception>
        public void WriteBytes(byte[] source)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            EnsureSpaceAvailable(source.Length);
            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var remaining = source.Length;
            var sourceOffset = 0;
            while (remaining > 0)
            {
                var chunkRemaining = _chunkSize - chunkOffset;
                var bytesToCopy = (remaining < chunkRemaining) ? remaining : chunkRemaining;
                Buffer.BlockCopy(source, sourceOffset, _chunks[chunkIndex].Bytes, chunkOffset, bytesToCopy);
                chunkIndex += 1;
                chunkOffset = 0;
                remaining -= bytesToCopy;
                sourceOffset += bytesToCopy;
                _position += bytesToCopy;
            }
            if (_length < _position)
            {
                _length = _position;
            }
        }

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="source">The bytes (in the form of an IByteBuffer).</param>
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        /// <exception cref="System.InvalidOperationException">The MultiChunkBuffer is read only.</exception>
        public void WriteBytes(IByteBuffer source)
        {
            ThrowIfDisposed();
            EnsureIsWritable();
            EnsureSpaceAvailable(source.Length);

            var savedPosition = source.Position;
            source.Position = 0;

            var chunkIndex = (_sliceOffset + _position) / _chunkSize;
            var chunkOffset = (_sliceOffset + _position) % _chunkSize;
            var remaining = source.Length;
            while (remaining > 0)
            {
                var chunkRemaining = _chunkSize - chunkOffset;
                var bytesToCopy = (remaining < chunkRemaining) ? remaining : chunkRemaining;
                source.ReadBytes(_chunks[chunkIndex].Bytes, chunkOffset, bytesToCopy);
                chunkIndex += 1;
                chunkOffset = 0;
                remaining -= bytesToCopy;
                _position += bytesToCopy;
            }
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
        /// <exception cref="System.ObjectDisposedException">MultiChunkBuffer</exception>
        public void WriteTo(Stream stream)
        {
            ThrowIfDisposed();

            var chunkIndex = _sliceOffset / _chunkSize;
            var chunkOffset = _sliceOffset % _chunkSize;
            var remaining = _length;
            while (remaining > 0)
            {
                var chunkRemaining = _chunkSize - chunkOffset;
                var bytesToWrite = (remaining < chunkRemaining) ? remaining : chunkRemaining;
                stream.Write(_chunks[chunkIndex].Bytes, chunkOffset, bytesToWrite);
                chunkIndex += 1;
                chunkOffset = 0;
                remaining -= bytesToWrite;
            }
        }

        // protected methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_chunks != null)
                    {
                        foreach (var chunk in _chunks)
                        {
                            chunk.DecrementReferenceCount();
                        }
                        _chunks = null;
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

        private void EnsureSpaceAvailable(int needed)
        {
            if (needed > _capacity - _position)
            {
                var targetCapacity = _position + needed;
                ExpandCapacity(targetCapacity);
            }
        }

        private void ExpandCapacity(int targetCapacity)
        {
            if (_chunkPool == null)
            {
                throw new InvalidOperationException("Capacity cannot be expanded because this buffer was created without specifying a chunk pool.");
            }

            while (_capacity < targetCapacity)
            {
                var chunk = _chunkPool.AcquireChunk();
                chunk.IncrementReferenceCount();
                _chunks.Add(chunk);
                _capacity += _chunkSize;
            }
        }

        private void ShrinkCapacity(int targetCapacity)
        {
            while (_capacity > targetCapacity && (_capacity - targetCapacity) >= _chunkSize)
            {
                var lastIndex = _chunks.Count - 1;
                _chunks[lastIndex].DecrementReferenceCount();
                _chunks.RemoveAt(lastIndex);
                _capacity -= _chunkSize;
            }

            if (_length > targetCapacity)
            {
                _length = targetCapacity;
            }
            if (_position > targetCapacity)
            {
                _position = targetCapacity;
            }
        }
    }
}
