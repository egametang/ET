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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON writer to a TextWriter (in JSON format).
    /// </summary>
    public class JsonWriter : BsonWriter
    {
        // private fields
        private TextWriter _textWriter;
        private JsonWriterSettings _jsonWriterSettings; // same value as in base class just declared as derived class
        private JsonWriterContext _context;

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonWriter class.
        /// </summary>
        /// <param name="writer">A TextWriter.</param>
        public JsonWriter(TextWriter writer)
            : this(writer, JsonWriterSettings.Defaults)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonWriter class.
        /// </summary>
        /// <param name="writer">A TextWriter.</param>
        /// <param name="settings">Optional JsonWriter settings.</param>
        public JsonWriter(TextWriter writer, JsonWriterSettings settings)
            : base(settings)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            _textWriter = writer;
            _jsonWriterSettings = settings; // already frozen by base class
            _context = new JsonWriterContext(null, ContextType.TopLevel, "");
            State = BsonWriterState.Initial;
        }

        // public properties
        /// <summary>
        /// Gets the base TextWriter.
        /// </summary>
        /// <value>
        /// The base TextWriter.
        /// </value>
        public TextWriter BaseTextWriter
        {
            get { return _textWriter; }
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
                Flush();
                _context = null;
                State = BsonWriterState.Closed;
            }
        }

        /// <summary>
        /// Flushes any pending data to the output destination.
        /// </summary>
        public override void Flush()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            _textWriter.Flush();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        public override void WriteBinaryData(BsonBinaryData binaryData)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteBinaryData", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var subType = binaryData.SubType;
            var bytes = binaryData.Bytes;
            var guidRepresentation = binaryData.GuidRepresentation;

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$binary\" : \"{0}\", \"$type\" : \"{1}\" }}", Convert.ToBase64String(bytes), ((int)subType).ToString("x2"));
                    break;

                case JsonOutputMode.Shell:
                default:
                    switch (subType)
                    {
                        case BsonBinarySubType.UuidLegacy:
                        case BsonBinarySubType.UuidStandard:
                            _textWriter.Write(GuidToString(subType, bytes, guidRepresentation));
                            break;

                        default:
                            _textWriter.Write("new BinData({0}, \"{1}\")", (int)subType, Convert.ToBase64String(bytes));
                            break;
                    }
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Boolean to the writer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        public override void WriteBoolean(bool value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteBoolean", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write(value ? "true" : "false");

            State = GetNextState();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public override void WriteBytes(byte[] bytes)
        {
            WriteBinaryData(new BsonBinaryData(bytes, BsonBinarySubType.Binary));
        }

        /// <summary>
        /// Writes a BSON DateTime to the writer.
        /// </summary>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        public override void WriteDateTime(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteDateTime", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$date\" : {0} }}", value);
                    break;

                case JsonOutputMode.Shell:
                default:
                    // use ISODate for values that fall within .NET's DateTime range, and "new Date" for all others
                    if (value >= BsonConstants.DateTimeMinValueMillisecondsSinceEpoch &&
                        value <= BsonConstants.DateTimeMaxValueMillisecondsSinceEpoch)
                    {
                        var utcDateTime = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(value);
                        _textWriter.Write("ISODate(\"{0}\")", utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ"));
                    }
                    else
                    {
                        _textWriter.Write("new Date({0})", value);
                    }
                    break;
            }

            State = GetNextState();
        }

        /// <inheritdoc />
        public override void WriteDecimal128(Decimal128 value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState(nameof(WriteDecimal128), BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Shell:
                    _textWriter.Write("NumberDecimal(\"{0}\")", value.ToString());
                    break;

                default:
                    _textWriter.Write("{{ \"$numberDecimal\" : \"{0}\" }}", value.ToString());
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Double to the writer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        public override void WriteDouble(double value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteDouble", BsonWriterState.Value, BsonWriterState.Initial);
            }

            // if string representation looks like an integer add ".0" so that it looks like a double
            var stringRepresentation = value.ToString("R", NumberFormatInfo.InvariantInfo);
            if (Regex.IsMatch(stringRepresentation, @"^[+-]?\d+$"))
            {
                stringRepresentation += ".0";
            }

            WriteNameHelper(Name);
            _textWriter.Write(stringRepresentation);

            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON array to the writer.
        /// </summary>
        public override void WriteEndArray()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteEndArray", BsonWriterState.Value);
            }

            base.WriteEndArray();
            _textWriter.Write("]");

            _context = _context.ParentContext;
            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON document to the writer.
        /// </summary>
        public override void WriteEndDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Name)
            {
                ThrowInvalidState("WriteEndDocument", BsonWriterState.Name);
            }

            base.WriteEndDocument();
            if (_jsonWriterSettings.Indent && _context.HasElements)
            {
                _textWriter.Write(_jsonWriterSettings.NewLineChars);
                if (_context.ParentContext != null)
                {
                    _textWriter.Write(_context.ParentContext.Indentation);
                }
                _textWriter.Write("}");
            }
            else
            {
                _textWriter.Write(" }");
            }

            if (_context.ContextType == ContextType.ScopeDocument)
            {
                _context = _context.ParentContext;
                WriteEndDocument();
            }
            else
            {
                _context = _context.ParentContext;
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
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteInt32", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Int64 to the writer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        public override void WriteInt64(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteInt64", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write(value);
                    break;

                case JsonOutputMode.Shell:
                default:
                    if (value >= int.MinValue && value <= int.MaxValue)
                    {
                        _textWriter.Write("NumberLong({0})", value);
                    }
                    else
                    {
                        _textWriter.Write("NumberLong(\"{0}\")", value);
                    }
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScript(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteJavaScript", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("{{ \"$code\" : \"{0}\" }}", EscapedString(code));

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScriptWithScope(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteJavaScriptWithScope", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteStartDocument();
            WriteName("$code");
            WriteString(code);
            WriteName("$scope");

            State = BsonWriterState.ScopeDocument;
        }

        /// <summary>
        /// Writes a BSON MaxKey to the writer.
        /// </summary>
        public override void WriteMaxKey()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteMaxKey", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$maxKey\" : 1 }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("MaxKey");
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON MinKey to the writer.
        /// </summary>
        public override void WriteMinKey()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteMinKey", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$minKey\" : 1 }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("MinKey");
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON null to the writer.
        /// </summary>
        public override void WriteNull()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteNull", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("null");

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON ObjectId to the writer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        public override void WriteObjectId(ObjectId objectId)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteObjectId", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var bytes = objectId.ToByteArray();

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$oid\" : \"{0}\" }}", BsonUtils.ToHexString(bytes));
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("ObjectId(\"{0}\")", BsonUtils.ToHexString(bytes));
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON regular expression to the writer.
        /// </summary>
        /// <param name="regex">A BsonRegularExpression.</param>
        public override void WriteRegularExpression(BsonRegularExpression regex)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteRegularExpression", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var pattern = regex.Pattern;
            var options = regex.Options;

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$regex\" : \"{0}\", \"$options\" : \"{1}\" }}", EscapedString(pattern), EscapedString(options));
                    break;

                case JsonOutputMode.Shell:
                default:
                    var escapedPattern = (pattern == "") ? "(?:)" : pattern.Replace("/", @"\/");
                    _textWriter.Write("/{0}/{1}", escapedPattern, options);
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes the start of a BSON array to the writer.
        /// </summary>
        public override void WriteStartArray()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteStartArray", BsonWriterState.Value, BsonWriterState.Initial);
            }

            base.WriteStartArray();
            WriteNameHelper(Name);
            _textWriter.Write("[");

            _context = new JsonWriterContext(_context, ContextType.Array, _jsonWriterSettings.IndentChars);
            State = BsonWriterState.Value;
        }

        /// <summary>
        /// Writes the start of a BSON document to the writer.
        /// </summary>
        public override void WriteStartDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial && State != BsonWriterState.ScopeDocument)
            {
                ThrowInvalidState("WriteStartDocument", BsonWriterState.Value, BsonWriterState.Initial, BsonWriterState.ScopeDocument);
            }

            base.WriteStartDocument();
            if (State == BsonWriterState.Value || State == BsonWriterState.ScopeDocument)
            {
                WriteNameHelper(Name);
            }
            _textWriter.Write("{");

            var contextType = (State == BsonWriterState.ScopeDocument) ? ContextType.ScopeDocument : ContextType.Document;
            _context = new JsonWriterContext(_context, contextType, _jsonWriterSettings.IndentChars);
            State = BsonWriterState.Name;
        }

        /// <summary>
        /// Writes a BSON String to the writer.
        /// </summary>
        /// <param name="value">The String value.</param>
        public override void WriteString(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteString", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            WriteQuotedString(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Symbol to the writer.
        /// </summary>
        /// <param name="value">The symbol.</param>
        public override void WriteSymbol(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteSymbol", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("{{ \"$symbol\" : \"{0}\" }}", EscapedString(value));

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON timestamp to the writer.
        /// </summary>
        /// <param name="value">The combined timestamp/increment value.</param>
        public override void WriteTimestamp(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteTimestamp", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var secondsSinceEpoch = (int)((value >> 32) & 0xffffffff);
            var increment = (int)(value & 0xffffffff);

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$timestamp\" : {{ \"t\" : {0}, \"i\" : {1} }} }}", secondsSinceEpoch, increment);
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("Timestamp({0}, {1})", secondsSinceEpoch, increment);
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON undefined to the writer.
        /// </summary>
        public override void WriteUndefined()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteUndefined", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$undefined\" : true }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("undefined");
                    break;
            }

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
        private string EscapedString(string value)
        {
            if (value.All(c => !NeedsEscaping(c)))
            {
                return value;
            }

            var sb = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        switch (CharUnicodeInfo.GetUnicodeCategory(c))
                        {
                            case UnicodeCategory.UppercaseLetter:
                            case UnicodeCategory.LowercaseLetter:
                            case UnicodeCategory.TitlecaseLetter:
                            case UnicodeCategory.OtherLetter:
                            case UnicodeCategory.DecimalDigitNumber:
                            case UnicodeCategory.LetterNumber:
                            case UnicodeCategory.OtherNumber:
                            case UnicodeCategory.SpaceSeparator:
                            case UnicodeCategory.ConnectorPunctuation:
                            case UnicodeCategory.DashPunctuation:
                            case UnicodeCategory.OpenPunctuation:
                            case UnicodeCategory.ClosePunctuation:
                            case UnicodeCategory.InitialQuotePunctuation:
                            case UnicodeCategory.FinalQuotePunctuation:
                            case UnicodeCategory.OtherPunctuation:
                            case UnicodeCategory.MathSymbol:
                            case UnicodeCategory.CurrencySymbol:
                            case UnicodeCategory.ModifierSymbol:
                            case UnicodeCategory.OtherSymbol:
                                sb.Append(c);
                                break;
                            default:
                                sb.AppendFormat("\\u{0:x4}", (int)c);
                                break;
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        private BsonWriterState GetNextState()
        {
            if (_context.ContextType == ContextType.Array || _context.ContextType == ContextType.TopLevel)
            {
                return BsonWriterState.Value;
            }
            else
            {
                return BsonWriterState.Name;
            }
        }

        private string GuidToString(BsonBinarySubType subType, byte[] bytes, GuidRepresentation guidRepresentation)
        {
            if (bytes.Length != 16)
            {
                var message = string.Format("Length of binary subtype {0} must be 16, not {1}.", subType, bytes.Length);
                throw new ArgumentException(message);
            }
            if (subType == BsonBinarySubType.UuidLegacy && guidRepresentation == GuidRepresentation.Standard)
            {
                throw new ArgumentException("GuidRepresentation for binary subtype UuidLegacy must not be Standard.");
            }
            if (subType == BsonBinarySubType.UuidStandard && guidRepresentation != GuidRepresentation.Standard)
            {
                var message = string.Format("GuidRepresentation for binary subtype UuidStandard must be Standard, not {0}.", guidRepresentation);
                throw new ArgumentException(message);
            }

            if (guidRepresentation == GuidRepresentation.Unspecified)
            {
                var s = BsonUtils.ToHexString(bytes);
                var parts = new string[]
                {
                    s.Substring(0, 8),
                    s.Substring(8, 4),
                    s.Substring(12, 4),
                    s.Substring(16, 4),
                    s.Substring(20, 12)
                };
                return string.Format("HexData({0}, \"{1}\")", (int)subType, string.Join("-", parts));
            }
            else
            {
                string uuidConstructorName;
                switch (guidRepresentation)
                {
                    case GuidRepresentation.CSharpLegacy: uuidConstructorName = "CSUUID"; break;
                    case GuidRepresentation.JavaLegacy: uuidConstructorName = "JUUID"; break;
                    case GuidRepresentation.PythonLegacy: uuidConstructorName = "PYUUID"; break;
                    case GuidRepresentation.Standard: uuidConstructorName = "UUID"; break;
                    default: throw new BsonInternalException("Unexpected GuidRepresentation");
                }
                var guid = GuidConverter.FromBytes(bytes, guidRepresentation);
                return string.Format("{0}(\"{1}\")", uuidConstructorName, guid.ToString());
            }
        }

        private bool NeedsEscaping(char c)
        {
            switch (c)
            {
                case '"':
                case '\\':
                case '\b':
                case '\f':
                case '\n':
                case '\r':
                case '\t':
                    return true;

                default:
                    switch (CharUnicodeInfo.GetUnicodeCategory(c))
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.DecimalDigitNumber:
                        case UnicodeCategory.LetterNumber:
                        case UnicodeCategory.OtherNumber:
                        case UnicodeCategory.SpaceSeparator:
                        case UnicodeCategory.ConnectorPunctuation:
                        case UnicodeCategory.DashPunctuation:
                        case UnicodeCategory.OpenPunctuation:
                        case UnicodeCategory.ClosePunctuation:
                        case UnicodeCategory.InitialQuotePunctuation:
                        case UnicodeCategory.FinalQuotePunctuation:
                        case UnicodeCategory.OtherPunctuation:
                        case UnicodeCategory.MathSymbol:
                        case UnicodeCategory.CurrencySymbol:
                        case UnicodeCategory.ModifierSymbol:
                        case UnicodeCategory.OtherSymbol:
                            return false;

                        default:
                            return true;
                    }
            }
        }

        private void WriteNameHelper(string name)
        {
            switch (_context.ContextType)
            {
                case ContextType.Array:
                    // don't write Array element names in Json
                    if (_context.HasElements)
                    {
                        _textWriter.Write(", ");
                    }
                    break;
                case ContextType.Document:
                case ContextType.ScopeDocument:
                    if (_context.HasElements)
                    {
                        _textWriter.Write(",");
                    }
                    if (_jsonWriterSettings.Indent)
                    {
                        _textWriter.Write(_jsonWriterSettings.NewLineChars);
                        _textWriter.Write(_context.Indentation);
                    }
                    else
                    {
                        _textWriter.Write(" ");
                    }
                    WriteQuotedString(name);
                    _textWriter.Write(" : ");
                    break;
                case ContextType.TopLevel:
                    break;
                default:
                    throw new BsonInternalException("Invalid ContextType.");
            }

            _context.HasElements = true;
        }

        private void WriteQuotedString(string value)
        {
            _textWriter.Write("\"");
            _textWriter.Write(EscapedString(value));
            _textWriter.Write("\"");
        }
    }
}
