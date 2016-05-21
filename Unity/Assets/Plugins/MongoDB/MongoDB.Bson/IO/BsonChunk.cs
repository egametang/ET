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
    /// Represents a BSON buffer chunk.
    /// </summary>
    public class BsonChunk
    {
        // private fields
        private readonly byte[] _bytes;
        private readonly BsonChunkPool _chunkPool;
        private int _referenceCount;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonChunk"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="chunkPool">The chunk pool.</param>
        /// <exception cref="System.ArgumentNullException">
        /// bytes
        /// or
        /// pool
        /// </exception>
        public BsonChunk(byte[] bytes, BsonChunkPool chunkPool)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chunkPool == null)
            {
                throw new ArgumentNullException("chunkPool");
            }

            _bytes = bytes;
            _chunkPool = chunkPool;
        }

        // public properties
        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        public byte[] Bytes
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Gets the reference count.
        /// </summary>
        /// <value>
        /// The reference count.
        /// </value>
        public int ReferenceCount
        {
            get { return _referenceCount; }
        }

        // public methods
        /// <summary>
        /// Decrements the reference count.
        /// </summary>
        /// <exception cref="BsonInternalException">Reference count is less than or equal to zero.</exception>
        public void DecrementReferenceCount()
        {
            if (_referenceCount <= 0)
            {
                throw new BsonInternalException("Reference count is less than or equal to zero.");
            }

            if (--_referenceCount == 0 && _chunkPool != null)
            {
                _chunkPool.ReleaseChunk(this);
            }
        }

        /// <summary>
        /// Increments the reference count.
        /// </summary>
        public void IncrementReferenceCount()
        {
            _referenceCount++;
        }
    }
}
