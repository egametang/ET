/* Copyright 2017-present MongoDB Inc.
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
    /// An abstract base class for an IBsonWriter that wraps another IBsonWriter.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.IO.IBsonWriter" />
    public abstract class WrappingBsonWriter : IBsonWriter
    {
        // private fields
        private bool _disposed;
        private readonly IBsonWriter _wrapped;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WrappingBsonWriter"/> class.
        /// </summary>
        /// <param name="wrapped">The wrapped writer.</param>
        public WrappingBsonWriter(IBsonWriter wrapped)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException(nameof(wrapped));
            }
            _wrapped = wrapped;
        }

        // public properties
        /// <inheritdoc />
        public virtual long Position
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Position;
            }
        }

        /// <inheritdoc />
        public virtual int SerializationDepth
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.SerializationDepth;
            }
        }

        /// <inheritdoc />
        public virtual BsonWriterSettings Settings
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.Settings;
            }
        }

        /// <inheritdoc />
        public virtual BsonWriterState State
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped.State;
            }
        }

        /// <summary>
        /// Gets the wrapped writer.
        /// </summary>
        /// <value>
        /// The wrapped writer.
        /// </value>
        public IBsonWriter Wrapped
        {
            get
            {
                ThrowIfDisposed();
                return _wrapped;
            }
        }

        // public methods
        /// <inheritdoc />
        public virtual void Close()
        {
            // let subclass decide whether to throw or not if Dispose has been called
            _wrapped.Close();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual void Flush()
        {
            ThrowIfDisposed();
            _wrapped.Flush();
        }

        /// <inheritdoc />
        public virtual void PopElementNameValidator()
        {
            ThrowIfDisposed();
            _wrapped.PopElementNameValidator();
        }

        /// <inheritdoc />
        public virtual void PopSettings()
        {
            ThrowIfDisposed();
            _wrapped.PopSettings();
        }

        /// <inheritdoc />
        public virtual void PushElementNameValidator(IElementNameValidator validator)
        {
            ThrowIfDisposed();
            _wrapped.PushElementNameValidator(validator);
        }

        /// <inheritdoc />
        public virtual void PushSettings(Action<BsonWriterSettings> configurator)
        {
            ThrowIfDisposed();
            _wrapped.PushSettings(configurator);
        }

        /// <inheritdoc />
        public virtual void WriteBinaryData(BsonBinaryData binaryData)
        {
            ThrowIfDisposed();
            _wrapped.WriteBinaryData(binaryData);
        }

        /// <inheritdoc />
        public virtual void WriteBoolean(bool value)
        {
            ThrowIfDisposed();
            _wrapped.WriteBoolean(value);
        }

        /// <inheritdoc />
        public virtual void WriteBytes(byte[] bytes)
        {
            ThrowIfDisposed();
            _wrapped.WriteBytes(bytes);
        }

        /// <inheritdoc />
        public virtual void WriteDateTime(long value)
        {
            ThrowIfDisposed();
            _wrapped.WriteDateTime(value);
        }

        /// <inheritdoc />
        public virtual void WriteDecimal128(Decimal128 value)
        {
            ThrowIfDisposed();
            _wrapped.WriteDecimal128(value);
        }

        /// <inheritdoc />
        public virtual void WriteDouble(double value)
        {
            ThrowIfDisposed();
            _wrapped.WriteDouble(value);
        }

        /// <inheritdoc />
        public virtual void WriteEndArray()
        {
            ThrowIfDisposed();
            _wrapped.WriteEndArray();
        }

        /// <inheritdoc />
        public virtual void WriteEndDocument()
        {
            ThrowIfDisposed();
            _wrapped.WriteEndDocument();
        }

        /// <inheritdoc />
        public virtual void WriteInt32(int value)
        {
            ThrowIfDisposed();
            _wrapped.WriteInt32(value);
        }

        /// <inheritdoc />
        public virtual void WriteInt64(long value)
        {
            ThrowIfDisposed();
            _wrapped.WriteInt64(value);
        }

        /// <inheritdoc />
        public virtual void WriteJavaScript(string code)
        {
            ThrowIfDisposed();
            _wrapped.WriteJavaScript(code);
        }

        /// <inheritdoc />
        public virtual void WriteJavaScriptWithScope(string code)
        {
            ThrowIfDisposed();
            _wrapped.WriteJavaScriptWithScope(code);
        }

        /// <inheritdoc />
        public virtual void WriteMaxKey()
        {
            ThrowIfDisposed();
            _wrapped.WriteMaxKey();
        }

        /// <inheritdoc />
        public virtual void WriteMinKey()
        {
            ThrowIfDisposed();
            _wrapped.WriteMinKey();
        }

        /// <inheritdoc />
        public virtual void WriteName(string name)
        {
            ThrowIfDisposed();
            _wrapped.WriteName(name);
        }

        /// <inheritdoc />
        public virtual void WriteNull()
        {
            ThrowIfDisposed();
            _wrapped.WriteNull();
        }

        /// <inheritdoc />
        public virtual void WriteObjectId(ObjectId objectId)
        {
            ThrowIfDisposed();
            _wrapped.WriteObjectId(objectId);
        }

        /// <inheritdoc />
        public virtual void WriteRawBsonArray(IByteBuffer slice)
        {
            ThrowIfDisposed();
            _wrapped.WriteRawBsonArray(slice);
        }

        /// <inheritdoc />
        public virtual void WriteRawBsonDocument(IByteBuffer slice)
        {
            ThrowIfDisposed();
            _wrapped.WriteRawBsonDocument(slice);
        }

        /// <inheritdoc />
        public virtual void WriteRegularExpression(BsonRegularExpression regex)
        {
            ThrowIfDisposed();
            _wrapped.WriteRegularExpression(regex);
        }

        /// <inheritdoc />
        public virtual void WriteStartArray()
        {
            ThrowIfDisposed();
            _wrapped.WriteStartArray();
        }

        /// <inheritdoc />
        public virtual void WriteStartDocument()
        {
            ThrowIfDisposed();
            _wrapped.WriteStartDocument();
        }

        /// <inheritdoc />
        public virtual void WriteString(string value)
        {
            ThrowIfDisposed();
            _wrapped.WriteString(value);
        }

        /// <inheritdoc />
        public virtual void WriteSymbol(string value)
        {
            ThrowIfDisposed();
            _wrapped.WriteSymbol(value);
        }

        /// <inheritdoc />
        public virtual void WriteTimestamp(long value)
        {
            ThrowIfDisposed();
            _wrapped.WriteTimestamp(value);
        }

        /// <inheritdoc />
        public virtual void WriteUndefined()
        {
            ThrowIfDisposed();
            _wrapped.WriteUndefined();
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _wrapped.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
