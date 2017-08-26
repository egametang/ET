/* Copyright 2010-2016 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a Stream backed by an IByteBuffer. Similar to MemoryStream but backed by an IByteBuffer
    /// instead of a byte array and also implements the BsonStream interface for higher performance BSON I/O.
    /// </summary>
    public class ByteBufferStream : BsonStream
    {
        // private fields
        private IByteBuffer _buffer;
        private bool _disposed;
        private int _length;
        private readonly bool _ownsBuffer;
        private int _position;
        private readonly byte[] _temp = new byte[12];
        private readonly byte[] _tempUtf8 = new byte[128];

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBufferStream"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="ownsBuffer">Whether the stream owns the buffer and should Dispose it when done.</param>
        public ByteBufferStream(IByteBuffer buffer, bool ownsBuffer = false)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            _buffer = buffer;
            _ownsBuffer = ownsBuffer;
            _length = buffer.Length;
        }

        // public properties
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public IByteBuffer Buffer
        {
            get
            {
                ThrowIfDisposed();
                return _buffer;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return !_disposed; }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get { return !_disposed; }
        }

        /// <inheritdoc/>
        public override bool CanTimeout
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get { return !_disposed && !_buffer.IsReadOnly; }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return _position;
            }
            set
            {
                if (value < 0 || value > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                ThrowIfDisposed();
                _position = (int)value;
            }
        }

        // public methods
        /// <inheritdoc/>
        public override void Flush()
        {
            ThrowIfDisposed();
            // do nothing
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            ThrowIfDisposed();

            if (_position >= _length)
            {
                return 0;
            }

            var available = _length - _position;
            if (count > available)
            {
                count = available;
            }

            _buffer.GetBytes(_position, buffer, offset, count);
            _position += count;

            return count;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            ThrowIfDisposed();
            if (_position >= _length)
            {
                return -1;
            }
            return _buffer.GetByte(_position++);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();

            long position;
            switch (origin)
            {
                case SeekOrigin.Begin: position = offset; break;
                case SeekOrigin.Current: position = _position + offset; break;
                case SeekOrigin.End: position = _length + offset; break;
                default: throw new ArgumentException("Invalid origin.", "origin");
            }
            if (position < 0)
            {
                throw new IOException("Attempted to seek before the beginning of the stream.");
            }
            if (position > int.MaxValue)
            {
                throw new IOException("Attempted to seek beyond the maximum value that can be represented using 32 bits.");
            }

            _position = (int)position;
            return position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            if (value < 0 || value > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            ThrowIfDisposed();
            EnsureWriteable();

            _buffer.EnsureCapacity((int)value);
            _length = (int)value;
            if (_position > _length)
            {
                _position = _length;
            }
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            ThrowIfDisposed();
            EnsureWriteable();

            PrepareToWrite(count);
            _buffer.SetBytes(_position, buffer, offset, count);
            SetPositionAfterWrite(_position + count);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            PrepareToWrite(1);
            _buffer.SetByte(_position, value);
            SetPositionAfterWrite(_position + 1);
        }

        // protected methods
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_ownsBuffer)
                {
                    _buffer.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        // private methods
        private void EnsureWriteable()
        {
            if (!CanWrite)
            {
                throw new NotSupportedException("Stream is not writeable.");
            }
        }

        private int FindNullByte()
        {
            var position = _position;
            while (position < _length)
            {
                var segment = _buffer.AccessBackingBytes(position);
                var endOfSegmentIndex = segment.Offset + segment.Count;
                for (var index = segment.Offset; index < endOfSegmentIndex; index++)
                {
                    if (segment.Array[index] == 0)
                    {
                        return position + (index - segment.Offset);
                    }
                }
                position += segment.Count;
            }

            throw new EndOfStreamException();
        }

        private void PrepareToWrite(int count)
        {
            var minimumCapacity = (long)_position + (long)count;
            if (minimumCapacity > int.MaxValue)
            {
                throw new IOException("Stream was too long.");
            }

            _buffer.EnsureCapacity((int)minimumCapacity);
            _buffer.Length = _buffer.Capacity;
            if (_length < _position)
            {
                _buffer.Clear(_length, _position - _length);
            }
        }

        private byte[] ReadBytes(int count)
        {
            ThrowIfEndOfStream(count);
            var bytes = new byte[count];
            _buffer.GetBytes(_position, bytes, 0, count);
            _position += count;
            return bytes;
        }

        private void SetPositionAfterWrite(int position)
        {
            _position = position;
            if (_length < position)
            {
                _length = position;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ByteBufferStream");
            }
        }

        private void ThrowIfEndOfStream(int count)
        {
            var minimumLength = (long)_position + (long)count;
            if (_length < minimumLength)
            {
                if (_position < _length)
                {
                    _position = _length;
                }
                throw new EndOfStreamException();
            }
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
            return Utf8Helper.DecodeUtf8String(bytes.Array, bytes.Offset, bytes.Count, encoding);
        }

        /// <inheritdoc/>
        public override ArraySegment<byte> ReadCStringBytes()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(1);

            var segment = _buffer.AccessBackingBytes(_position);
            var index = Array.IndexOf<byte>(segment.Array, 0, segment.Offset, segment.Count);
            if (index != -1)
            {
                var length = index - segment.Offset;
                _position += length + 1; // advance over the null byte
                return new ArraySegment<byte>(segment.Array, segment.Offset, length); // without the null byte
            }
            else
            {
                var nullPosition = FindNullByte();
                var length = nullPosition - _position;
                var cstring = ReadBytes(length + 1); // advance over the null byte
                return new ArraySegment<byte>(cstring, 0, length); // without the null byte
            }
        }

        /// <inheritdoc/>
        public override Decimal128 ReadDecimal128()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(16);

            var lowBits = (ulong)ReadInt64();
            var highBits = (ulong)ReadInt64();
            return Decimal128.FromIEEEBits(highBits, lowBits);
        }

        /// <inheritdoc/>
        public override double ReadDouble()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(8);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 8)
            {
                _position += 8;
                return BitConverter.ToDouble(segment.Array, segment.Offset);
            }
            else
            {
                this.ReadBytes(_temp, 0, 8);
                return BitConverter.ToDouble(_temp, 0);
            }
        }

        /// <inheritdoc/>
        public override int ReadInt32()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(4);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 4)
            {
                _position += 4;
                var bytes = segment.Array;
                var offset = segment.Offset;
                return bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
            }
            else
            {
                this.ReadBytes(_temp, 0, 4);
                return _temp[0] | (_temp[1] << 8) | (_temp[2] << 16) | (_temp[3] << 24);
            }
        }

        /// <inheritdoc/>
        public override long ReadInt64()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(8);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 8)
            {
                _position += 8;
                return BitConverter.ToInt64(segment.Array, segment.Offset);
            }
            else
            {
                this.ReadBytes(_temp, 0, 8);
                return BitConverter.ToInt64(_temp, 0);
            }
        }

        /// <inheritdoc/>
        public override ObjectId ReadObjectId()
        {
            ThrowIfDisposed();
            ThrowIfEndOfStream(12);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 12)
            {
                _position += 12;
                return new ObjectId(segment.Array, segment.Offset);
            }
            else
            {
                this.ReadBytes(_temp, 0, 12);
                return new ObjectId(_temp, 0);
            }
        }

        /// <inheritdoc/>
        public override IByteBuffer ReadSlice()
        {
            ThrowIfDisposed();

            var position = _position;
            var length = ReadInt32();
            ThrowIfEndOfStream(length - 4);
            Position = position + length;

            return _buffer.GetSlice(position, length);
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
            if (length <= 0)
            {
                var message = string.Format("Invalid string length: {0}.", length);
                throw new FormatException(message);
            }

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= length)
            {
                ThrowIfEndOfStream(length);
                if (segment.Array[segment.Offset + length - 1] != 0)
                {
                    throw new FormatException("String is missing terminating null byte.");
                }
                _position += length;
                return Utf8Helper.DecodeUtf8String(segment.Array, segment.Offset, length - 1, encoding);
            }
            else
            {
                var bytes = length <= _tempUtf8.Length ? _tempUtf8 : new byte[length];
                this.ReadBytes(bytes, 0, length);
                if (bytes[length - 1] != 0)
                {
                    throw new FormatException("String is missing terminating null byte.");
                }
                return Utf8Helper.DecodeUtf8String(bytes, 0, length - 1, encoding);
            }
        }

        /// <inheritdoc/>
        public override void SkipCString()
        {
            ThrowIfDisposed();
            var nullPosition = FindNullByte();
            _position = nullPosition + 1;
        }

        /// <inheritdoc/>
        public override void WriteCString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ThrowIfDisposed();

            var maxLength = CStringUtf8Encoding.GetMaxByteCount(value.Length) + 1;
            PrepareToWrite(maxLength);

            int actualLength;
            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= maxLength)
            {
                actualLength = CStringUtf8Encoding.GetBytes(value, segment.Array, segment.Offset, Utf8Encodings.Strict);
                segment.Array[segment.Offset + actualLength] = 0;
            }
            else
            {
                byte[] bytes;
                if (maxLength <= _tempUtf8.Length)
                {
                    bytes = _tempUtf8;
                    actualLength = CStringUtf8Encoding.GetBytes(value, bytes, 0, Utf8Encodings.Strict);
                }
                else
                {
                    bytes = Utf8Encodings.Strict.GetBytes(value);
                    if (Array.IndexOf<byte>(bytes, 0) != -1)
                    {
                        throw new ArgumentException("A CString cannot contain null bytes.", "value");
                    }
                    actualLength = bytes.Length;
                }

                _buffer.SetBytes(_position, bytes, 0, actualLength);
                _buffer.SetByte(_position + actualLength, 0);
            }

            SetPositionAfterWrite(_position + actualLength + 1);
        }

        /// <inheritdoc/>
        public override void WriteCStringBytes(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ThrowIfDisposed();

            var length = value.Length;

            PrepareToWrite(length + 1);

            _buffer.SetBytes(_position, value, 0, length);
            _buffer.SetByte(_position + length, 0);

            SetPositionAfterWrite(_position + length + 1);
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

            PrepareToWrite(8);

            var bytes = BitConverter.GetBytes(value);
            _buffer.SetBytes(_position, bytes, 0, 8);

            SetPositionAfterWrite(_position + 8);
        }

        /// <inheritdoc/>
        public override void WriteInt32(int value)
        {
            ThrowIfDisposed();

            PrepareToWrite(4);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 4)
            {
                segment.Array[segment.Offset] = (byte)value;
                segment.Array[segment.Offset + 1] = (byte)(value >> 8);
                segment.Array[segment.Offset + 2] = (byte)(value >> 16);
                segment.Array[segment.Offset + 3] = (byte)(value >> 24);
            }
            else
            {
                _temp[0] = (byte)(value);
                _temp[1] = (byte)(value >> 8);
                _temp[2] = (byte)(value >> 16);
                _temp[3] = (byte)(value >> 24);
                _buffer.SetBytes(_position, _temp, 0, 4);
            }

            SetPositionAfterWrite(_position + 4);
        }

        /// <inheritdoc/>
        public override void WriteInt64(long value)
        {
            ThrowIfDisposed();

            PrepareToWrite(8);

            var bytes = BitConverter.GetBytes(value);
            _buffer.SetBytes(_position, bytes, 0, 8);

            SetPositionAfterWrite(_position + 8);
        }

        /// <inheritdoc/>
        public override void WriteObjectId(ObjectId value)
        {
            ThrowIfDisposed();

            PrepareToWrite(12);

            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= 12)
            {
                value.ToByteArray(segment.Array, segment.Offset);
            }
            else
            {
                var bytes = value.ToByteArray();
                _buffer.SetBytes(_position, bytes, 0, 12);
            }

            SetPositionAfterWrite(_position + 12);
        }

        /// <inheritdoc/>
        public override void WriteString(string value, UTF8Encoding encoding)
        {
            ThrowIfDisposed();

            var maxLength = encoding.GetMaxByteCount(value.Length) + 5;
            PrepareToWrite(maxLength);

            int actualLength;
            var segment = _buffer.AccessBackingBytes(_position);
            if (segment.Count >= maxLength)
            {
                actualLength = encoding.GetBytes(value, 0, value.Length, segment.Array, segment.Offset + 4);

                var lengthPlusOne = actualLength + 1;
                segment.Array[segment.Offset] = (byte)lengthPlusOne;
                segment.Array[segment.Offset + 1] = (byte)(lengthPlusOne >> 8);
                segment.Array[segment.Offset + 2] = (byte)(lengthPlusOne >> 16);
                segment.Array[segment.Offset + 3] = (byte)(lengthPlusOne >> 24);
                segment.Array[segment.Offset + 4 + actualLength] = 0;
            }
            else
            {
                byte[] bytes;
                if (maxLength <= _tempUtf8.Length)
                {
                    bytes = _tempUtf8;
                    actualLength = encoding.GetBytes(value, 0, value.Length, bytes, 0);
                }
                else
                {
                    bytes = encoding.GetBytes(value);
                    actualLength = bytes.Length;
                }

                var lengthPlusOneBytes = BitConverter.GetBytes(actualLength + 1);

                _buffer.SetBytes(_position, lengthPlusOneBytes, 0, 4);
                _buffer.SetBytes(_position + 4, bytes, 0, actualLength);
                _buffer.SetByte(_position + 4 + actualLength, 0);
            }

            SetPositionAfterWrite(_position + actualLength + 5);
        }
    }
}
