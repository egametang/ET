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
using System.Collections.Generic;
using System.IO;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON writer to a BSON Stream.
    /// </summary>
    public class BsonBinaryWriter : BsonWriter
    {
        // private fields
        private readonly Stream _baseStream;
        private readonly BsonStream _bsonStream;
        private readonly BsonBinaryWriterSettings _settings; // same value as in base class just declared as derived class
        private readonly Stack<int> _maxDocumentSizeStack = new Stack<int>();
        private BsonBinaryWriterContext _context;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryWriter class.
        /// </summary>
        /// <param name="stream">A stream. The BsonBinaryWriter does not own the stream and will not Dispose it.</param>
        public BsonBinaryWriter(Stream stream)
            : this(stream, BsonBinaryWriterSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryWriter class.
        /// </summary>
        /// <param name="stream">A stream. The BsonBinaryWriter does not own the stream and will not Dispose it.</param>
        /// <param name="settings">The BsonBinaryWriter settings.</param>
        public BsonBinaryWriter(Stream stream, BsonBinaryWriterSettings settings)
            : base(settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanSeek)
            {
                throw new ArgumentException("The stream must be capable of seeking.", "stream");
            }

            _baseStream = stream;
            _bsonStream = (stream as BsonStream) ?? new BsonStreamAdapter(stream);
            _settings = settings; // already frozen by base class
            _maxDocumentSizeStack.Push(_settings.MaxDocumentSize);

            _context = null;
            State = BsonWriterState.Initial;
        }

        // public properties
        /// <summary>
        /// Gets the base stream.
        /// </summary>
        /// <value>
        /// The base stream.
        /// </value>
        public Stream BaseStream
        {
            get { return _baseStream; }
        }

        /// <summary>
        /// Gets the BSON stream.
        /// </summary>
        /// <value>
        /// The BSON stream.
        /// </value>
        public BsonStream BsonStream
        {
            get { return _bsonStream; }
        }

        // public methods
        /// <summary>
        /// Closes the writer. Also closes the base stream.
        /// </summary>
        public override void Close()
        {
            // Close can be called on Disposed objects
            if (State != BsonWriterState.Closed)
            {
                if (State == BsonWriterState.Done)
                {
                    Flush();
                }
                _context = null;
                State = BsonWriterState.Closed;
            }
        }

        /// <summary>
        /// Flushes any pending data to the output destination.
        /// </summary>
        public override void Flush()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State == BsonWriterState.Closed)
            {
                throw new InvalidOperationException("Flush called on closed BsonWriter.");
            }
            if (State != BsonWriterState.Done)
            {
                throw new InvalidOperationException("Flush called before BsonBinaryWriter was finished writing to buffer.");
            }
            _bsonStream.Flush();
        }

        /// <summary>
        /// Pops the max document size stack, restoring the previous max document size.
        /// </summary>
        public void PopMaxDocumentSize()
        {
            _maxDocumentSizeStack.Pop();
        }

        /// <summary>
        /// Pushes a new max document size onto the max document size stack.
        /// </summary>
        /// <param name="maxDocumentSize">The maximum size of the document.</param>
        public void PushMaxDocumentSize(int maxDocumentSize)
        {
            _maxDocumentSizeStack.Push(Math.Min(maxDocumentSize, _maxDocumentSizeStack.Peek()));
        }

#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        public override void WriteBinaryData(BsonBinaryData binaryData)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBinaryData", BsonWriterState.Value);
            }

            var bytes = binaryData.Bytes;
            var subType = binaryData.SubType;
            var guidRepresentation = binaryData.GuidRepresentation;
            switch (subType)
            {
                case BsonBinarySubType.OldBinary:
                    if (_settings.FixOldBinarySubTypeOnOutput)
                    {
                        subType = BsonBinarySubType.Binary; // replace obsolete OldBinary with new Binary sub type
                    }
                    break;
                case BsonBinarySubType.UuidLegacy:
                case BsonBinarySubType.UuidStandard:
                    if (_settings.GuidRepresentation != GuidRepresentation.Unspecified)
                    {
                        var expectedSubType = (_settings.GuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                        if (subType != expectedSubType)
                        {
                            var message = string.Format(
                                "The GuidRepresentation for the writer is {0}, which requires the subType argument to be {1}, not {2}.",
                                _settings.GuidRepresentation, expectedSubType, subType);
                            throw new BsonSerializationException(message);
                        }
                        if (guidRepresentation != _settings.GuidRepresentation)
                        {
                            var message = string.Format(
                                "The GuidRepresentation for the writer is {0}, which requires the the guidRepresentation argument to also be {0}, not {1}.",
                                _settings.GuidRepresentation, guidRepresentation);
                            throw new BsonSerializationException(message);
                        }
                    }
                    break;
            }

            _bsonStream.WriteBsonType(BsonType.Binary);
            WriteNameHelper();
            if (subType == BsonBinarySubType.OldBinary)
            {
                // sub type OldBinary has two sizes (for historical reasons)
                _bsonStream.WriteInt32(bytes.Length + 4);
                _bsonStream.WriteBinarySubType(subType);
                _bsonStream.WriteInt32(bytes.Length);
            }
            else
            {
                _bsonStream.WriteInt32(bytes.Length);
                _bsonStream.WriteBinarySubType(subType);
            }
            _bsonStream.WriteBytes(bytes, 0, bytes.Length);

            State = GetNextState();
        }
#pragma warning restore 618

        /// <summary>
        /// Writes a BSON Boolean to the writer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        public override void WriteBoolean(bool value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBoolean", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Boolean);
            WriteNameHelper();
            _bsonStream.WriteBoolean(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public override void WriteBytes(byte[] bytes)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBytes", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Binary);
            WriteNameHelper();
            _bsonStream.WriteInt32(bytes.Length);
            _bsonStream.WriteBinarySubType(BsonBinarySubType.Binary);
            _bsonStream.WriteBytes(bytes, 0, bytes.Length);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON DateTime to the writer.
        /// </summary>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        public override void WriteDateTime(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteDateTime", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.DateTime);
            WriteNameHelper();
            _bsonStream.WriteInt64(value);

            State = GetNextState();
        }

        /// <inheritdoc />
        public override void WriteDecimal128(Decimal128 value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState(nameof(WriteDecimal128), BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Decimal128);
            WriteNameHelper();
            _bsonStream.WriteDecimal128(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Double to the writer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        public override void WriteDouble(double value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteDouble", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Double);
            WriteNameHelper();
            _bsonStream.WriteDouble(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON array to the writer.
        /// </summary>
        public override void WriteEndArray()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteEndArray", BsonWriterState.Value);
            }
            if (_context.ContextType != ContextType.Array)
            {
                ThrowInvalidContextType("WriteEndArray", _context.ContextType, ContextType.Array);
            }

            base.WriteEndArray();
            _bsonStream.WriteByte(0);
            BackpatchSize(); // size of document

            _context = _context.ParentContext;
            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON document to the writer.
        /// </summary>
        public override void WriteEndDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Name)
            {
                ThrowInvalidState("WriteEndDocument", BsonWriterState.Name);
            }
            if (_context.ContextType != ContextType.Document && _context.ContextType != ContextType.ScopeDocument)
            {
                ThrowInvalidContextType("WriteEndDocument", _context.ContextType, ContextType.Document, ContextType.ScopeDocument);
            }

            base.WriteEndDocument();
            _bsonStream.WriteByte(0);
            BackpatchSize(); // size of document

            _context = _context.ParentContext;
            if (_context == null)
            {
                State = BsonWriterState.Done;
            }
            else
            {
                if (_context.ContextType == ContextType.JavaScriptWithScope)
                {
                    BackpatchSize(); // size of the JavaScript with scope value
                    _context = _context.ParentContext;
                }
                State = GetNextState();
            }
        }

        /// <summary>
        /// Writes a BSON Int32 to the writer.
        /// </summary>
        /// <param name="value">The Int32 value.</param>
        public override void WriteInt32(int value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteInt32", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Int32);
            WriteNameHelper();
            _bsonStream.WriteInt32(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Int64 to the writer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        public override void WriteInt64(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteInt64", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Int64);
            WriteNameHelper();
            _bsonStream.WriteInt64(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScript(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteJavaScript", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.JavaScript);
            WriteNameHelper();
            _bsonStream.WriteString(code, _settings.Encoding);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScriptWithScope(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteJavaScriptWithScope", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.JavaScriptWithScope);
            WriteNameHelper();
            _context = new BsonBinaryWriterContext(_context, ContextType.JavaScriptWithScope, _bsonStream.Position);
            _bsonStream.WriteInt32(0); // reserve space for size of JavaScript with scope value
            _bsonStream.WriteString(code, _settings.Encoding);

            State = BsonWriterState.ScopeDocument;
        }

        /// <summary>
        /// Writes a BSON MaxKey to the writer.
        /// </summary>
        public override void WriteMaxKey()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteMaxKey", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.MaxKey);
            WriteNameHelper();

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON MinKey to the writer.
        /// </summary>
        public override void WriteMinKey()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteMinKey", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.MinKey);
            WriteNameHelper();

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON null to the writer.
        /// </summary>
        public override void WriteNull()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteNull", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Null);
            WriteNameHelper();

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON ObjectId to the writer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        public override void WriteObjectId(ObjectId objectId)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteObjectId", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.ObjectId);
            WriteNameHelper();
            _bsonStream.WriteObjectId(objectId);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a raw BSON array.
        /// </summary>
        /// <param name="slice">The byte buffer containing the raw BSON array.</param>
        public override void WriteRawBsonArray(IByteBuffer slice)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteRawBsonArray", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Array);
            WriteNameHelper();
            _bsonStream.WriteSlice(slice); // assumes slice is a valid raw BSON array

            State = GetNextState();
        }

        /// <summary>
        /// Writes a raw BSON document.
        /// </summary>
        /// <param name="slice">The byte buffer containing the raw BSON document.</param>
        public override void WriteRawBsonDocument(IByteBuffer slice)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Initial && State != BsonWriterState.Value && State != BsonWriterState.ScopeDocument && State != BsonWriterState.Done)
            {
                ThrowInvalidState("WriteRawBsonDocument", BsonWriterState.Initial, BsonWriterState.Value, BsonWriterState.ScopeDocument, BsonWriterState.Done);
            }

            if (State == BsonWriterState.Value)
            {
                _bsonStream.WriteBsonType(BsonType.Document);
                WriteNameHelper();
            }
            _bsonStream.WriteSlice(slice); // assumes byteBuffer is a valid raw BSON document

            if (_context == null)
            {
                State = BsonWriterState.Done;
            }
            else
            {
                if (_context.ContextType == ContextType.JavaScriptWithScope)
                {
                    BackpatchSize(); // size of the JavaScript with scope value
                    _context = _context.ParentContext;
                }
                State = GetNextState();
            }
        }

        /// <summary>
        /// Writes a BSON regular expression to the writer.
        /// </summary>
        /// <param name="regex">A BsonRegularExpression.</param>
        public override void WriteRegularExpression(BsonRegularExpression regex)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteRegularExpression", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.RegularExpression);
            WriteNameHelper();
            _bsonStream.WriteCString(regex.Pattern);
            _bsonStream.WriteCString(regex.Options);

            State = GetNextState();
        }

        /// <summary>
        /// Writes the start of a BSON array to the writer.
        /// </summary>
        public override void WriteStartArray()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteStartArray", BsonWriterState.Value);
            }

            base.WriteStartArray();
            _bsonStream.WriteBsonType(BsonType.Array);
            WriteNameHelper();
            _context = new BsonBinaryWriterContext(_context, ContextType.Array, _bsonStream.Position);
            _bsonStream.WriteInt32(0); // reserve space for size

            State = BsonWriterState.Value;
        }

        /// <summary>
        /// Writes the start of a BSON document to the writer.
        /// </summary>
        public override void WriteStartDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Initial && State != BsonWriterState.Value && State != BsonWriterState.ScopeDocument && State != BsonWriterState.Done)
            {
                ThrowInvalidState("WriteStartDocument", BsonWriterState.Initial, BsonWriterState.Value, BsonWriterState.ScopeDocument, BsonWriterState.Done);
            }

            base.WriteStartDocument();
            if (State == BsonWriterState.Value)
            {
                _bsonStream.WriteBsonType(BsonType.Document);
                WriteNameHelper();
            }
            var contextType = (State == BsonWriterState.ScopeDocument) ? ContextType.ScopeDocument : ContextType.Document;
            _context = new BsonBinaryWriterContext(_context, contextType, _bsonStream.Position);
            _bsonStream.WriteInt32(0); // reserve space for size

            State = BsonWriterState.Name;
        }

        /// <summary>
        /// Writes a BSON String to the writer.
        /// </summary>
        /// <param name="value">The String value.</param>
        public override void WriteString(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteString", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.String);
            WriteNameHelper();
            _bsonStream.WriteString(value, _settings.Encoding);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Symbol to the writer.
        /// </summary>
        /// <param name="value">The symbol.</param>
        public override void WriteSymbol(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteSymbol", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Symbol);
            WriteNameHelper();
            _bsonStream.WriteString(value, _settings.Encoding);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON timestamp to the writer.
        /// </summary>
        /// <param name="value">The combined timestamp/increment value.</param>
        public override void WriteTimestamp(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteTimestamp", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Timestamp);
            WriteNameHelper();
            _bsonStream.WriteInt64(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON undefined to the writer.
        /// </summary>
        public override void WriteUndefined()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonBinaryWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteUndefined", BsonWriterState.Value);
            }

            _bsonStream.WriteBsonType(BsonType.Undefined);
            WriteNameHelper();

            State = GetNextState();
        }

        // protected methods
        /// <summary>
        /// Disposes of any resources used by the writer.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Close();
                }
                catch { } // ignore exceptions
            }
            base.Dispose(disposing);
        }

        // private methods
        private void BackpatchSize()
        {
            var size = _bsonStream.Position - _context.StartPosition;
            if (size > _maxDocumentSizeStack.Peek())
            {
                var message = string.Format("Size {0} is larger than MaxDocumentSize {1}.", size, _maxDocumentSizeStack.Peek());
                throw new FormatException(message);
            }

            var currentPosition = _bsonStream.Position;
            _bsonStream.Position = _context.StartPosition;
            _bsonStream.WriteInt32((int)size);
            _bsonStream.Position = currentPosition;
        }

        private BsonWriterState GetNextState()
        {
            if (_context.ContextType == ContextType.Array)
            {
                return BsonWriterState.Value;
            }
            else
            {
                return BsonWriterState.Name;
            }
        }

        private void WriteNameHelper()
        {
            if (_context.ContextType == ContextType.Array)
            {
                var index = _context.Index++;
                var nameBytes = ArrayElementNameAccelerator.Default.GetElementNameBytes(index);
                _bsonStream.WriteCStringBytes(nameBytes);
            }
            else
            {
                _bsonStream.WriteCString(Name);
            }
        }
    }
}
