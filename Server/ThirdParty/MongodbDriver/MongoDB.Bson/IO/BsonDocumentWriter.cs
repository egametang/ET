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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON writer to a BsonDocument.
    /// </summary>
    public class BsonDocumentWriter : BsonWriter
    {
        // private fields
        private BsonDocument _document;
        private BsonDocumentWriterContext _context;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentWriter class.
        /// </summary>
        /// <param name="document">The document to write to (normally starts out as an empty document).</param>
        public BsonDocumentWriter(BsonDocument document)
            : this(document, BsonDocumentWriterSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentWriter class.
        /// </summary>
        /// <param name="document">The document to write to (normally starts out as an empty document).</param>
        /// <param name="settings">The settings.</param>
        public BsonDocumentWriter(BsonDocument document, BsonDocumentWriterSettings settings)
            : base(settings)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            _document = document;
            _context = null;
            State = BsonWriterState.Initial;
        }

        // public properties
        /// <summary>
        /// Gets the BsonDocument being written to.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        // public methods
        /// <summary>
        /// Closes the writer.
        /// </summary>
        public override void Close()
        {
            // Close can be called on Disposed objects
            _context = null;
            State = BsonWriterState.Closed;
        }

        /// <summary>
        /// Flushes any pending data to the output destination.
        /// </summary>
        public override void Flush()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        public override void WriteBinaryData(BsonBinaryData binaryData)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBinaryData", BsonWriterState.Value);
            }

            WriteValue(binaryData);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Boolean to the writer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        public override void WriteBoolean(bool value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBoolean", BsonWriterState.Value);
            }

            WriteValue(value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public override void WriteBytes(byte[] bytes)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteBytes", BsonWriterState.Value);
            }

            WriteValue(new BsonBinaryData(bytes, BsonBinarySubType.Binary));
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON DateTime to the writer.
        /// </summary>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        public override void WriteDateTime(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteDateTime", BsonWriterState.Value);
            }

            WriteValue(new BsonDateTime(value));
            State = GetNextState();
        }

        /// <inheritdoc />
        public override void WriteDecimal128(Decimal128 value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState(nameof(WriteDecimal128), BsonWriterState.Value);
            }

            WriteValue(new BsonDecimal128(value));
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Double to the writer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        public override void WriteDouble(double value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteDouble", BsonWriterState.Value);
            }

            WriteValue(value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON array to the writer.
        /// </summary>
        public override void WriteEndArray()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteEndArray", BsonWriterState.Value);
            }
            if (_context.ContextType != ContextType.Array)
            {
                ThrowInvalidContextType("WriteEndArray", _context.ContextType, ContextType.Array);
            }

            base.WriteEndArray();
            var array = _context.Array;
            _context = _context.ParentContext;
            WriteValue(array);
            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON document to the writer.
        /// </summary>
        public override void WriteEndDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Name)
            {
                ThrowInvalidState("WriteEndDocument", BsonWriterState.Name);
            }
            if (_context.ContextType != ContextType.Document && _context.ContextType != ContextType.ScopeDocument)
            {
                ThrowInvalidContextType("WriteEndDocument", _context.ContextType, ContextType.Document, ContextType.ScopeDocument);
            }

            base.WriteEndDocument();
            if (_context.ContextType == ContextType.ScopeDocument)
            {
                var scope = _context.Document;
                _context = _context.ParentContext;
                var code = _context.Code;
                _context = _context.ParentContext;
                WriteValue(new BsonJavaScriptWithScope(code, scope));
            }
            else
            {
                var document = _context.Document;
                _context = _context.ParentContext;
                if (_context != null)
                {
                    WriteValue(document);
                }
            }

            if (_context == null)
            {
                State = BsonWriterState.Done;
            }
            else
            {
                State = GetNextState();
            }
        }

        /// <summary>
        /// Writes a BSON Int32 to the writer.
        /// </summary>
        /// <param name="value">The Int32 value.</param>
        public override void WriteInt32(int value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteInt32", BsonWriterState.Value);
            }

            WriteValue(value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Int64 to the writer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        public override void WriteInt64(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteInt64", BsonWriterState.Value);
            }

            WriteValue(value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScript(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteJavaScript", BsonWriterState.Value);
            }

            WriteValue(new BsonJavaScript(code));
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScriptWithScope(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteJavaScriptWithScope", BsonWriterState.Value);
            }

            _context = new BsonDocumentWriterContext(_context, ContextType.JavaScriptWithScope, code);
            State = BsonWriterState.ScopeDocument;
        }

        /// <summary>
        /// Writes a BSON MaxKey to the writer.
        /// </summary>
        public override void WriteMaxKey()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteMaxKey", BsonWriterState.Value);
            }

            WriteValue(BsonMaxKey.Value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON MinKey to the writer.
        /// </summary>
        public override void WriteMinKey()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteMinKey", BsonWriterState.Value);
            }

            WriteValue(BsonMinKey.Value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes the name of an element to the writer.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        public override void WriteName(string name)
        {
            base.WriteName(name);
            _context.Name = name;
        }

        /// <summary>
        /// Writes a BSON null to the writer.
        /// </summary>
        public override void WriteNull()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteNull", BsonWriterState.Value);
            }

            WriteValue(BsonNull.Value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON ObjectId to the writer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        public override void WriteObjectId(ObjectId objectId)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteObjectId", BsonWriterState.Value);
            }

            WriteValue(objectId);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON regular expression to the writer.
        /// </summary>
        /// <param name="regex">A BsonRegularExpression.</param>
        public override void WriteRegularExpression(BsonRegularExpression regex)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteRegularExpression", BsonWriterState.Value);
            }

            WriteValue(regex);
            State = GetNextState();
        }

        /// <summary>
        /// Writes the start of a BSON array to the writer.
        /// </summary>
        public override void WriteStartArray()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteStartArray", BsonWriterState.Value);
            }

            base.WriteStartArray();
            _context = new BsonDocumentWriterContext(_context, ContextType.Array, new BsonArray());
            State = BsonWriterState.Value;
        }

        /// <summary>
        /// Writes the start of a BSON document to the writer.
        /// </summary>
        public override void WriteStartDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Initial && State != BsonWriterState.Value && State != BsonWriterState.ScopeDocument && State != BsonWriterState.Done)
            {
                ThrowInvalidState("WriteStartDocument", BsonWriterState.Initial, BsonWriterState.Value, BsonWriterState.ScopeDocument, BsonWriterState.Done);
            }

            base.WriteStartDocument();
            switch (State)
            {
                case BsonWriterState.Initial:
                case BsonWriterState.Done:
                    _context = new BsonDocumentWriterContext(null, ContextType.Document, _document);
                    break;
                case BsonWriterState.Value:
                    _context = new BsonDocumentWriterContext(_context, ContextType.Document, new BsonDocument());
                    break;
                case BsonWriterState.ScopeDocument:
                    _context = new BsonDocumentWriterContext(_context, ContextType.ScopeDocument, new BsonDocument());
                    break;
                default:
                    throw new BsonInternalException("Unexpected state.");
            }

            State = BsonWriterState.Name;
        }

        /// <summary>
        /// Writes a BSON String to the writer.
        /// </summary>
        /// <param name="value">The String value.</param>
        public override void WriteString(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteString", BsonWriterState.Value);
            }

            WriteValue(value);
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Symbol to the writer.
        /// </summary>
        /// <param name="value">The symbol.</param>
        public override void WriteSymbol(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteSymbol", BsonWriterState.Value);
            }

            WriteValue(BsonSymbolTable.Lookup(value));
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON timestamp to the writer.
        /// </summary>
        /// <param name="value">The combined timestamp/increment value.</param>
        public override void WriteTimestamp(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteTimestamp", BsonWriterState.Value);
            }

            WriteValue(new BsonTimestamp(value));
            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON undefined to the writer.
        /// </summary>
        public override void WriteUndefined()
        {
            if (Disposed) { throw new ObjectDisposedException("BsonDocumentWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteUndefined", BsonWriterState.Value);
            }

            WriteValue(BsonUndefined.Value);
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

        private void WriteValue(BsonValue value)
        {
            if (_context.ContextType == ContextType.Array)
            {
                _context.Array.Add(value);
            }
            else
            {
                _context.Document.Add(_context.Name, value);
            }
        }
    }
}
