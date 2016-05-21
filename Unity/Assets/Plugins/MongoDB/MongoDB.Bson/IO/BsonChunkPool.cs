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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a pool of chunks used by BsonBuffer.
    /// </summary>
    public class BsonChunkPool
    {
        // private static fields
        private static BsonChunkPool __default = new BsonChunkPool(2048, 16 * 1024); // 32MiB of 16KiB chunks

        // private fields
        private readonly object _lock = new object();
        private readonly Stack<BsonChunk> _chunks = new Stack<BsonChunk>();
        private readonly int _maxPoolSize;
        private readonly int _chunkSize;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonChunkPool"/> class.
        /// </summary>
        /// <param name="maxPoolSize">The maximum number of chunks to keep in the pool.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        public BsonChunkPool(int maxPoolSize, int chunkSize)
        {
            _maxPoolSize = maxPoolSize;
            _chunkSize = chunkSize;
        }

        // public static properties
        /// <summary>
        /// Gets the default chunk pool.
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
                    throw new ArgumentNullException("Default");
                }
                __default = value;
            }
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
        /// Gets or sets the max pool size.
        /// </summary>
        public int MaxPoolSize
        {
            get { return _maxPoolSize; }
        }

        // internal methods
        /// <summary>
        /// Acquires a chunk.
        /// </summary>
        /// <returns></returns>
        internal  BsonChunk AcquireChunk()
        {
            lock (_lock)
            {
                if (_chunks.Count > 0)
                {
                    return _chunks.Pop();
                }
            }

            // release the lock before allocating memory
            var bytes = new byte[_chunkSize];
            return new BsonChunk(bytes, this);
        }

        /// <summary>
        /// Releases a chunk.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        internal void ReleaseChunk(BsonChunk chunk)
        {
            if (chunk.ReferenceCount != 0)
            {
                new BsonInternalException("A chunk is being returned to the pool and the reference count is not zero.");
            }

            lock (_lock)
            {
                if (_chunks.Count < _maxPoolSize)
                {
                    _chunks.Push(chunk);
                }
                // otherwise just let it get garbage collected
            }
        }
    }
}
