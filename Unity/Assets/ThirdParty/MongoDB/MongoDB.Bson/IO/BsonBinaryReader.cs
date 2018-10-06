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
using System.Globalization;
using System.IO;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON reader for a binary BSON byte array.
    /// </summary>
    public class BsonBinaryReader : BsonReader
    {
        // private fields
        private readonly Stream _baseStream;
        private readonly BsonStream _bsonStream;
        private readonly BsonBinaryReaderSettings _settings; // same value as in base class just declared as derived class
        private BsonBinaryReaderContext _context;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryReader class.
        /// </summary>
        /// <param name="stream">A stream (BsonBinary does not own the stream and will not Dispose it).</param>
        public BsonBinaryReader(Stream stream)
            : this(stream, BsonBinaryReaderSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryReader class.
        /// </summary>
        /// <param name="stream">A stream (BsonBinary does not own the stream and will not Dispose it).</param>
        /// <param name="settings">A BsonBinaryReaderSettings.</param>
        public BsonBinaryReader(Stream stream, BsonBinaryReaderSettings settings)
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

            _context = new BsonBinaryReaderContext(null, ContextType.TopLevel, 0, 0);
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
            return new BsonBinaryReaderBookmark(State, CurrentBsonType, CurrentName, _context, _bsonStream.Position);
        }

        /// <summary>
        /// Determines whether this reader is at end of file.
        /// </summary>
        /// <returns>
        /// Whether this reader is at end of file.
        /// </returns>
        public override bool IsAtEndOfFile()
        {
            return _bsonStream.Position >= _bsonStream.Length;
        }

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A BsonBinaryData.</returns>
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        public override BsonBinaryData ReadBinaryData()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBinaryData", BsonType.Binary);

            int size = ReadSize();

            var subType = _bsonStream.ReadBinarySubType();
            if (subType == BsonBinarySubType.OldBinary)
            {
                // sub type OldBinary has two sizes (for historical reasons)
                int size2 = ReadSize();
                if (size2 != size - 4)
                {
                    throw new FormatException("Binary sub type OldBinary has inconsistent sizes");
                }
                size = size2;

                if (_settings.FixOldBinarySubTypeOnInput)
                {
                    subType = BsonBinarySubType.Binary; // replace obsolete OldBinary with new Binary sub type
                }
            }

            var bytes = _bsonStream.ReadBytes(size);

            var guidRepresentation = GuidRepresentation.Unspecified;
            if (subType == BsonBinarySubType.UuidLegacy || subType == BsonBinarySubType.UuidStandard)
            {
                if (_settings.GuidRepresentation != GuidRepresentation.Unspecified)
                {
                    var expectedSubType = (_settings.GuidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
                    if (subType != expectedSubType)
                    {
                        var message = string.Format(
                            "The GuidRepresentation for the reader is {0}, which requires the binary sub type to be {1}, not {2}.",
                            _settings.GuidRepresentation, expectedSubType, subType);
                        throw new FormatException(message);
                    }
                }
                guidRepresentation = (subType == BsonBinarySubType.UuidStandard) ? GuidRepresentation.Standard : _settings.GuidRepresentation;
            }

            State = GetNextState();
            return new BsonBinaryData(bytes, subType, guidRepresentation);
        }
#pragma warning restore 618

        /// <summary>
        /// Reads a BSON boolean from the reader.
        /// </summary>
        /// <returns>A Boolean.</returns>
        public override bool ReadBoolean()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBoolean", BsonType.Boolean);
            State = GetNextState();
            return _bsonStream.ReadBoolean();
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

            if (_context.ContextType == ContextType.Array)
            {
                _context.CurrentArrayIndex++;
            }

            try
            {

                CurrentBsonType = _bsonStream.ReadBsonType();
            }
            catch (FormatException ex)
            {
                if (ex.Message.StartsWith("Detected unknown BSON type"))
                {
                    // insert the element name into the error message
                    var periodIndex = ex.Message.IndexOf('.');
                    var dottedElementName = GenerateDottedElementName();
                    var message = ex.Message.Substring(0, periodIndex) + $" for fieldname \"{dottedElementName}\"" + ex.Message.Substring(periodIndex);
                    throw new FormatException(message);
                }
                throw;
            }

            if (CurrentBsonType == BsonType.EndOfDocument)
            {
                switch (_context.ContextType)
                {
                    case ContextType.Array:
                        State = BsonReaderState.EndOfArray;
                        return BsonType.EndOfDocument;
                    case ContextType.Document:
                    case ContextType.ScopeDocument:
                        State = BsonReaderState.EndOfDocument;
                        return BsonType.EndOfDocument;
                    default:
                        var message = string.Format("BsonType EndOfDocument is not valid when ContextType is {0}.", _context.ContextType);
                        throw new FormatException(message);
                }
            }
            else
            {
                switch (_context.ContextType)
                {
                    case ContextType.Array:
                        _bsonStream.SkipCString(); // ignore array element names
                        State = BsonReaderState.Value;
                        break;
                    case ContextType.Document:
                    case ContextType.ScopeDocument:
                        State = BsonReaderState.Name;
                        break;
                    default:
                        throw new BsonInternalException("Unexpected ContextType.");
                }

                return CurrentBsonType;
            }
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

            int size = ReadSize();

            var subType = _bsonStream.ReadBinarySubType();
            if (subType != BsonBinarySubType.Binary && subType != BsonBinarySubType.OldBinary)
            {
                var message = string.Format("ReadBytes requires the binary sub type to be Binary, not {0}.", subType);
                throw new FormatException(message);
            }

            State = GetNextState();
            return _bsonStream.ReadBytes(size);
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
            var value = _bsonStream.ReadInt64();
            if (value == BsonConstants.DateTimeMaxValueMillisecondsSinceEpoch + 1)
            {
                if (_settings.FixOldDateTimeMaxValueOnInput)
                {
                    value = BsonConstants.DateTimeMaxValueMillisecondsSinceEpoch;
                }
            }
            return value;
        }

        /// <inheritdoc />
        public override Decimal128 ReadDecimal128()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType(nameof(ReadDecimal128), BsonType.Decimal128);
            State = GetNextState();
            return _bsonStream.ReadDecimal128();
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
            return _bsonStream.ReadDouble();
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

            _context = _context.PopContext(_bsonStream.Position);
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
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

            _context = _context.PopContext(_bsonStream.Position);
            if (_context.ContextType == ContextType.JavaScriptWithScope)
            {
                _context = _context.PopContext(_bsonStream.Position); // JavaScriptWithScope
            }
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
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
            return _bsonStream.ReadInt32();
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
            return _bsonStream.ReadInt64();
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
            return _bsonStream.ReadString(_settings.Encoding);
        }

        /// <summary>
        /// Reads a BSON JavaScript with scope from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <returns>A string.</returns>
        public override string ReadJavaScriptWithScope()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadJavaScriptWithScope", BsonType.JavaScriptWithScope);

            var startPosition = _bsonStream.Position; // position of size field
            var size = ReadSize();
            _context = new BsonBinaryReaderContext(_context, ContextType.JavaScriptWithScope, startPosition, size);
            var code = _bsonStream.ReadString(_settings.Encoding);

            State = BsonReaderState.ScopeDocument;
            return code;
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
        /// <returns>The name of the element.</returns>
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

            CurrentName = nameDecoder.Decode(_bsonStream, _settings.Encoding);
            State = BsonReaderState.Value;

            if (_context.ContextType == ContextType.Document)
            {
                _context.CurrentElementName = CurrentName;
            }

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
            return _bsonStream.ReadObjectId();
        }

        /// <summary>
        /// Reads a raw BSON array.
        /// </summary>
        /// <returns>
        /// The raw BSON array.
        /// </returns>
        public override IByteBuffer ReadRawBsonArray()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadRawBsonArray", BsonType.Array);

            var slice = _bsonStream.ReadSlice();

            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
                default: throw new BsonInternalException("Unexpected ContextType.");
            }

            return slice;
        }

        /// <summary>
        /// Reads a raw BSON document.
        /// </summary>
        /// <returns>
        /// The raw BSON document.
        /// </returns>
        public override IByteBuffer ReadRawBsonDocument()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadRawBsonDocument", BsonType.Document);

            var slice = _bsonStream.ReadSlice();

            if (_context.ContextType == ContextType.JavaScriptWithScope)
            {
                _context = _context.PopContext(_bsonStream.Position); // JavaScriptWithScope
            }
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
                default: throw new BsonInternalException("Unexpected ContextType.");
            }

            return slice;
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
            var pattern = _bsonStream.ReadCString(_settings.Encoding);
            var options = _bsonStream.ReadCString(_settings.Encoding);
            return new BsonRegularExpression(pattern, options);
        }

        /// <summary>
        /// Reads the start of a BSON array.
        /// </summary>
        public override void ReadStartArray()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadStartArray", BsonType.Array);

            var startPosition = _bsonStream.Position; // position of size field
            var size = ReadSize();
            _context = new BsonBinaryReaderContext(_context, ContextType.Array, startPosition, size);
            State = BsonReaderState.Type;
        }

        /// <summary>
        /// Reads the start of a BSON document.
        /// </summary>
        public override void ReadStartDocument()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadStartDocument", BsonType.Document);

            var contextType = (State == BsonReaderState.ScopeDocument) ? ContextType.ScopeDocument : ContextType.Document;
            var startPosition = _bsonStream.Position; // position of size field
            var size = ReadSize();
            _context = new BsonBinaryReaderContext(_context, contextType, startPosition, size);
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
            return _bsonStream.ReadString(_settings.Encoding);
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
            return _bsonStream.ReadString(_settings.Encoding);
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
            return _bsonStream.ReadInt64();
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
            var binaryReaderBookmark = (BsonBinaryReaderBookmark)bookmark;
            State = binaryReaderBookmark.State;
            CurrentBsonType = binaryReaderBookmark.CurrentBsonType;
            CurrentName = binaryReaderBookmark.CurrentName;
            _context = binaryReaderBookmark.CloneContext();
            _bsonStream.Position = binaryReaderBookmark.Position;
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

            _bsonStream.SkipCString();
            CurrentName = null;
            State = BsonReaderState.Value;

            if (_context.ContextType == ContextType.Document)
            {
                _context.CurrentElementName = CurrentName;
            }
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

            int skip;
            switch (CurrentBsonType)
            {
                case BsonType.Array: skip = ReadSize() - 4; break;
                case BsonType.Binary: skip = ReadSize() + 1; break;
                case BsonType.Boolean: skip = 1; break;
                case BsonType.DateTime: skip = 8; break;
                case BsonType.Document: skip = ReadSize() - 4; break;
                case BsonType.Decimal128: skip = 16; break;
                case BsonType.Double: skip = 8; break;
                case BsonType.Int32: skip = 4; break;
                case BsonType.Int64: skip = 8; break;
                case BsonType.JavaScript: skip = ReadSize(); break;
                case BsonType.JavaScriptWithScope: skip = ReadSize() - 4; break;
                case BsonType.MaxKey: skip = 0; break;
                case BsonType.MinKey: skip = 0; break;
                case BsonType.Null: skip = 0; break;
                case BsonType.ObjectId: skip = 12; break;
                case BsonType.RegularExpression: _bsonStream.SkipCString(); _bsonStream.SkipCString(); skip = 0; break;
                case BsonType.String: skip = ReadSize(); break;
                case BsonType.Symbol: skip = ReadSize(); break;
                case BsonType.Timestamp: skip = 8; break;
                case BsonType.Undefined: skip = 0; break;
                default: throw new BsonInternalException("Unexpected BsonType.");
            }
            _bsonStream.Seek(skip, SeekOrigin.Current);

            State = BsonReaderState.Type;
        }

        // protected methods
        /// <summary>
        /// Disposes of any resources used by the reader.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            // don't Dispose the _stream because we don't own it
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
        private string GenerateDottedElementName()
        {
            string elementName;
            if (_context.ContextType == ContextType.Document)
            {
                try
                {
                    elementName = _bsonStream.ReadCString(Utf8Encodings.Lenient);
                }
                catch
                {
                    elementName = "?"; // ignore exception
                }
            }
            else if (_context.ContextType == ContextType.Array)
            {
                elementName = _context.CurrentArrayIndex.ToString(NumberFormatInfo.InvariantInfo);
            }
            else
            {
                elementName = "?";
            }

            return GenerateDottedElementName(_context.ParentContext, elementName);
        }

        private string GenerateDottedElementName(BsonBinaryReaderContext context, string elementName)
        {
            if (context.ContextType == ContextType.Document)
            {
                return GenerateDottedElementName(context.ParentContext, (context.CurrentElementName ?? "?") + "." + elementName);
            }
            else if (context.ContextType == ContextType.Array)
            {
                var indexElementName = context.CurrentArrayIndex.ToString(NumberFormatInfo.InvariantInfo);
                return GenerateDottedElementName(context.ParentContext, indexElementName + "." + elementName);
            }
            else if (context.ParentContext != null)
            {
                return GenerateDottedElementName(context.ParentContext, "?." + elementName);
            }
            else
            {
                return elementName;
            }
        }

        private BsonReaderState GetNextState()
        {
            switch (_context.ContextType)
            {
                case ContextType.Array:
                case ContextType.Document:
                case ContextType.ScopeDocument:
                    return BsonReaderState.Type;
                case ContextType.TopLevel:
                    return BsonReaderState.Initial;
                default:
                    throw new BsonInternalException("Unexpected ContextType.");
            }
        }

        private int ReadSize()
        {
            int size = _bsonStream.ReadInt32();
            if (size < 0)
            {
                var message = string.Format("Size {0} is not valid because it is negative.", size);
                throw new FormatException(message);
            }
            if (size > _settings.MaxDocumentSize)
            {
                var message = string.Format("Size {0} is not valid because it is larger than MaxDocumentSize {1}.", size, _settings.MaxDocumentSize);
                throw new FormatException(message);
            }
            return size;
        }
    }
}
