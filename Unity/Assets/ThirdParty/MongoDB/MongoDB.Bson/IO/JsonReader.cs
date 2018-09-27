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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON reader for a JSON string.
    /// </summary>
    public class JsonReader : BsonReader
    {
        #region static
        private static readonly string[] __variableLengthIso8601Formats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFzz",
            "yyyyMMddTHHmmss.FFFFFFFK",
            "yyyyMMddTHHmmss.FFFFFFFzz"
        };

        private static readonly string[][] __fixedLengthIso8601Formats = new string[][]
        {
            null, // length = 0
            null, // length = 1
            null, // length = 2
            null, // length = 3
            new [] { "yyyy" }, // length = 4
            null, // length = 5
            null, // length = 6
            new [] { "yyyy-MM" }, // length = 7
            new [] { "yyyyMMdd" }, // length = 8
            null, // length = 9
            new [] { "yyyy-MM-dd" }, // length = 10
            new [] { "yyyyMMddTHH" }, // length = 11
            new [] { "yyyyMMddTHHZ" }, // length = 12
            new [] { "yyyy-MM-ddTHH" , "yyyyMMddTHHmm" }, // length = 13
            new [] { "yyyy-MM-ddTHHZ", "yyyyMMddTHHmmZ", "yyyyMMddTHHzz" }, // length = 14
            null, // length = 15
            new [] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHHzz", "yyyyMMddTHHmmssZ", "yyyyMMddTHHmmzz" }, // length = 16
            new [] { "yyyy-MM-ddTHH:mmZ", "yyyyMMddTHHzzz" }, // length = 17
            new [] { "yyyyMMddTHHmmsszz" }, // length = 18
            new [] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHHzzz", "yyyy-MM-ddTHH:mmzz", "yyyyMMddTHHmmzzz" }, // length = 19
            null, // length = 20
            null, // length = 21
            new [] { "yyyy-MM-ddTHH:mmzzz", "yyyy-MM-ddTHH:mm:sszz" } // length = 22
        };
        #endregion

        // private fields
        private readonly JsonBuffer _buffer;
        private readonly JsonReaderSettings _jsonReaderSettings; // same value as in base class just declared as derived class
        private JsonReaderContext _context;
        private JsonToken _currentToken;
        private BsonValue _currentValue;
        private JsonToken _pushedToken;

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonReader class.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        public JsonReader(string json)
            : this(json, JsonReaderSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonReader class.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="settings">The reader settings.</param>
        public JsonReader(string json, JsonReaderSettings settings)
            : this(new JsonBuffer(json), settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonReader class.
        /// </summary>
        /// <param name="textReader">The TextReader.</param>
        public JsonReader(TextReader textReader)
            : this(textReader, JsonReaderSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonReader class.
        /// </summary>
        /// <param name="textReader">The TextReader.</param>
        /// <param name="settings">The reader settings.</param>
        public JsonReader(TextReader textReader, JsonReaderSettings settings)
            : this(new JsonBuffer(textReader), settings)
        {
        }

        private JsonReader(JsonBuffer buffer, JsonReaderSettings settings)
            : base(settings)
        {
            _buffer = buffer;
            _jsonReaderSettings = settings; // already frozen by base class
            _context = new JsonReaderContext(null, ContextType.TopLevel);
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
            return new JsonReaderBookmark(State, CurrentBsonType, CurrentName, _context, _currentToken, _currentValue, _pushedToken, _buffer.Position);
        }

        /// <summary>
        /// Determines whether this reader is at end of file.
        /// </summary>
        /// <returns>
        /// Whether this reader is at end of file.
        /// </returns>
        public override bool IsAtEndOfFile()
        {
            int c;
            while ((c = _buffer.Read()) != -1)
            {
                if (!char.IsWhiteSpace((char)c))
                {
                    _buffer.UnRead(c);
                    return false;
                }
            }
            return true;
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
                if (State == BsonReaderState.Initial)
                {
                    _buffer.ResetBuffer();
                }

                // in JSON the top level value can be of any type so fall through
                State = BsonReaderState.Type;
            }
            if (State != BsonReaderState.Type)
            {
                ThrowInvalidState("ReadBsonType", BsonReaderState.Type);
            }

            if (_context.ContextType == ContextType.Document)
            {
                var nameToken = PopToken();
                switch (nameToken.Type)
                {
                    case JsonTokenType.String:
                    case JsonTokenType.UnquotedString:
                        CurrentName = nameToken.StringValue;
                        break;
                    case JsonTokenType.EndObject:
                        State = BsonReaderState.EndOfDocument;
                        return BsonType.EndOfDocument;
                    default:
                        var message = string.Format("JSON reader was expecting a name but found '{0}'.", nameToken.Lexeme);
                        throw new FormatException(message);
                }

                var colonToken = PopToken();
                if (colonToken.Type != JsonTokenType.Colon)
                {
                    var message = string.Format("JSON reader was expecting ':' but found '{0}'.", colonToken.Lexeme);
                    throw new FormatException(message);
                }
            }

            var valueToken = PopToken();
            if (_context.ContextType == ContextType.Array && valueToken.Type == JsonTokenType.EndArray)
            {
                State = BsonReaderState.EndOfArray;
                return BsonType.EndOfDocument;
            }

            var noValueFound = false;
            switch (valueToken.Type)
            {
                case JsonTokenType.BeginArray:
                    CurrentBsonType = BsonType.Array;
                    break;
                case JsonTokenType.BeginObject:
                    CurrentBsonType = ParseExtendedJson();
                    break;
                case JsonTokenType.DateTime:
                    CurrentBsonType = BsonType.DateTime;
                    _currentValue = valueToken.DateTimeValue;
                    break;
                case JsonTokenType.Double:
                    CurrentBsonType = BsonType.Double;
                    _currentValue = valueToken.DoubleValue;
                    break;
                case JsonTokenType.EndOfFile:
                    CurrentBsonType = BsonType.EndOfDocument;
                    break;
                case JsonTokenType.Int32:
                    CurrentBsonType = BsonType.Int32;
                    _currentValue = valueToken.Int32Value;
                    break;
                case JsonTokenType.Int64:
                    CurrentBsonType = BsonType.Int64;
                    _currentValue = valueToken.Int64Value;
                    break;
                case JsonTokenType.ObjectId:
                    CurrentBsonType = BsonType.ObjectId;
                    _currentValue = valueToken.ObjectIdValue;
                    break;
                case JsonTokenType.RegularExpression:
                    CurrentBsonType = BsonType.RegularExpression;
                    _currentValue = valueToken.RegularExpressionValue;
                    break;
                case JsonTokenType.String:
                    CurrentBsonType = BsonType.String;
                    _currentValue = valueToken.StringValue;
                    break;
                case JsonTokenType.UnquotedString:
                    switch (valueToken.Lexeme)
                    {
                        case "false":
                        case "true":
                            CurrentBsonType = BsonType.Boolean;
                            _currentValue = JsonConvert.ToBoolean(valueToken.Lexeme);
                            break;
                        case "Infinity":
                            CurrentBsonType = BsonType.Double;
                            _currentValue = double.PositiveInfinity;
                            break;
                        case "NaN":
                            CurrentBsonType = BsonType.Double;
                            _currentValue = double.NaN;
                            break;
                        case "null":
                            CurrentBsonType = BsonType.Null;
                            break;
                        case "undefined":
                            CurrentBsonType = BsonType.Undefined;
                            break;
                        case "BinData":
                            CurrentBsonType = BsonType.Binary;
                            _currentValue = ParseBinDataConstructor();
                            break;
                        case "Date":
                            CurrentBsonType = BsonType.String;
                            _currentValue = ParseDateTimeConstructor(false); // withNew = false
                            break;
                        case "HexData":
                            CurrentBsonType = BsonType.Binary;
                            _currentValue = ParseHexDataConstructor();
                            break;
                        case "ISODate":
                            CurrentBsonType = BsonType.DateTime;
                            _currentValue = ParseISODateTimeConstructor();
                            break;
                        case "MaxKey":
                            CurrentBsonType = BsonType.MaxKey;
                            _currentValue = BsonMaxKey.Value;
                            break;
                        case "MinKey":
                            CurrentBsonType = BsonType.MinKey;
                            _currentValue = BsonMinKey.Value;
                            break;
                        case "NumberDecimal":
                            CurrentBsonType = BsonType.Decimal128;
                            _currentValue = ParseNumberDecimalConstructor();
                            break;
                        case "Number":
                        case "NumberInt":
                            CurrentBsonType = BsonType.Int32;
                            _currentValue = ParseNumberConstructor();
                            break;
                        case "NumberLong":
                            CurrentBsonType = BsonType.Int64;
                            _currentValue = ParseNumberLongConstructor();
                            break;
                        case "ObjectId":
                            CurrentBsonType = BsonType.ObjectId;
                            _currentValue = ParseObjectIdConstructor();
                            break;
                        case "RegExp":
                            CurrentBsonType = BsonType.RegularExpression;
                            _currentValue = ParseRegularExpressionConstructor();
                            break;
                        case "Timestamp":
                            CurrentBsonType = BsonType.Timestamp;
                            _currentValue = ParseTimestampConstructor();
                            break;
                        case "UUID":
                        case "GUID":
                        case "CSUUID":
                        case "CSGUID":
                        case "JUUID":
                        case "JGUID":
                        case "PYUUID":
                        case "PYGUID":
                            CurrentBsonType = BsonType.Binary;
                            _currentValue = ParseUUIDConstructor(valueToken.Lexeme);
                            break;
                        case "new":
                            CurrentBsonType = ParseNew(out _currentValue);
                            break;
                        default:
                            noValueFound = true;
                            break;
                    }
                    break;
                default:
                    noValueFound = true;
                    break;
            }
            if (noValueFound)
            {
                var message = string.Format("JSON reader was expecting a value but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            _currentToken = valueToken;

            if (_context.ContextType == ContextType.Array || _context.ContextType == ContextType.Document)
            {
                var commaToken = PopToken();
                if (commaToken.Type != JsonTokenType.Comma)
                {
                    PushToken(commaToken);
                }
            }

            switch (_context.ContextType)
            {
                case ContextType.Document:
                case ContextType.ScopeDocument:
                default:
                    State = BsonReaderState.Name;
                    break;
                case ContextType.Array:
                case ContextType.JavaScriptWithScope:
                case ContextType.TopLevel:
                    State = BsonReaderState.Value;
                    break;
            }
            return CurrentBsonType;
        }

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A byte array.</returns>
        public override byte[] ReadBytes()
        {
#pragma warning disable 618
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadBinaryData", BsonType.Binary);
            State = GetNextState();
            var binaryData = _currentValue.AsBsonBinaryData;

            var subType = binaryData.SubType;
            if (subType != BsonBinarySubType.Binary && subType != BsonBinarySubType.OldBinary)
            {
                var message = string.Format("ReadBytes requires the binary sub type to be Binary, not {0}.", subType);
                throw new FormatException(message);
            }

            return binaryData.Bytes;
#pragma warning restore
        }

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
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
                default: throw new BsonInternalException("Unexpected ContextType.");
            }

            if (_context.ContextType == ContextType.Array || _context.ContextType == ContextType.Document)
            {
                var commaToken = PopToken();
                if (commaToken.Type != JsonTokenType.Comma)
                {
                    PushToken(commaToken);
                }
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
            if (_context != null && _context.ContextType == ContextType.JavaScriptWithScope)
            {
                _context = _context.PopContext(); // JavaScriptWithScope
                VerifyToken("}"); // outermost closing bracket for JavaScriptWithScope
            }
            switch (_context.ContextType)
            {
                case ContextType.Array: State = BsonReaderState.Type; break;
                case ContextType.Document: State = BsonReaderState.Type; break;
                case ContextType.TopLevel: State = BsonReaderState.Initial; break;
                default: throw new BsonInternalException("Unexpected ContextType");
            }

            if (_context.ContextType == ContextType.Array || _context.ContextType == ContextType.Document)
            {
                var commaToken = PopToken();
                if (commaToken.Type != JsonTokenType.Comma)
                {
                    PushToken(commaToken);
                }
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
            return _currentValue.AsString;
        }

        /// <summary>
        /// Reads a BSON JavaScript with scope from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <returns>A string.</returns>
        public override string ReadJavaScriptWithScope()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadJavaScriptWithScope", BsonType.JavaScriptWithScope);
            _context = new JsonReaderContext(_context, ContextType.JavaScriptWithScope);
            State = BsonReaderState.ScopeDocument;
            return _currentValue.AsString;
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

            _context = new JsonReaderContext(_context, ContextType.Array);
            State = BsonReaderState.Type;
        }

        /// <summary>
        /// Reads the start of a BSON document.
        /// </summary>
        public override void ReadStartDocument()
        {
            if (Disposed) { ThrowObjectDisposedException(); }
            VerifyBsonType("ReadStartDocument", BsonType.Document);

            _context = new JsonReaderContext(_context, ContextType.Document);
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
            return _currentValue.AsString;
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
            var timestamp = _currentValue.AsBsonTimestamp;
            return timestamp.Value;
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
            if (Disposed) { ThrowObjectDisposedException(); }
            var jsonReaderBookmark = (JsonReaderBookmark)bookmark;
            State = jsonReaderBookmark.State;
            CurrentBsonType = jsonReaderBookmark.CurrentBsonType;
            CurrentName = jsonReaderBookmark.CurrentName;
            _context = jsonReaderBookmark.CloneContext();
            _currentToken = jsonReaderBookmark.CurrentToken;
            _currentValue = jsonReaderBookmark.CurrentValue;
            _pushedToken = jsonReaderBookmark.PushedToken;
            _buffer.Position = jsonReaderBookmark.Position;
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

            switch (CurrentBsonType)
            {
                case BsonType.Array:
                    ReadStartArray();
                    while (ReadBsonType() != BsonType.EndOfDocument)
                    {
                        SkipValue();
                    }
                    ReadEndArray();
                    break;
                case BsonType.Binary:
                    ReadBinaryData();
                    break;
                case BsonType.Boolean:
                    ReadBoolean();
                    break;
                case BsonType.DateTime:
                    ReadDateTime();
                    break;
                case BsonType.Document:
                    ReadStartDocument();
                    while (ReadBsonType() != BsonType.EndOfDocument)
                    {
                        SkipName();
                        SkipValue();
                    }
                    ReadEndDocument();
                    break;
                case BsonType.Double:
                    ReadDouble();
                    break;
                case BsonType.Int32:
                    ReadInt32();
                    break;
                case BsonType.Int64:
                    ReadInt64();
                    break;
                case BsonType.JavaScript:
                    ReadJavaScript();
                    break;
                case BsonType.JavaScriptWithScope:
                    ReadJavaScriptWithScope();
                    ReadStartDocument();
                    while (ReadBsonType() != BsonType.EndOfDocument)
                    {
                        SkipName();
                        SkipValue();
                    }
                    ReadEndDocument();
                    break;
                case BsonType.MaxKey:
                    ReadMaxKey();
                    break;
                case BsonType.MinKey:
                    ReadMinKey();
                    break;
                case BsonType.Null:
                    ReadNull();
                    break;
                case BsonType.ObjectId:
                    ReadObjectId();
                    break;
                case BsonType.RegularExpression:
                    ReadRegularExpression();
                    break;
                case BsonType.String:
                    ReadString();
                    break;
                case BsonType.Symbol:
                    ReadSymbol();
                    break;
                case BsonType.Timestamp:
                    ReadTimestamp();
                    break;
                case BsonType.Undefined:
                    ReadUndefined();
                    break;
                default:
                    throw new BsonInternalException("Invalid BsonType.");
            }
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
        private string FormatInvalidTokenMessage(JsonToken token)
        {
            return string.Format("Invalid JSON token: '{0}'", token.Lexeme);
        }

        private string FormatJavaScriptDateTimeString(DateTime dateTime)
        {
            var utc = BsonUtils.ToUniversalTime(dateTime);
            var local = BsonUtils.ToLocalTime(utc);
            var offset = local - utc;
            var offsetSign = "+";
            if (offset < TimeSpan.Zero)
            {
                offset = -offset;
                offsetSign = "-";
            }
            var timeZone = TimeZoneInfo.Local;
            var timeZoneName = local.IsDaylightSavingTime() ? timeZone.DaylightName : timeZone.StandardName;
            var dateTimeString = string.Format(
                "{0} GMT{1}{2:D2}{3:D2} ({4})",
                local.ToString("ddd MMM dd yyyy HH:mm:ss"), offsetSign, offset.Hours, offset.Minutes, timeZoneName);
            return dateTimeString;
        }

        private BsonReaderState GetNextState()
        {
            switch (_context.ContextType)
            {
                case ContextType.Array:
                case ContextType.Document:
                    return BsonReaderState.Type;
                case ContextType.TopLevel:
                    return BsonReaderState.Initial;
                default:
                    throw new BsonInternalException("Unexpected ContextType.");
            }
        }

        private BsonValue ParseBinDataConstructor()
        {
            VerifyToken("(");
            var subTypeToken = PopToken();
            if (subTypeToken.Type != JsonTokenType.Int32)
            {
                var message = string.Format("JSON reader expected a binary subtype but found '{0}'.", subTypeToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(",");
            var bytesToken = PopToken();
            if (bytesToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", bytesToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            var bytes = Convert.FromBase64String(bytesToken.StringValue);
            var subType = (BsonBinarySubType)subTypeToken.Int32Value;
            GuidRepresentation guidRepresentation;
            switch (subType)
            {
                case BsonBinarySubType.UuidLegacy: guidRepresentation = _jsonReaderSettings.GuidRepresentation; break;
                case BsonBinarySubType.UuidStandard: guidRepresentation = GuidRepresentation.Standard; break;
                default: guidRepresentation = GuidRepresentation.Unspecified; break;
            }
            return new BsonBinaryData(bytes, subType, guidRepresentation);
        }

        private BsonValue ParseBinDataExtendedJson()
        {
            VerifyToken(":");

            var bytesToken = PopToken();
            if (bytesToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", bytesToken.Lexeme);
                throw new FormatException(message);
            }
            var bytes = Convert.FromBase64String(bytesToken.StringValue);

            VerifyToken(",");
            VerifyString("$type");
            VerifyToken(":");

            BsonBinarySubType subType;
            var subTypeToken = PopToken();
            if (subTypeToken.Type == JsonTokenType.String)
            {
                subType = (BsonBinarySubType)Convert.ToInt32(subTypeToken.StringValue, 16);
            }
            else if (subTypeToken.Type == JsonTokenType.Int32 || subTypeToken.Type == JsonTokenType.Int64)
            {
                subType = (BsonBinarySubType)subTypeToken.Int32Value;
            }
            else
            {
                var message = string.Format("JSON reader expected a string or integer but found '{0}'.", subTypeToken.Lexeme);
                throw new FormatException(message);
            }

            VerifyToken("}");

            GuidRepresentation guidRepresentation;
            switch (subType)
            {
                case BsonBinarySubType.UuidLegacy: guidRepresentation = _jsonReaderSettings.GuidRepresentation; break;
                case BsonBinarySubType.UuidStandard: guidRepresentation = GuidRepresentation.Standard; break;
                default: guidRepresentation = GuidRepresentation.Unspecified; break;
            }

            return new BsonBinaryData(bytes, subType, guidRepresentation);
        }

        private BsonValue ParseHexDataConstructor()
        {
            VerifyToken("(");
            var subTypeToken = PopToken();
            if (subTypeToken.Type != JsonTokenType.Int32)
            {
                var message = string.Format("JSON reader expected a binary subtype but found '{0}'.", subTypeToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(",");
            var bytesToken = PopToken();
            if (bytesToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", bytesToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            var bytes = BsonUtils.ParseHexString(bytesToken.StringValue);
            var subType = (BsonBinarySubType)subTypeToken.Int32Value;
            GuidRepresentation guidRepresentation;
            switch (subType)
            {
                case BsonBinarySubType.UuidLegacy: guidRepresentation = _jsonReaderSettings.GuidRepresentation; break;
                case BsonBinarySubType.UuidStandard: guidRepresentation = GuidRepresentation.Standard; break;
                default: guidRepresentation = GuidRepresentation.Unspecified; break;
            }
            return new BsonBinaryData(bytes, subType, guidRepresentation);
        }

        private BsonType ParseJavaScriptExtendedJson(out BsonValue value)
        {
            VerifyToken(":");
            var codeToken = PopToken();
            if (codeToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", codeToken.Lexeme);
                throw new FormatException(message);
            }
            var nextToken = PopToken();
            switch (nextToken.Type)
            {
                case JsonTokenType.Comma:
                    VerifyString("$scope");
                    VerifyToken(":");
                    State = BsonReaderState.Value;
                    value = codeToken.StringValue;
                    return BsonType.JavaScriptWithScope;
                case JsonTokenType.EndObject:
                    value = codeToken.StringValue;
                    return BsonType.JavaScript;
                default:
                    var message = string.Format("JSON reader expected ',' or '}}' but found '{0}'.", codeToken.Lexeme);
                    throw new FormatException(message);
            }
        }

        private BsonValue ParseISODateTimeConstructor()
        {
            VerifyToken("(");
            var valueToken = PopToken();
            if (valueToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            var value = valueToken.StringValue;
            string[] formats = null;
            if (!value.Contains(".") && value.Length < __fixedLengthIso8601Formats.Length)
            {
                formats = __fixedLengthIso8601Formats[value.Length];
            }
            if (formats == null)
            {
                formats = __variableLengthIso8601Formats;
            }
            var utcDateTime = DateTime.ParseExact(value, formats, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            return new BsonDateTime(utcDateTime);
        }

        private BsonValue ParseDateTimeExtendedJson()
        {
            VerifyToken(":");
            var valueToken = PopToken();

            long millisecondsSinceEpoch;
            if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                millisecondsSinceEpoch = valueToken.Int64Value;
            }
            else if (valueToken.Type == JsonTokenType.String)
            {
                DateTime dateTime;
                var dateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
                if (!DateTime.TryParse(valueToken.StringValue, CultureInfo.InvariantCulture, dateTimeStyles, out dateTime))
                {
                    var message = string.Format("Invalid $date string: '{0}'.", valueToken.StringValue);
                    throw new FormatException(message);
                }
                millisecondsSinceEpoch = BsonUtils.ToMillisecondsSinceEpoch(dateTime);
            }
            else if (valueToken.Type == JsonTokenType.BeginObject)
            {
                VerifyToken("$numberLong");
                VerifyToken(":");
                var millisecondsSinceEpochToken = PopToken();
                if (millisecondsSinceEpochToken.Type == JsonTokenType.String)
                {
                    millisecondsSinceEpoch = long.Parse(millisecondsSinceEpochToken.StringValue, CultureInfo.InvariantCulture);
                }
                else if (millisecondsSinceEpochToken.Type == JsonTokenType.Int32 || millisecondsSinceEpochToken.Type == JsonTokenType.Int64)
                {
                    millisecondsSinceEpoch = millisecondsSinceEpochToken.Int64Value;
                }
                else
                {
                    var message = string.Format("JSON reader expected an integer or a string for {{ $date : {{ $numberLong : ... }} }} but found a '{0}'.", valueToken.Lexeme);
                    throw new FormatException(message);
                }
                VerifyToken("}");
            }
            else
            {
                var message = string.Format("JSON reader expected an ISO 8601 string, an integer, or {{ $numberLong : ... }} for $date but found a '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }

            VerifyToken("}");
            return new BsonDateTime(millisecondsSinceEpoch);
        }

        private BsonValue ParseDateTimeConstructor(bool withNew)
        {
            VerifyToken("(");

            // Date when used without "new" behaves differently (JavaScript has some weird parts)
            if (!withNew)
            {
                VerifyToken(")");
                var dateTimeString = FormatJavaScriptDateTimeString(DateTime.UtcNow);
                return new BsonString(dateTimeString);
            }

            var token = PopToken();
            if (token.Lexeme == ")")
            {
                return new BsonDateTime(DateTime.UtcNow);
            }
            else if (token.Type == JsonTokenType.String)
            {
                VerifyToken(")");
                var dateTimeString = token.StringValue;
                var dateTime = ParseJavaScriptDateTimeString(dateTimeString);
                return new BsonDateTime(dateTime);
            }
            else if (token.Type == JsonTokenType.Int32 || token.Type == JsonTokenType.Int64)
            {
                var args = new List<long>();
                while (true)
                {
                    args.Add(token.Int64Value);
                    token = PopToken();
                    if (token.Lexeme == ")")
                    {
                        break;
                    }
                    if (token.Lexeme != ",")
                    {
                        var message = string.Format("JSON reader expected a ',' or a ')' but found '{0}'.", token.Lexeme);
                        throw new FormatException(message);
                    }
                    token = PopToken();
                    if (token.Type != JsonTokenType.Int32 && token.Type != JsonTokenType.Int64)
                    {
                        var message = string.Format("JSON reader expected an integer but found '{0}'.", token.Lexeme);
                        throw new FormatException(message);
                    }
                }
                switch (args.Count)
                {
                    case 1:
                        return new BsonDateTime(args[0]);
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        var year = (int)args[0];
                        var month = (int)args[1] + 1; // JavaScript starts at 0 but .NET starts at 1
                        var day = (int)args[2];
                        var hours = (args.Count >= 4) ? (int)args[3] : 0;
                        var minutes = (args.Count >= 5) ? (int)args[4] : 0;
                        var seconds = (args.Count >= 6) ? (int)args[5] : 0;
                        var milliseconds = (args.Count == 7) ? (int)args[6] : 0;
                        var dateTime = new DateTime(year, month, day, hours, minutes, seconds, milliseconds, DateTimeKind.Utc);
                        return new BsonDateTime(dateTime);
                    default:
                        var message = string.Format("JSON reader expected 1 or 3-7 integers but found {0}.", args.Count);
                        throw new FormatException(message);
                }
            }
            else
            {
                var message = string.Format("JSON reader expected an integer or a string but found '{0}'.", token.Lexeme);
                throw new FormatException(message);
            }
        }

        private BsonType ParseExtendedJson()
        {
            var nameToken = PopToken();
            if (nameToken.Type == JsonTokenType.String || nameToken.Type == JsonTokenType.UnquotedString)
            {
                switch (nameToken.StringValue)
                {
                    case "$binary": _currentValue = ParseBinDataExtendedJson(); return BsonType.Binary;
                    case "$code": return ParseJavaScriptExtendedJson(out _currentValue);
                    case "$date": _currentValue = ParseDateTimeExtendedJson(); return BsonType.DateTime;
                    case "$maxkey": case "$maxKey": _currentValue = ParseMaxKeyExtendedJson(); return BsonType.MaxKey;
                    case "$minkey": case "$minKey": _currentValue = ParseMinKeyExtendedJson(); return BsonType.MinKey;
                    case "$numberDecimal": _currentValue = ParseNumberDecimalExtendedJson(); return BsonType.Decimal128;
                    case "$numberLong": _currentValue = ParseNumberLongExtendedJson(); return BsonType.Int64;
                    case "$oid": _currentValue = ParseObjectIdExtendedJson(); return BsonType.ObjectId;
                    case "$regex": _currentValue = ParseRegularExpressionExtendedJson(); return BsonType.RegularExpression;
                    case "$symbol": _currentValue = ParseSymbolExtendedJson(); return BsonType.Symbol;
                    case "$timestamp": _currentValue = ParseTimestampExtendedJson(); return BsonType.Timestamp;
                    case "$undefined": _currentValue = ParseUndefinedExtendedJson(); return BsonType.Undefined;
                }
            }
            PushToken(nameToken);
            return BsonType.Document;
        }

        private DateTime ParseJavaScriptDateTimeString(string dateTimeString)
        {
            // if DateTime.TryParse succeeds we're done, otherwise assume it's an RFC 822 formatted DateTime string
            DateTime dateTime;
            if (DateTime.TryParse(dateTimeString, out dateTime))
            {
                return dateTime;
            }
            else
            {
                var rfc822DateTimePattern =
                    @"^((?<dayOfWeek>(Mon|Tue|Wed|Thu|Fri|Sat|Sun)), )?" +
                    @"(?<day>\d{1,2}) +" +
                    @"(?<monthName>Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) " +
                    @"(?<year>\d{2}|\d{4}) " +
                    @"(?<hour>\d{1,2}):" +
                    @"(?<minutes>\d{1,2}):" +
                    @"(?<seconds>\d{1,2}(.\d{1,7})?) " +
                    @"(?<zone>UT|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT|[A-Z]|([+-]\d{4}))$";
                var match = Regex.Match(dateTimeString, rfc822DateTimePattern);
                if (match.Success)
                {
                    var day = int.Parse(match.Groups["day"].Value);

                    int month;
                    var monthName = match.Groups["monthName"].Value;
                    switch (monthName)
                    {
                        case "Jan": month = 1; break;
                        case "Feb": month = 2; break;
                        case "Mar": month = 3; break;
                        case "Apr": month = 4; break;
                        case "May": month = 5; break;
                        case "Jun": month = 6; break;
                        case "Jul": month = 7; break;
                        case "Aug": month = 8; break;
                        case "Sep": month = 9; break;
                        case "Oct": month = 10; break;
                        case "Nov": month = 11; break;
                        case "Dec": month = 12; break;
                        default:
                            var message = string.Format("\"{0}\" is not a valid RFC 822 month name.", monthName);
                            throw new FormatException(message);
                    }

                    var yearString = match.Groups["year"].Value;
                    int year = int.Parse(yearString);
                    if (yearString.Length == 2)
                    {
                        year += 2000;
                        if (year - DateTime.UtcNow.Year >= 19) { year -= 100; }
                    }

                    var hour = int.Parse(match.Groups["hour"].Value);
                    var minutes = int.Parse(match.Groups["minutes"].Value);
                    var secondsString = match.Groups["seconds"].Value;
                    int seconds;
                    double milliseconds;
                    if (secondsString.IndexOf('.') != -1)
                    {
                        var timeSpan = TimeSpan.FromSeconds(double.Parse(secondsString));
                        seconds = timeSpan.Seconds;
                        milliseconds = timeSpan.TotalMilliseconds - seconds * 1000;
                    }
                    else
                    {
                        seconds = int.Parse(secondsString);
                        milliseconds = 0;
                    }

                    dateTime = new DateTime(year, month, day, hour, minutes, seconds, DateTimeKind.Utc).AddMilliseconds(milliseconds);

                    // check day of week before converting to UTC
                    var dayOfWeekString = match.Groups["dayOfWeek"].Value;
                    if (dayOfWeekString != "")
                    {
                        DayOfWeek dayOfWeek;
                        switch (dayOfWeekString)
                        {
                            case "Mon": dayOfWeek = DayOfWeek.Monday; break;
                            case "Tue": dayOfWeek = DayOfWeek.Tuesday; break;
                            case "Wed": dayOfWeek = DayOfWeek.Wednesday; break;
                            case "Thu": dayOfWeek = DayOfWeek.Thursday; break;
                            case "Fri": dayOfWeek = DayOfWeek.Friday; break;
                            case "Sat": dayOfWeek = DayOfWeek.Saturday; break;
                            case "Sun": dayOfWeek = DayOfWeek.Sunday; break;
                            default:
                                var message = string.Format("\"{0}\" is not a valid RFC 822 day name.", dayOfWeekString);
                                throw new FormatException(message);
                        }
                        if (dateTime.DayOfWeek != dayOfWeek)
                        {
                            var message = string.Format("\"{0}\" is not the right day of the week for {1}.", dayOfWeekString, dateTime.ToString("o"));
                            throw new FormatException(message);
                        }
                    }

                    TimeSpan offset;
                    var zone = match.Groups["zone"].Value;
                    switch (zone)
                    {
                        case "UT": case "GMT": case "Z": offset = TimeSpan.Zero; break;
                        case "EST": offset = TimeSpan.FromHours(-5); break;
                        case "EDT": offset = TimeSpan.FromHours(-4); break;
                        case "CST": offset = TimeSpan.FromHours(-6); break;
                        case "CDT": offset = TimeSpan.FromHours(-5); break;
                        case "MST": offset = TimeSpan.FromHours(-7); break;
                        case "MDT": offset = TimeSpan.FromHours(-6); break;
                        case "PST": offset = TimeSpan.FromHours(-8); break;
                        case "PDT": offset = TimeSpan.FromHours(-7); break;
                        case "A": offset = TimeSpan.FromHours(-1); break;
                        case "B": offset = TimeSpan.FromHours(-2); break;
                        case "C": offset = TimeSpan.FromHours(-3); break;
                        case "D": offset = TimeSpan.FromHours(-4); break;
                        case "E": offset = TimeSpan.FromHours(-5); break;
                        case "F": offset = TimeSpan.FromHours(-6); break;
                        case "G": offset = TimeSpan.FromHours(-7); break;
                        case "H": offset = TimeSpan.FromHours(-8); break;
                        case "I": offset = TimeSpan.FromHours(-9); break;
                        case "K": offset = TimeSpan.FromHours(-10); break;
                        case "L": offset = TimeSpan.FromHours(-11); break;
                        case "M": offset = TimeSpan.FromHours(-12); break;
                        case "N": offset = TimeSpan.FromHours(1); break;
                        case "O": offset = TimeSpan.FromHours(2); break;
                        case "P": offset = TimeSpan.FromHours(3); break;
                        case "Q": offset = TimeSpan.FromHours(4); break;
                        case "R": offset = TimeSpan.FromHours(5); break;
                        case "S": offset = TimeSpan.FromHours(6); break;
                        case "T": offset = TimeSpan.FromHours(7); break;
                        case "U": offset = TimeSpan.FromHours(8); break;
                        case "V": offset = TimeSpan.FromHours(9); break;
                        case "W": offset = TimeSpan.FromHours(10); break;
                        case "X": offset = TimeSpan.FromHours(11); break;
                        case "Y": offset = TimeSpan.FromHours(12); break;
                        default:
                            var offsetSign = zone.Substring(0);
                            var offsetHours = zone.Substring(1, 2);
                            var offsetMinutes = zone.Substring(3, 2);
                            offset = TimeSpan.FromHours(int.Parse(offsetHours)) + TimeSpan.FromMinutes(int.Parse(offsetMinutes));
                            if (offsetSign == "-")
                            {
                                offset = -offset;
                            }
                            break;
                    }

                    return dateTime.Add(-offset);
                }
                else
                {
                    var message = string.Format("The DateTime string \"{0}\" is not a valid DateTime string for either .NET or JavaScript.", dateTimeString);
                    throw new FormatException(message);
                }
            }
        }

        private BsonValue ParseMaxKeyExtendedJson()
        {
            VerifyToken(":");
            VerifyToken("1");
            VerifyToken("}");
            return BsonMaxKey.Value;
        }

        private BsonValue ParseMinKeyExtendedJson()
        {
            VerifyToken(":");
            VerifyToken("1");
            VerifyToken("}");
            return BsonMinKey.Value;
        }

        private BsonType ParseNew(out BsonValue value)
        {
            var typeToken = PopToken();
            if (typeToken.Type != JsonTokenType.UnquotedString)
            {
                var message = string.Format("JSON reader expected a type name but found '{0}'.", typeToken.Lexeme);
                throw new FormatException(message);
            }
            switch (typeToken.Lexeme)
            {
                case "BinData":
                    value = ParseBinDataConstructor();
                    return BsonType.Binary;
                case "Date":
                    value = ParseDateTimeConstructor(true); // withNew = true
                    return BsonType.DateTime;
                case "HexData":
                    value = ParseHexDataConstructor();
                    return BsonType.Binary;
                case "ISODate":
                    value = ParseISODateTimeConstructor();
                    return BsonType.DateTime;
                case "NumberDecimal":
                    value = ParseNumberDecimalConstructor();
                    return BsonType.Decimal128;
                case "NumberInt":
                    value = ParseNumberConstructor();
                    return BsonType.Int32;
                case "NumberLong":
                    value = ParseNumberLongConstructor();
                    return BsonType.Int64;
                case "ObjectId":
                    value = ParseObjectIdConstructor();
                    return BsonType.ObjectId;
                case "RegExp":
                    value = ParseRegularExpressionConstructor();
                    return BsonType.RegularExpression;
                case "Timestamp":
                    value = ParseTimestampConstructor();
                    return BsonType.Timestamp;
                case "UUID":
                case "GUID":
                case "CSUUID":
                case "CSGUID":
                case "JUUID":
                case "JGUID":
                case "PYUUID":
                case "PYGUID":
                    value = ParseUUIDConstructor(typeToken.Lexeme);
                    return BsonType.Binary;
                default:
                    var message = string.Format("JSON reader expected a type name but found '{0}'.", typeToken.Lexeme);
                    throw new FormatException(message);
            }
        }

        private BsonValue ParseNumberConstructor()
        {
            VerifyToken("(");
            var valueToken = PopToken();
            int value;
            if (valueToken.IsNumber)
            {
                value = valueToken.Int32Value;
            }
            else if (valueToken.Type == JsonTokenType.String)
            {
                value = int.Parse(valueToken.StringValue);
            }
            else
            {
                var message = string.Format("JSON reader expected an integer or a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            return (BsonInt32)value;
        }

        private BsonValue ParseNumberDecimalConstructor()
        {
            VerifyToken("(");
            var valueToken = PopToken();
            Decimal128 value;
            if (valueToken.Type == JsonTokenType.String)
            {
                value = Decimal128.Parse(valueToken.StringValue);
            }
            else if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                value = new Decimal128(valueToken.Int64Value);
            }
            else
            {
                var message = string.Format("JSON reader expected an integer or a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            return (BsonDecimal128)value;
        }

        private BsonValue ParseNumberLongConstructor()
        {
            VerifyToken("(");
            var valueToken = PopToken();
            long value;
            if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                value = valueToken.Int64Value;
            }
            else if (valueToken.Type == JsonTokenType.String)
            {
                value = long.Parse(valueToken.StringValue);
            }
            else
            {
                var message = string.Format("JSON reader expected an integer or a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            return (BsonInt64)value;
        }

        private BsonValue ParseNumberDecimalExtendedJson()
        {
            VerifyToken(":");

            Decimal128 value;
            var valueToken = PopToken();
            if (valueToken.Type == JsonTokenType.String)
            {
                value = Decimal128.Parse(valueToken.StringValue);
            }
            else if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                value = new Decimal128(valueToken.Int64Value);
            }
            else
            {
                var message = string.Format("JSON reader expected a string or an integer but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }

            VerifyToken("}");
            return (BsonDecimal128)value;
        }

        private BsonValue ParseNumberLongExtendedJson()
        {
            VerifyToken(":");

            long value;
            var valueToken = PopToken();
            if (valueToken.Type == JsonTokenType.String)
            {
                value = long.Parse(valueToken.StringValue, CultureInfo.InvariantCulture);
            }
            else if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                value = valueToken.Int64Value;
            }
            else
            {
                var message = string.Format("JSON reader expected a string or an integer but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }

            VerifyToken("}");
            return (BsonInt64)value;
        }

        private BsonValue ParseObjectIdConstructor()
        {
            VerifyToken("(");
            var valueToken = PopToken();
            if (valueToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            return new BsonObjectId(ObjectId.Parse(valueToken.StringValue));
        }

        private BsonValue ParseObjectIdExtendedJson()
        {
            VerifyToken(":");
            var valueToken = PopToken();
            if (valueToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken("}");
            return new BsonObjectId(ObjectId.Parse(valueToken.StringValue));
        }

        private BsonValue ParseRegularExpressionConstructor()
        {
            VerifyToken("(");
            var patternToken = PopToken();
            if (patternToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", patternToken.Lexeme);
                throw new FormatException(message);
            }
            var options = "";
            var commaToken = PopToken();
            if (commaToken.Lexeme == ",")
            {
                var optionsToken = PopToken();
                if (optionsToken.Type != JsonTokenType.String)
                {
                    var message = string.Format("JSON reader expected a string but found '{0}'.", optionsToken.Lexeme);
                    throw new FormatException(message);
                }
                options = optionsToken.StringValue;
            }
            else
            {
                PushToken(commaToken);
            }
            VerifyToken(")");
            return new BsonRegularExpression(patternToken.StringValue, options);
        }

        private BsonValue ParseRegularExpressionExtendedJson()
        {
            VerifyToken(":");
            var patternToken = PopToken();
            if (patternToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", patternToken.Lexeme);
                throw new FormatException(message);
            }
            var options = "";
            var commaToken = PopToken();
            if (commaToken.Lexeme == ",")
            {
                VerifyString("$options");
                VerifyToken(":");
                var optionsToken = PopToken();
                if (optionsToken.Type != JsonTokenType.String)
                {
                    var message = string.Format("JSON reader expected a string but found '{0}'.", optionsToken.Lexeme);
                    throw new FormatException(message);
                }
                options = optionsToken.StringValue;
            }
            else
            {
                PushToken(commaToken);
            }
            VerifyToken("}");
            return new BsonRegularExpression(patternToken.StringValue, options);
        }

        private BsonValue ParseSymbolExtendedJson()
        {
            VerifyToken(":");
            var nameToken = PopToken();
            if (nameToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", nameToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken("}");
            return (BsonString)nameToken.StringValue; // will be converted to a BsonSymbol at a higher level
        }

        private BsonValue ParseTimestampConstructor()
        {
            VerifyToken("(");
            int secondsSinceEpoch;
            var secondsSinceEpochToken = PopToken();
            if (secondsSinceEpochToken.IsNumber)
            {
                secondsSinceEpoch = secondsSinceEpochToken.Int32Value;
            }
            else
            {
                var message = string.Format("JSON reader expected a number but found '{0}'.", secondsSinceEpochToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(",");
            int increment;
            var incrementToken = PopToken();
            if (secondsSinceEpochToken.IsNumber)
            {
                increment = incrementToken.Int32Value;
            }
            else
            {
                var message = string.Format("JSON reader expected a number but found '{0}'.", secondsSinceEpochToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            return new BsonTimestamp(secondsSinceEpoch, increment);
        }

        private BsonValue ParseTimestampExtendedJson()
        {
            VerifyToken(":");
            var nextToken = PopToken();
            if (nextToken.Type == JsonTokenType.BeginObject)
            {
                return ParseTimestampExtendedJsonNewRepresentation();
            }
            else
            {
                return ParseTimestampExtendedJsonOldRepresentation(nextToken);
            }
        }

        private BsonValue ParseTimestampExtendedJsonNewRepresentation()
        {
            VerifyString("t");
            VerifyToken(":");
            var secondsSinceEpochToken = PopToken();
            int secondsSinceEpoch;
            if (secondsSinceEpochToken.IsNumber)
            {
                secondsSinceEpoch = secondsSinceEpochToken.Int32Value;
            }
            else
            {
                var message = string.Format("JSON reader expected an integer but found '{0}'.", secondsSinceEpochToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(",");
            VerifyString("i");
            VerifyToken(":");
            var incrementToken = PopToken();
            int increment;
            if (incrementToken.IsNumber)
            {
                increment = incrementToken.Int32Value;
            }
            else
            {
                var message = string.Format("JSON reader expected an integer but found '{0}'.", incrementToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken("}");
            VerifyToken("}");
            return new BsonTimestamp(secondsSinceEpoch, increment);
        }

        private BsonValue ParseTimestampExtendedJsonOldRepresentation(JsonToken valueToken)
        {
            long value;
            if (valueToken.Type == JsonTokenType.Int32 || valueToken.Type == JsonTokenType.Int64)
            {
                value = valueToken.Int64Value;
            }
            else if (valueToken.Type == JsonTokenType.UnquotedString && valueToken.Lexeme == "NumberLong")
            {
                value = ParseNumberLongConstructor().AsInt64;
            }
            else
            {
                var message = string.Format("JSON reader expected an integer but found '{0}'.", valueToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken("}");
            return new BsonTimestamp(value);
        }

        private BsonValue ParseUndefinedExtendedJson()
        {
            VerifyToken(":");
            VerifyToken("true");
            VerifyToken("}");
            return BsonMaxKey.Value;
        }

        private BsonValue ParseUUIDConstructor(string uuidConstructorName)
        {
            VerifyToken("(");
            var bytesToken = PopToken();
            if (bytesToken.Type != JsonTokenType.String)
            {
                var message = string.Format("JSON reader expected a string but found '{0}'.", bytesToken.Lexeme);
                throw new FormatException(message);
            }
            VerifyToken(")");
            var hexString = bytesToken.StringValue.Replace("{", "").Replace("}", "").Replace("-", "");
            var bytes = BsonUtils.ParseHexString(hexString);
            var guid = GuidConverter.FromBytes(bytes, GuidRepresentation.Standard);
            GuidRepresentation guidRepresentation;
            switch (uuidConstructorName)
            {
                case "CSUUID":
                case "CSGUID":
                    guidRepresentation = GuidRepresentation.CSharpLegacy;
                    break;
                case "JUUID":
                case "JGUID":
                    guidRepresentation = GuidRepresentation.JavaLegacy;
                    break;
                case "PYUUID":
                case "PYGUID":
                    guidRepresentation = GuidRepresentation.PythonLegacy;
                    break;
                case "UUID":
                case "GUID":
                    guidRepresentation = GuidRepresentation.Standard;
                    break;
                default:
                    throw new BsonInternalException("Unexpected uuidConstructorName");
            }
            bytes = GuidConverter.ToBytes(guid, guidRepresentation);
            var subType = (guidRepresentation == GuidRepresentation.Standard) ? BsonBinarySubType.UuidStandard : BsonBinarySubType.UuidLegacy;
            return new BsonBinaryData(bytes, subType, guidRepresentation);
        }

        private JsonToken PopToken()
        {
            if (_pushedToken != null)
            {
                var token = _pushedToken;
                _pushedToken = null;
                return token;
            }
            else
            {
                return JsonScanner.GetNextToken(_buffer);
            }
        }

        private void PushToken(JsonToken token)
        {
            if (_pushedToken == null)
            {
                _pushedToken = token;
            }
            else
            {
                throw new BsonInternalException("There is already a pending token.");
            }
        }

        private void VerifyString(string expectedString)
        {
            var token = PopToken();
            if ((token.Type != JsonTokenType.String && token.Type != JsonTokenType.UnquotedString) || token.StringValue != expectedString)
            {
                var message = string.Format("JSON reader expected '{0}' but found '{1}'.", expectedString, token.StringValue);
                throw new FormatException(message);
            }
        }

        private void VerifyToken(string expectedLexeme)
        {
            var token = PopToken();
            if (token.Lexeme != expectedLexeme)
            {
                var message = string.Format("JSON reader expected '{0}' but found '{1}'.", expectedLexeme, token.Lexeme);
                throw new FormatException(message);
            }
        }
    }
}
