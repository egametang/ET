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
    /// Represents a chunk backed by a byte array.
    /// </summary>
    public class ByteArrayChunk : IBsonChunk
    {
        #region static
        private static byte[] CreateByteArray(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            return new byte[size];
        }
        #endregion

        // fields
        private byte[] _bytes;
        private bool _disposed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayChunk"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public ByteArrayChunk(int size)
            : this(CreateByteArray(size))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayChunk"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <exception cref="System.ArgumentNullException">bytes</exception>
        public ByteArrayChunk(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            _bytes = bytes;
        }

        // properties
        /// <inheritdoc/>
        public ArraySegment<byte> Bytes
        {
            get
            {
                ThrowIfDisposed();
                return new ArraySegment<byte>(_bytes);
            }
        }

        // methods
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public IBsonChunk Fork()
        {
            ThrowIfDisposed();
            return new ByteArrayChunk(_bytes);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bytes = null;
            }
            _disposed = true;
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
