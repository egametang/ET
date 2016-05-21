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
using System.IO;
using System.Text;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON writer to a BSON Stream.
    /// </summary>
    public class BsonBinaryWriter : BsonWriter
    {
        // private static fields
        private static readonly UTF8Encoding __strictUtf8Encoding = new UTF8Encoding(false, true);

        // private fields
        private Stream _stream; // can be null if we're only writing to the buffer
        private BsonBuffer _buffer;
        private bool _disposeBuffer;
        private BsonBinaryWriterSettings _binaryWriterSettings; // same value as in base class just declared as derived class
        private Stack<int> _maxDocumentSizeStack = new Stack<int>();
        private BsonBinaryWriterContext _context;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryWriter class.
        /// </summary>
        /// <param name="stream">A stream.</param>
        /// <param name="buffer">A BsonBuffer.</param>
        /// <param name="settings">Optional BsonBinaryWriter settings.</param>
        public BsonBinaryWriter(Stream stream, BsonBuffer buffer, BsonBinaryWriterSettings settings)
            : this(buffer ?? new BsonBuffer(), buffer == null, settings)
        {
            _stream = stream;
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryWriter class.
        /// </summary>
        /// <param name="buffer">A BsonBuffer.</param>
        /// <param name="disposeBuffer">if set to <c>true</c> this BsonBinaryReader will own the buffer and when Dispose is called the buffer will be Disposed also.</param>
        /// <param name="settings">Optional BsonBinaryWriter settings.</param>
        /// <exception cref="System.ArgumentNullException">
        /// encoder
        /// or
        /// settings
        /// </exception>
        public BsonBinaryWriter(BsonBuffer buffer, bool disposeBuffer, BsonBinaryWriterSettings settings)
            : base(settings)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("encoder");
            }

            _buffer = buffer;
            _disposeBuffer = disposeBuffer;
            _binaryWriterSettings = settings; // already frozen by base class
            _maxDocumentSizeStack.Push(_binaryWriterSettings.MaxDocumentSize);

            _context = null;
            State = BsonWriterState.Initial;
        }

        // public properties
        /// <summary>
        /// Gets the writer's BsonBuffer.
        /// </summary>
        public BsonBuffer Buffer
        {
            get { return _buffer; }
        }

        // public methods
        /// <summary>
        /// Closes the writer.
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
                if (_stream != null && _binaryWriterSettings.CloseOutput)
                {
                    _stream.Close();
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
            if (_stream != null)
            {
                _buffer.WriteTo(_stream);
                _stream.Flush();
                _buffer.Clear(); // only clear the buffer if we have written it to a stream
            }
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
                    if (_binaryWriterSettings.FixOldBinarySubTypeOnOutput)
                    {
                        subType = BsonBinarySubType.Binary; // replace obsolete OldBinary with new Binary sub type
                    }
                    break;
                case BsonBinarySubType.UuidLegacy:
                case BsonBinarySubType.UuidStandard:
                    if (_binaryWriterSettings.GuidRepresentation != GuidRepresentation.Unspecified)
                    {
                        var expectedSubType = (_binaryWriterSettings.GuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                        if (subType != expectedSubType)
                        {
                            var message = string.Format(
                                "The GuidRepresentation for the writer is {0}, which requires the subType argument to be {1}, not {2}.",
                                _binaryWriterSettings.GuidRepresentation, expectedSubType, subType);
                            throw new BsonSerializationException(message);
                        }
                        if (guidRepresentation != _binaryWriterSettings.GuidRepresentation)
                        {
                            var message = string.Format(
                                "The GuidRepresentation for the writer is {0}, which requires the the guidRepresentation argument to also be {0}, not {1}.",
                                _binaryWriterSettings.GuidRepresentation, guidRepresentation);
                            throw new BsonSerializationException(message);
                        }
                    }
                    break;
            }

            _buffer.WriteByte((byte)BsonType.Binary);
            WriteNameHelper();
            if (subType == BsonBinarySubType.OldBinary)
            {
                // sub type OldBinary has two sizes (for historical reasons)
                _buffer.WriteInt32(bytes.Length + 4);
                _buffer.WriteByte((byte)subType);
                _buffer.WriteInt32(bytes.Length);
            }
            else
            {
                _buffer.WriteInt32(bytes.Length);
                _buffer.WriteByte((byte)subType);
            }
            _buffer.WriteBytes(bytes);

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

            _buffer.WriteByte((byte)BsonType.Boolean);
            WriteNameHelper();
            _buffer.WriteBoolean(value);

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

            _buffer.WriteByte((byte)BsonType.Binary);
            WriteNameHelper();
            _buffer.WriteInt32(bytes.Length);
            _buffer.WriteByte((byte)BsonBinarySubType.Binary);
            _buffer.WriteBytes(bytes);

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

            _buffer.WriteByte((byte)BsonType.DateTime);
            WriteNameHelper();
            _buffer.WriteInt64(value);

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

            _buffer.WriteByte((byte)BsonType.Double);
            WriteNameHelper();
            _buffer.WriteDouble(value);

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
            _buffer.WriteByte(0);
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
            _buffer.WriteByte(0);
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

            _buffer.WriteByte((byte)BsonType.Int32);
            WriteNameHelper();
            _buffer.WriteInt32(value);

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

            _buffer.WriteByte((byte)BsonType.Int64);
            WriteNameHelper();
            _buffer.WriteInt64(value);

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

            _buffer.WriteByte((byte)BsonType.JavaScript);
            WriteNameHelper();
            _buffer.WriteString(_binaryWriterSettings.Encoding, code);

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

            _buffer.WriteByte((byte)BsonType.JavaScriptWithScope);
            WriteNameHelper();
            _context = new BsonBinaryWriterContext(_context, ContextType.JavaScriptWithScope, _buffer.Position);
            _buffer.WriteInt32(0); // reserve space for size of JavaScript with scope value
            _buffer.WriteString(_binaryWriterSettings.Encoding, code);

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

            _buffer.WriteByte((byte)BsonType.MaxKey);
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

            _buffer.WriteByte((byte)BsonType.MinKey);
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

            _buffer.WriteByte((byte)BsonType.Null);
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

            _buffer.WriteByte((byte)BsonType.ObjectId);
            WriteNameHelper();
            _buffer.WriteObjectId(objectId);

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

            _buffer.WriteByte((byte)BsonType.Array);
            WriteNameHelper();
            _buffer.ByteBuffer.WriteBytes(slice); // assumes byteBuffer is a valid raw BSON array

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
                _buffer.WriteByte((byte)BsonType.Document);
                WriteNameHelper();
            }
            _buffer.ByteBuffer.WriteBytes(slice); // assumes byteBuffer is a valid raw BSON document

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

            _buffer.WriteByte((byte)BsonType.RegularExpression);
            WriteNameHelper();
            _buffer.WriteCString(_binaryWriterSettings.Encoding, regex.Pattern);
            _buffer.WriteCString(_binaryWriterSettings.Encoding, regex.Options);

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
            _buffer.WriteByte((byte)BsonType.Array);
            WriteNameHelper();
            _context = new BsonBinaryWriterContext(_context, ContextType.Array, _buffer.Position);
            _buffer.WriteInt32(0); // reserve space for size

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
                _buffer.WriteByte((byte)BsonType.Document);
                WriteNameHelper();
            }
	        _context = new BsonBinaryWriterContext(_context, ContextType.Document, _buffer.Position);
            _buffer.WriteInt32(0); // reserve space for size

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

            _buffer.WriteByte((byte)BsonType.String);
            WriteNameHelper();
            _buffer.WriteString(_binaryWriterSettings.Encoding, value);

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

            _buffer.WriteByte((byte)BsonType.Symbol);
            WriteNameHelper();
            _buffer.WriteString(_binaryWriterSettings.Encoding, value);

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

            _buffer.WriteByte((byte)BsonType.Timestamp);
            WriteNameHelper();
            _buffer.WriteInt64(value);

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

            _buffer.WriteByte((byte)BsonType.Undefined);
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
                Close();
                if (_buffer != null)
                {
                    if (_disposeBuffer)
                    {
                        _buffer.Dispose();
                    }
                    _buffer = null;
                }
            }
            base.Dispose(disposing);
        }

        // private methods
        private void BackpatchSize()
        {
            int size = _buffer.Position - _context.StartPosition;
            if (size > _maxDocumentSizeStack.Peek())
            {
                var message = string.Format("Size {0} is larger than MaxDocumentSize {1}.", size, _maxDocumentSizeStack.Peek());
                throw new Exception(message);
            }
            _buffer.Backpatch(_context.StartPosition, size);
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
            string name;
            if (_context.ContextType == ContextType.Array)
            {
                name = (_context.Index++).ToString();
            }
            else
            {
                name = Name;
            }

            _buffer.WriteCString(__strictUtf8Encoding, name);
        }
    }
}
