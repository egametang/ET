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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON reader for a BsonDocument.
    /// </summary>
    public class BsonDocumentReader : BsonReader
    {
        // private fields
        private BsonDocumentReaderContext _context;
        private BsonValue _currentValue;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentReader class.
        /// </summary>
        /// <param name="document">A BsonDocument.</param>
        public BsonDocumentReader(BsonDocument document)
            : this(document, BsonDocumentReaderSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDocumentReader class.
        /// </summary>
        /// <param name="document">A BsonDocument.</param>
        /// <param name="settings">The reader settings.</param>
        public BsonDocumentReader(BsonDocument document, BsonDocumentReaderSettings settings)
            : base(settings)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            _context = new BsonDocumentReaderContext(null, ContextType.TopLevel, document);
            _currentValue = document;
        }

        // public methods
        /// <summary>
        /// Closes the reader.
        /// </summary>
        public override void Close()
        {
            // Close can be called on Disposed objects
            State = BsonReaderState.Closed;
        }

        /// <summary>
        /// Gets a bookmark to the reader's current position and state.
        /// </summary>
        /// <returns>A bookmark.</returns>
        public override BsonReaderBookmark GetBookmark()
        {
            return new BsonDocumentReaderBookmark(State, CurrentBsonType, CurrentName, _context, _currentValue);
        }

        /// <summary>
        /// Determines whether this reader is at end of file.
        /// </summary>
        /// <returns>
        /// Whether this reader is at end of file.
        /// </returns>
        public override bool IsAtEndOfFile()
        {
            return State == BsonReaderState.Done;
        }

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A BsonBinaryData.</returns>
        public override BsonBinaryData ReadBinaryData()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBinaryData", BsonType.Binary);

            State = GetNextState();
            return _currentValue.AsBsonBinaryData;
        }

        /// <summary>
        /// Reads a BSON boolean from the reader.
        /// </summary>
        /// <returns>A Boolean.</returns>
        public override bool ReadBoolean()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBoolean", BsonType.Boolean);
            State = GetNextState();
            return _currentValue.AsBoolean;
        }

        /// <summary>
        /// Reads a BsonType from the reader.
        /// </summary>
        /// <returns>A BsonType.</returns>
        public override BsonType ReadBsonType()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            if (State == BsonReaderState.Initial || State == BsonReaderState.ScopeDocument)
            {
                // there is an implied type of Document for the top level and for scope documents
                CurrentBsonType = BsonType.Document;
                State = BsonReaderState.Value;
                return CurrentBsonType;
            }
            if (State != BsonReaderState.Type)
            {
                ThrowInvalidState("ReadBsonType", BsonReaderState.Type);
            }

            switch (_context.ContextType)
            {
                case ContextType.Array:
                    if (!_context.TryGetNextValue(out _currentValue))
                    {
                        State = BsonReaderState.EndOfArray;
                        return BsonType.EndOfDocument;
                    }
                    State = BsonReaderState.Value;
                    break;
                case ContextType.Document:
                    BsonElement currentElement;
                    if (!_context.TryGetNextElement(out currentElement))
                    {
                        State = BsonReaderState.EndOfDocument;
                        return BsonType.EndOfDocument;
                    }
                    CurrentName = currentElement.Name;
                    _currentValue = currentElement.Value;
                    State = BsonReaderState.Name;
                    break;
                default:
                    throw new BsonInternalException("Invalid ContextType.");
            }

            CurrentBsonType = _currentValue.BsonType;
            return CurrentBsonType;
        }

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A byte array.</returns>
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        public override byte[] ReadBytes()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBytes", BsonType.Binary);

            State = GetNextState();
            var binaryData = _currentValue.AsBsonBinaryData;

            var subType = binaryData.SubType;
            if (subType != BsonBinarySubType.Binary && subType != BsonBinarySubType.OldBinary)
            {
                var message = string.Format("ReadBytes requires the binary sub type to be Binary, not {0}.", subType);
                throw new FormatException(message);
            }

            return binaryData.Bytes;
        }
#pragma warning restore 618

        /// <summary>
        /// Reads a BSON DateTime from the reader.
        /// </summary>
        /// <returns>The number of milliseconds since the Unix epoch.</returns>
        public override long ReadDateTime()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadDateTime", BsonType.DateTime);
            State = GetNextState();
            return _currentValue.AsBsonDateTime.MillisecondsSinceEpoch;
        }

        /// <inheritdoc />
        public override Decimal128 ReadDecimal128()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType(nameof(ReadDecimal128), BsonType.Decimal128);
            State = GetNextState();
            return _currentValue.AsDecimal128;
        }

        /// <summary>
        /// Reads a BSON Double from the reader.
        /// </summary>
        /// <returns>A Double.</returns>
        public override double ReadDouble()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadDouble", BsonType.Double);
            State = GetNextState();
            return _currentValue.AsDouble;
        }

        /// <summary>
        /// Reads the end of a BSON array from the reader.
        /// </summary>
        public override void ReadEndArray()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            if (_context.ContextType != ContextType.Array)
            {
                ThrowInvalidContextType("ReadEndArray", _context.ContextType, ContextType.Array);
            }
            if (State == BsonReaderState.Type)
            {
                ReadBsonType(); // will set state to EndOfArray if at end of array
            }
            if (State != BsonReaderState.EndOfArray)
            {
                ThrowInvalidState("ReadEndArray", BsonReaderState.EndOfArray);
            }

            _context = _context.PopContext();
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Done; break;
                default: throw new BsonInternalException("Unexpected ContextType.");
            }
        }

        /// <summary>
        /// Reads the end of a BSON document from the reader.
        /// </summary>
        public override void ReadEndDocument()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            if (_context.ContextType != ContextType.Document && _context.ContextType != ContextType.ScopeDocument)
            {
                ThrowInvalidContextType("ReadEndDocument", _context.ContextType, ContextType.Document, ContextType.ScopeDocument);
            }
            if (State == BsonReaderState.Type)
            {
                ReadBsonType(); // will set state to EndOfDocument if at end of document
            }
            if (State != BsonReaderState.EndOfDocument)
            {
                ThrowInvalidState("ReadEndDocument", BsonReaderState.EndOfDocument);
            }

            _context = _context.PopContext();
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Done; break;
                default: throw new BsonInternalException("Unexpected ContextType.");
            }
        }

        /// <summary>
        /// Reads a BSON Int32 from the reader.
        /// </summary>
        /// <returns>An Int32.</returns>
        public override int ReadInt32()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadInt32", BsonType.Int32);
            State = GetNextState();
            return _currentValue.AsInt32;
        }

        /// <summary>
        /// Reads a BSON Int64 from the reader.
        /// </summary>
        /// <returns>An Int64.</returns>
        public override long ReadInt64()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadInt64", BsonType.Int64);
            State = GetNextState();
            return _currentValue.AsInt64;
        }

        /// <summary>
        /// Reads a BSON JavaScript from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ReadJavaScript()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadJavaScript", BsonType.JavaScript);
            State = GetNextState();
            return _currentValue.AsBsonJavaScript.Code;
        }

        /// <summary>
        /// Reads a BSON JavaScript with scope from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <returns>A string.</returns>
        public override string ReadJavaScriptWithScope()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadJavaScriptWithScope", BsonType.JavaScriptWithScope);

            State = BsonReaderState.ScopeDocument;
            return _currentValue.AsBsonJavaScriptWithScope.Code;
        }

        /// <summary>
        /// Reads a BSON MaxKey from the reader.
        /// </summary>
        public override void ReadMaxKey()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadMaxKey", BsonType.MaxKey);
            State = GetNextState();
        }

        /// <summary>
        /// Reads a BSON MinKey from the reader.
        /// </summary>
        public override void ReadMinKey()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadMinKey", BsonType.MinKey);
            State = GetNextState();
        }

        /// <summary>
        /// Reads the name of an element from the reader.
        /// </summary>
        /// <param name="nameDecoder">The name decoder.</param>
        /// <returns>
        /// The name of the element.
        /// </returns>
        public override string ReadName(INameDecoder nameDecoder)
        {
            if (nameDecoder == null)
            {
                throw new ArgumentNullException("nameDecoder");
            }

            if (Disposed) { ThrowObjectDisposedException(); }
            if (State == BsonReaderState.Type)
            {
                ReadBsonType();
            }
            if (State != BsonReaderState.Name)
            {
                ThrowInvalidState("ReadName", BsonReaderState.Name);
            }

            nameDecoder.Inform(CurrentName);
            State = BsonReaderState.Value;
            return CurrentName;
        }

        /// <summary>
        /// Reads a BSON null from the reader.
        /// </summary>
        public override void ReadNull()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadNull", BsonType.Null);
            State = GetNextState();
        }

        /// <summary>
        /// Reads a BSON ObjectId from the reader.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        public override ObjectId ReadObjectId()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadObjectId", BsonType.ObjectId);
            State = GetNextState();
            return _currentValue.AsObjectId;
        }

        /// <summary>
        /// Reads a BSON regular expression from the reader.
        /// </summary>
        /// <returns>A BsonRegularExpression.</returns>
        public override BsonRegularExpression ReadRegularExpression()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadRegularExpression", BsonType.RegularExpression);
            State = GetNextState();
            return _currentValue.AsBsonRegularExpression;
        }

        /// <summary>
        /// Reads the start of a BSON array.
        /// </summary>
        public override void ReadStartArray()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadStartArray", BsonType.Array);

            var array = _currentValue.AsBsonArray;
            _context = new BsonDocumentReaderContext(_context, ContextType.Array, array);
            State = BsonReaderState.Type;
        }

        /// <summary>
        /// Reads the start of a BSON document.
        /// </summary>
        public override void ReadStartDocument()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadStartDocument", BsonType.Document);

            BsonDocument document;
            var script = _currentValue as BsonJavaScriptWithScope;
            if (script != null)
            {
                document = script.Scope;
            }
            else
            {
                document = _currentValue.AsBsonDocument;
            }
            _context = new BsonDocumentReaderContext(_context, ContextType.Document, document);
            State = BsonReaderState.Type;
        }

        /// <summary>
        /// Reads a BSON string from the reader.
        /// </summary>
        /// <returns>A String.</returns>
        public override string ReadString()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadString", BsonType.String);
            State = GetNextState();
            return _currentValue.AsString;
        }

        /// <summary>
        /// Reads a BSON symbol from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ReadSymbol()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadSymbol", BsonType.Symbol);
            State = GetNextState();
            return _currentValue.AsBsonSymbol.Name;
        }

        /// <summary>
        /// Reads a BSON timestamp from the reader.
        /// </summary>
        /// <returns>The combined timestamp/increment.</returns>
        public override long ReadTimestamp()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadTimestamp", BsonType.Timestamp);
            State = GetNextState();
            return _currentValue.AsBsonTimestamp.Value;
        }

        /// <summary>
        /// Reads a BSON undefined from the reader.
        /// </summary>
        public override void ReadUndefined()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadUndefined", BsonType.Undefined);
            State = GetNextState();
        }

        /// <summary>
        /// Returns the reader to previously bookmarked position and state.
        /// </summary>
        /// <param name="bookmark">The bookmark.</param>
        public override void ReturnToBookmark(BsonReaderBookmark bookmark)
        {
            var documentReaderBookmark = (BsonDocumentReaderBookmark)bookmark;
            State = documentReaderBookmark.State;
            CurrentBsonType = documentReaderBookmark.CurrentBsonType;
            CurrentName = documentReaderBookmark.CurrentName;
            _context = documentReaderBookmark.CloneContext();
            _currentValue = documentReaderBookmark.CurrentValue;
        }

        /// <summary>
        /// Skips the name (reader must be positioned on a name).
        /// </summary>
        public override void SkipName()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            if (State != BsonReaderState.Name)
            {
                ThrowInvalidState("SkipName", BsonReaderState.Name);
            }

            State = BsonReaderState.Value;
        }

        /// <summary>
        /// Skips the value (reader must be positioned on a value).
        /// </summary>
        public override void SkipValue()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            if (State != BsonReaderState.Value)
            {
                ThrowInvalidState("SkipValue", BsonReaderState.Value);
            }
            State = BsonReaderState.Type;
        }

        // protected methods
        /// <summary>
        /// Disposes of any resources used by the reader.
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
        private BsonReaderState GetNextState()
        {
            switch (_context.ContextType)
            {
                case ContextType.Array:
                case ContextType.Document:
                    return BsonReaderState.Type;
                case ContextType.TopLevel:
                    return BsonReaderState.Done;
                default:
                    throw new BsonInternalException("Unexpected ContextType.");
            }
        }
    }
}
