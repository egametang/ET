/* Copyright 2013-2015 MongoDB Inc.
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
    /// Represents a source of chunks optimized for output buffers.
    /// </summary>
    public sealed class OutputBufferChunkSource : IBsonChunkSource
    {
        // constants
        const int DefaultInitialUnpooledChunkSize = 1024;
        const int DefaultMaxChunkSize = 1 * 1024 * 1024;
        const int DefaultMinChunkSize = 16 * 1024;

        // fields
        private readonly IBsonChunkSource _baseSource;
        private bool _disposed;
        private int _initialUnpooledChunkSize;
        private readonly int _maxChunkSize;
        private readonly int _minChunkSize;
        private int _previousChunkSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputBufferChunkSource"/> class.
        /// </summary>
        /// <param name="baseSource">The chunk source.</param>
        /// <param name="initialUnpooledChunkSize">The size of the initial unpooled chunk.</param>
        /// <param name="minChunkSize">The minimum size of a chunk.</param>
        /// <param name="maxChunkSize">The maximum size of a chunk.</param>
        public OutputBufferChunkSource(
            IBsonChunkSource baseSource,
            int initialUnpooledChunkSize = DefaultInitialUnpooledChunkSize,
            int minChunkSize = DefaultMinChunkSize,
            int maxChunkSize = DefaultMaxChunkSize)
        {
            if (baseSource == null)
            {
                throw new ArgumentNullException("baseSource");
            }
            if (initialUnpooledChunkSize < 0)
            {
                throw new ArgumentOutOfRangeException("initialUnpooledChunkSize");
            }
            if (minChunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException("minChunkSize");
            }
            if (maxChunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxChunkSize");
            }
            if (!PowerOf2.IsPowerOf2(minChunkSize))
            {
                throw new ArgumentException("minChunkSize is not a power of 2.", "minChunkSize");
            }
            if (!PowerOf2.IsPowerOf2(maxChunkSize))
            {
                throw new ArgumentException("maxChunkSize is not a power of 2.", "maxChunkSize");
            }
            if (maxChunkSize < minChunkSize)
            {
                throw new ArgumentException("maxChunkSize is less than minChunkSize", "maxChunkSize");
            }

            _baseSource = baseSource;
            _initialUnpooledChunkSize = initialUnpooledChunkSize;
            _minChunkSize = minChunkSize;
            _maxChunkSize = maxChunkSize;
        }

        // properties
        /// <summary>
        /// Gets the base source.
        /// </summary>
        /// <value>
        /// The base source.
        /// </value>
        public IBsonChunkSource BaseSource
        {
            get
            {
                ThrowIfDisposed();
                return _baseSource;
            }
        }

        /// <summary>
        /// Gets the initial unpooled chunk size.
        /// </summary>
        /// <value>
        /// The initial unpooled chunk size.
        /// </value>
        public int InitialUnpooledChunkSize
        {
            get { return _initialUnpooledChunkSize; }
        }

        /// <summary>
        /// Gets the maximum size of a chunk.
        /// </summary>
        /// <value>
        /// The maximum size of a chunk.
        /// </value>
        public int MaxChunkSize
        {
            get { return _maxChunkSize; }
        }

        /// <summary>
        /// Gets the minimum size of a chunk.
        /// </summary>
        /// <value>
        /// The minimum size of a chunk.
        /// </value>
        public int MinChunkSize
        {
            get { return _minChunkSize; }
        }

        // methods
        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <inheritdoc/>
        public IBsonChunk GetChunk(int requestedSize)
        {
            if (requestedSize <= 0)
            {
                throw new ArgumentOutOfRangeException("requestedSize");
            }
            ThrowIfDisposed();

            IBsonChunk chunk;
            if (_previousChunkSize == 0 && _initialUnpooledChunkSize != 0)
            {
                chunk = new ByteArrayChunk(_initialUnpooledChunkSize);
            }
            else
            {
                var powerOf2Size = PowerOf2.RoundUpToPowerOf2(_previousChunkSize + 1);
                var chunkSize = Math.Max(Math.Min(powerOf2Size, _maxChunkSize), _minChunkSize);
                chunk = _baseSource.GetChunk(chunkSize);
            }

            _previousChunkSize = chunk.Bytes.Count;
            return chunk;
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
