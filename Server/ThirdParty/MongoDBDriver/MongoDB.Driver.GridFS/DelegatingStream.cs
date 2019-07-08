/* Copyright 2016-present MongoDB Inc.
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
#if NET452
using System.Runtime.Remoting;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a Stream that delegates all of its operations to a wrapped Stream.
    /// </summary>
    /// <seealso cref="System.IO.Stream" />
    public class DelegatingStream : Stream
    {
        // private fields
        private readonly Stream _wrappedStream;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingStream"/> class.
        /// </summary>
        /// <param name="wrappedStream">The wrapped stream.</param>
        internal DelegatingStream(Stream wrappedStream)
        {
            _wrappedStream = wrappedStream;
        }

        // properties        
        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return _wrappedStream.CanRead; }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get { return _wrappedStream.CanSeek; }
        }

        /// <inheritdoc/>
        public override bool CanTimeout
        {
            get { return _wrappedStream.CanTimeout; }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return _wrappedStream.CanWrite; }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get { return _wrappedStream.Length; }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get { return _wrappedStream.Position; }
            set { _wrappedStream.Position = value; }
        }

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get { return _wrappedStream.ReadTimeout; }
            set { _wrappedStream.ReadTimeout = value; }
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get { return _wrappedStream.WriteTimeout; }
            set { _wrappedStream.WriteTimeout = value; }
        }

        // methods
#if NET452
        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _wrappedStream.BeginRead(buffer, offset, count, callback, state);
        }
#endif

#if NET452
        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _wrappedStream.BeginWrite(buffer, offset, count, callback, state);
        }
#endif

#if NET452
        /// <inheritdoc/>
        public override void Close()
        {
            _wrappedStream.Close();
        }
#endif

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _wrappedStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

 #if NET452
       /// <inheritdoc/>
        public override ObjRef CreateObjRef(Type requestedType)
        {
            return _wrappedStream.CreateObjRef(requestedType);
        }
#endif

#if NET452
        /// <inheritdoc/>
        [Obsolete("Not supported by DelegatingStream.")]
        protected override WaitHandle CreateWaitHandle()
        {
            throw new NotSupportedException();
        }
#endif

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrappedStream.Dispose();
            }
        }

#if NET452
        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _wrappedStream.EndRead(asyncResult);
        }
#endif

#if NET452
        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            _wrappedStream.EndWrite(asyncResult);
        }
#endif

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return _wrappedStream.Equals(obj);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _wrappedStream.Flush();
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _wrappedStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _wrappedStream.GetHashCode();
        }

 #if NET452
       /// <inheritdoc/>
        public override object InitializeLifetimeService()
        {
            return _wrappedStream.InitializeLifetimeService();
        }
#endif

#if NET452
        /// <inheritdoc/>
        [Obsolete("Not supported by DelegatingStream.")]
        protected override void ObjectInvariant()
        {
            throw new NotSupportedException();
        }
#endif

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)

        {
            return _wrappedStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _wrappedStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            return _wrappedStream.ReadByte();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _wrappedStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _wrappedStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _wrappedStream.ToString();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _wrappedStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _wrappedStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            _wrappedStream.WriteByte(value);
        }
    }
}
