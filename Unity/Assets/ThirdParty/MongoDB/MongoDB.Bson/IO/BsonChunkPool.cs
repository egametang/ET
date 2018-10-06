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
using System.Threading;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a pool of chunks.
    /// </summary>
    public sealed class BsonChunkPool : IBsonChunkSource
    {
        #region static
        // static fields
        private static BsonChunkPool __default = new BsonChunkPool(8192, 64 * 1024); // 512MiB of 64KiB chunks

        // static properties
        /// <summary>
        /// Gets or sets the default chunk pool.
        /// </summary>
        /// <value>
        /// The default chunk pool.
        /// </value>
        public static BsonChunkPool Default
        {
            get { return __default; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                __default = value;
            }
        }
        #endregion

        // private fields
        private Stack<ReferenceCountedChunk> _chunks = new Stack<ReferenceCountedChunk>();
        private readonly int _chunkSize;
        private bool _disposed;
        private readonly object _lock = new object();
        private readonly int _maxChunkCount;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonChunkPool"/> class.
        /// </summary>
        /// <param name="maxChunkCount">The maximum number of chunks to keep in the pool.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        public BsonChunkPool(int maxChunkCount, int chunkSize)
        {
            if (maxChunkCount < 0)
            {
                throw new ArgumentOutOfRangeException("maxChunkCount");
            }
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException("chunkSize");
            }

            _maxChunkCount = maxChunkCount;
            _chunkSize = chunkSize;
        }

        // public properties
        /// <summary>
        /// Gets the chunk size.
        /// </summary>
        /// <value>
        /// The chunk size.
        /// </value>
        public int ChunkSize
        {
            get { return _chunkSize; }
        }

        /// <summary>
        /// Gets the maximum size of the pool.
        /// </summary>
        /// <value>
        /// The maximum size of the pool.
        /// </value>
        public int MaxChunkCount
        {
            get { return _maxChunkCount; }
        }

        /// <summary>
        /// Gets the size of the pool.
        /// </summary>
        /// <value>
        /// The size of the pool.
        /// </value>
        public int ChunkCount
        {
            get
            {
                lock (_lock)
                {
                    return _chunks.Count;
                }
            }
        }

        // public methods
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _chunks = null;
            }
        }

        /// <inheritdoc/>
        public IBsonChunk GetChunk(int requestedSize)
        {
            ThrowIfDisposed();

            ReferenceCountedChunk referenceCountedChunk = null;
            lock (_lock)
            {
                if (_chunks.Count > 0)
                {
                    referenceCountedChunk = _chunks.Pop();
                }
            }

            if (referenceCountedChunk == null)
            {
                var chunk = new byte[_chunkSize];
                referenceCountedChunk = new ReferenceCountedChunk(chunk, this);
            }

            return new DisposableChunk(referenceCountedChunk);
        }

        // private methods
        private void ReleaseChunk(ReferenceCountedChunk chunk)
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    if (_chunks.Count < _maxChunkCount)
                    {
                        _chunks.Push(chunk);
                    }
                    // otherwise just let it get garbage collected
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

        // nested types
        private sealed class DisposableChunk : IBsonChunk
        {
            // fields
            private bool _disposed;
            private readonly ReferenceCountedChunk _referenceCountedChunk;

            // constructors
            public DisposableChunk(ReferenceCountedChunk referenceCountedChunk)
            {
                _referenceCountedChunk = referenceCountedChunk;
                _referenceCountedChunk.IncrementReferenceCount();
            }

            // properties
            public ArraySegment<byte> Bytes
            {
                get
                {
                    ThrowIfDisposed();
                    return new ArraySegment<byte>(_referenceCountedChunk.Chunk);
                }
            }

            // methods
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _referenceCountedChunk.DecrementReferenceCount();
                }
            }

            public IBsonChunk Fork()
            {
                ThrowIfDisposed();
                return new DisposableChunk(_referenceCountedChunk);
            }

            // private methods
            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }

        private sealed class ReferenceCountedChunk
        {
            private byte[] _chunk;
            private BsonChunkPool _pool;
            private int _referenceCount;

            public ReferenceCountedChunk(byte[] chunk, BsonChunkPool pool)
            {
                _chunk = chunk;
                _pool = pool;
            }

            public byte[] Chunk
            {
                get { return _chunk; }
            }

            public void DecrementReferenceCount()
            {
                if (Interlocked.Decrement(ref _referenceCount) == 0)
                {
                    _pool.ReleaseChunk(this);
                }
            }

            public void IncrementReferenceCount()
            {
                Interlocked.Increment(ref _referenceCount);
            }
        }
    }
}
