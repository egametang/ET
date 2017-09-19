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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// An IByteBuffer that is backed by multiple chunks.
    /// </summary>
    public sealed class MultiChunkBuffer : IByteBuffer
    {
        // private fields
        private int _capacity;
        private int _chunkIndex;
        private List<IBsonChunk> _chunks;
        private readonly IBsonChunkSource _chunkSource;
        private bool _disposed;
        private bool _isReadOnly;
        private int _length;
        private List<int> _positions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunkSource">The chunk pool.</param>
        /// <exception cref="System.ArgumentNullException">chunkPool</exception>
        public MultiChunkBuffer(IBsonChunkSource chunkSource)
        {
            if (chunkSource == null)
            {
                throw new ArgumentNullException("chunkSource");
            }

            _chunks = new List<IBsonChunk>();
            _chunkSource = chunkSource;
            _length = 0;
            _positions = new List<int> { 0 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChunkBuffer"/> class.
        /// </summary>
        /// <param name="chunks">The chunks.</param>
        /// <param name="length">The length.</param>
        /// <param name="isReadOnly">Whether the buffer is read only.</param>
        /// <exception cref="System.ArgumentNullException">chunks</exception>
        public MultiChunkBuffer(IEnumerable<IBsonChunk> chunks, int? length = null, bool isReadOnly = false)
        {
            if (chunks == null)
            {
                throw new ArgumentNullException("chunks");
            }
            var materializedList = new List<IBsonChunk>(chunks);

            var capacity = 0;
            var positions = new List<int> { 0 };
            foreach (var chunk in materializedList)
            {
                capacity += chunk.Bytes.Count;
                positions.Add(capacity);
            }

            if (length.HasValue && (length.Value < 0 || length.Value > capacity))
            {
                throw new ArgumentOutOfRangeException("length");
            }

            _capacity = capacity;
            _chunks = materializedList;
            _isReadOnly = isReadOnly;
            _length = length ?? capacity;
            _positions = positions;
        }

        // public properties
        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _isReadOnly ? _length : _capacity;
            }
        }

        /// <summary>
        /// Gets the chunk source.
        /// </summary>
        /// <value>
        /// The chunk source.
        /// </value>
        public IBsonChunkSource ChunkSource
        {
            get { return _chunkSource; }
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
                if (value < 0 || value > _capacity)
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

            var chunkIndex = GetChunkIndex(position);
            if (chunkIndex < _chunks.Count)
            {
                var segment = _chunks[chunkIndex].Bytes;
                var chunkOffset = position - _positions[chunkIndex];
                var chunkRemaining = segment.Count - chunkOffset;
                return new ArraySegment<byte>(segment.Array, segment.Offset + chunkOffset, chunkRemaining);
            }
            else
            {
                if (_chunks.Count > 0)
                {
                    var segment = _chunks[chunkIndex - 1].Bytes;
                    return new ArraySegment<byte>(segment.Array, segment.Offset + segment.Count, 0);
                }
                else
                {
                    return new ArraySegment<byte>(new byte[0], 0, 0);
                }
            }
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

            var chunkIndex = GetChunkIndex(position);
            var chunkOffset = position - _positions[chunkIndex];
            while (count > 0)
            {
                var segment = _chunks[chunkIndex].Bytes;
                var chunkRemaining = segment.Count - chunkOffset;
                var partialCount = Math.Min(count, chunkRemaining);
                Array.Clear(segment.Array, segment.Offset + chunkOffset, partialCount);
                chunkIndex += 1;
                chunkOffset = 0;
                count -= partialCount;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (var chunk in _chunks)
                {
                    chunk.Dispose();
                }
                _chunks = null;
                _positions = null;
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

            if (_capacity < minimumCapacity)
            {
                ExpandCapacity(minimumCapacity);
            }
        }

        /// <inheritdoc/>
        public byte GetByte(int position)
        {
            ThrowIfDisposed();
            if (position < 0 || position >= _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }

            var chunkIndex = GetChunkIndex(position);
            var chunkOffset = position - _positions[chunkIndex];
            var segment = _chunks[chunkIndex].Bytes;
            return segment.Array[segment.Offset + chunkOffset];
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

            var chunkIndex = GetChunkIndex(position);
            var chunkOffset = position - _positions[chunkIndex];
            while (count > 0)
            {
                var segment = _chunks[chunkIndex].Bytes;
                var chunkRemaining = segment.Count - chunkOffset;
                var partialCount = Math.Min(count, chunkRemaining);
                Buffer.BlockCopy(segment.Array, segment.Offset + chunkOffset, destination, offset, partialCount);
                chunkIndex += 1;
                chunkOffset = 0;
                count -= partialCount;
                offset += partialCount;
            }
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

            if (length == 0)
            {
                return new ByteArrayBuffer(new byte[0]);
            }

            var firstChunkIndex = GetChunkIndex(position);
            var lastChunkIndex = GetChunkIndex(position + length - 1);

            IByteBuffer forkedBuffer;
            if (firstChunkIndex == lastChunkIndex)
            {
                var forkedChunk = _chunks[firstChunkIndex].Fork();
                forkedBuffer = new SingleChunkBuffer(forkedChunk, forkedChunk.Bytes.Count, isReadOnly: true);
            }
            else
            {
                var forkedChunks = _chunks.Skip(firstChunkIndex).Take(lastChunkIndex - firstChunkIndex + 1).Select(c => c.Fork());
                var forkedBufferLength = _positions[lastChunkIndex + 1] - _positions[firstChunkIndex];
                forkedBuffer = new MultiChunkBuffer(forkedChunks, forkedBufferLength, isReadOnly: true);
            }

            var offset = position - _positions[firstChunkIndex];
            return new ByteBufferSlice(forkedBuffer, offset, length);
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
            if (position < 0 || position >= _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            EnsureIsWritable();

            var chunkIndex = GetChunkIndex(position);
            var chunkOffset = position - _positions[chunkIndex];
            var segment = _chunks[chunkIndex].Bytes;
            segment.Array[segment.Offset + chunkOffset] = value;
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

            var chunkIndex = GetChunkIndex(position);
            var chunkOffset = position - _positions[chunkIndex];
            while (count > 0)
            {
                var segment = _chunks[chunkIndex].Bytes;
                var chunkRemaining = segment.Count - chunkOffset;
                var partialCount = Math.Min(count, chunkRemaining);
                Buffer.BlockCopy(source, offset, segment.Array, segment.Offset + chunkOffset, partialCount);
                chunkIndex += 1;
                chunkOffset = 0;
                offset += partialCount;
                count -= partialCount;
            }
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

        private void ExpandCapacity(int minimumCapacity)
        {
            if (_chunkSource == null)
            {
                throw new InvalidOperationException("Capacity cannot be expanded because this buffer was created without specifying a chunk source.");
            }

            while (_capacity < minimumCapacity)
            {
                var chunk = _chunkSource.GetChunk(minimumCapacity);
                _chunks.Add(chunk);
                var newCapacity = (long)_capacity + (long)chunk.Bytes.Count;
                if (newCapacity > int.MaxValue)
                {
                    throw new InvalidOperationException("Capacity is limited to 2GB.");
                }
                _capacity = (int)newCapacity;
                _positions.Add(_capacity);
            }
        }

        private int GetChunkIndex(int position)
        {
            // locality of reference means this loop will only execute once most of the time
            while (true)
            {
                if (_chunkIndex + 1 < _positions.Count && position >= _positions[_chunkIndex + 1])
                {
                    _chunkIndex++;
                }
                else if (position < _positions[_chunkIndex])
                {
                    _chunkIndex--;
                }
                else
                {
                    return _chunkIndex;
                }
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
