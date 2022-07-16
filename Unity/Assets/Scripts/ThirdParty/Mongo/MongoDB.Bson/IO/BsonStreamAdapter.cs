/* Copyright 2010-present MongoDB Inc.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// A Stream that wraps another Stream while implementing the BsonStream abstract methods.
    /// </summary>
    public sealed class BsonStreamAdapter : BsonStream
    {
        // fields
        private bool _disposed;
        private bool _ownsStream;
        private readonly Stream _stream;
        private readonly byte[] _temp = new byte[12];

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonStreamAdapter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="ownsStream">if set to <c>true</c> [owns stream].</param>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        public BsonStreamAdapter(Stream stream, bool ownsStream = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            _stream = stream;
            _ownsStream = ownsStream;
        }

        // properties
        /// <summary>
        /// Gets the base stream.
        /// </summary>
        /// <value>
        /// The base stream.
        /// </value>
        public Stream BaseStream
        {
            get
            {
                ThrowIfDisposed();
                return _stream;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                ThrowIfDisposed();
                return _stream.CanRead;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                ThrowIfDisposed();
                return _stream.CanSeek;
            }
        }

        /// <inheritdoc/>
        public override bool CanTimeout
        {
            get
            {
                ThrowIfDisposed();
                return _stream.CanTimeout;
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                ThrowIfDisposed();
                return _stream.CanWrite;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return _stream.Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return _stream.Position;
            }
            set
            {
                ThrowIfDisposed();
                _stream.Position = value;
            }
        }

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get
            {
                ThrowIfDisposed();
                return _stream.ReadTimeout;
            }
            set
            {
                ThrowIfDisposed();
                _stream.ReadTimeout = value;
            }
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get
            {
                ThrowIfDisposed();
                return _stream.WriteTimeout;
            }
            set
            {
                ThrowIfDisposed();
                _stream.WriteTimeout = value;
            }
        }

        // methods
        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            base.Close(); // base class will call Dispose
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_ownsStream)
                    {
                        _stream.Dispose();
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return _stream.EndRead(asyncResult);
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            _stream.EndWrite(asyncResult);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            ThrowIfDisposed();
            _stream.Flush();
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _stream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return _stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            ThrowIfDisposed();
            return _stream.ReadByte();
        }

        /// <inheritdoc/>
        public override string ReadCString(UTF8Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            ThrowIfDisposed();

            var bytes = ReadCStringBytes();
            return Utf8Helper.DecodeUtf8String(bytes.Array, 0, bytes.Count, encoding);
        }

        /// <inheritdoc/>
        public override ArraySegment<byte> ReadCStringBytes()
        {
            ThrowIfDisposed();

            var memoryStream = new MemoryStream(32);

            while (true)
            {
                var b = _stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }
                if (b == 0)
                {
                    byte[] memoryStreamBuffer;
                    memoryStreamBuffer = memoryStream.GetBuffer();
                    return new ArraySegment<byte>(memoryStreamBuffer, 0, (int)memoryStream.Length);
                }

                memoryStream.WriteByte((byte)b);
            }
        }

        /// <inheritdoc/>
        public override Decimal128 ReadDecimal128()
        {
            ThrowIfDisposed();
            var lowBits = (ulong)ReadInt64();
            var highBits = (ulong)ReadInt64();
            return Decimal128.FromIEEEBits(highBits, lowBits);
        }

        /// <inheritdoc/>
        public override double ReadDouble()
        {
            ThrowIfDisposed();
            this.ReadBytes(_temp, 0, 8);
            return BitConverter.ToDouble(_temp, 0);
        }

        /// <inheritdoc/>
        public override int ReadInt32()
        {
            ThrowIfDisposed();
            this.ReadBytes(_temp, 0, 4);
            return _temp[0] | (_temp[1] << 8) | (_temp[2] << 16) | (_temp[3] << 24);
        }

        /// <inheritdoc/>
        public override long ReadInt64()
        {
            ThrowIfDisposed();
            this.ReadBytes(_temp, 0, 8);
            return BitConverter.ToInt64(_temp, 0);
        }

        /// <inheritdoc/>
        public override ObjectId ReadObjectId()
        {
            ThrowIfDisposed();
            this.ReadBytes(_temp, 0, 12);
            return new ObjectId(_temp, 0);
        }

        /// <inheritdoc/>
        public override IByteBuffer ReadSlice()
        {
            ThrowIfDisposed();
            var position = _stream.Position;
            var length = ReadInt32();
            var bytes = new byte[length];
            _stream.Position = position;
            this.ReadBytes(bytes, 0, length);
            return new ByteArrayBuffer(bytes, isReadOnly: true);
        }

        /// <inheritdoc/>
        public override string ReadString(UTF8Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            ThrowIfDisposed();

            var length = ReadInt32();
            using var rentedBuffer = ThreadStaticBuffer.RentBuffer(length);
            var bytes = rentedBuffer.Bytes;

            this.ReadBytes(bytes, 0, length);
            if (bytes[length - 1] != 0)
            {
                throw new FormatException("String is missing terminating null byte.");
            }

            return encoding.GetString(bytes, 0, length - 1);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return _stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            _stream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void SkipCString()
        {
            ThrowIfDisposed();

            while (true)
            {
                var b = _stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }
                if (b == 0)
                {
                    return;
                }
            }
        }

        /// <inheritdoc/>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            _stream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            _stream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override void WriteCString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ThrowIfDisposed();


            // Compare to 128 to preserve original behavior
            const int maxLengthToUseCStringUtf8EncodingWith = 128;
            if (CStringUtf8Encoding.GetMaxByteCount(value.Length) <= maxLengthToUseCStringUtf8EncodingWith)
            {
                using var rentedBuffer = ThreadStaticBuffer.RentBuffer(maxLengthToUseCStringUtf8EncodingWith);

                var length = CStringUtf8Encoding.GetBytes(value, rentedBuffer.Bytes, 0, Utf8Encodings.Strict);
                WriteBytes(rentedBuffer.Bytes, length);
            }
            else
            {
                using var rentedSegment = Utf8Encodings.Strict.GetBytesUsingThreadStaticBuffer(value);
                var segment = rentedSegment.Segment;
                if (Array.IndexOf<byte>(segment.Array, 0, 0, segment.Count) != -1)
                {
                    throw new ArgumentException("A CString cannot contain null bytes.", "value");
                }

                WriteBytes(segment.Array, segment.Count);
            }

            void WriteBytes(byte[] bytes, int length)
            {
                _stream.Write(bytes, 0, length);
                _stream.WriteByte(0);
            }
        }

        /// <inheritdoc/>
        public override void WriteCStringBytes(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ThrowIfDisposed();

            this.WriteBytes(value, 0, value.Length);
            WriteByte(0);
        }

        /// <inheritdoc/>
        public override void WriteDecimal128(Decimal128 value)
        {
            ThrowIfDisposed();
            WriteInt64((long)value.GetIEEELowBits());
            WriteInt64((long)value.GetIEEEHighBits());
        }

        /// <inheritdoc/>
        public override void WriteDouble(double value)
        {
            ThrowIfDisposed();
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, 8);
        }

        /// <inheritdoc/>
        public override void WriteInt32(int value)
        {
            ThrowIfDisposed();
            _temp[0] = (byte)(value);
            _temp[1] = (byte)(value >> 8);
            _temp[2] = (byte)(value >> 16);
            _temp[3] = (byte)(value >> 24);
            _stream.Write(_temp, 0, 4);
        }

        /// <inheritdoc/>
        public override void WriteInt64(long value)
        {
            ThrowIfDisposed();
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, 8);
        }

        /// <inheritdoc/>
        public override void WriteObjectId(ObjectId value)
        {
            ThrowIfDisposed();
            value.ToByteArray(_temp, 0);
            _stream.Write(_temp, 0, 12);
        }

        /// <inheritdoc/>
        public override void WriteString(string value, UTF8Encoding encoding)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            ThrowIfDisposed();

            using var rentedSegment = encoding.GetBytesUsingThreadStaticBuffer(value);
            var segment = rentedSegment.Segment;

            WriteInt32(segment.Count + 1);
            _stream.Write(segment.Array, 0, segment.Count);
            _stream.WriteByte(0);
        }
    }
}
