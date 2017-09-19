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
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a BSON reader for some external format (see subclasses).
    /// </summary>
    public abstract class BsonReader : IBsonReader
    {
        // private fields
        private bool _disposed = false;
        private BsonReaderSettings _settings;
        private BsonReaderState _state;
        private BsonType _currentBsonType;
        private string _currentName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonReader class.
        /// </summary>
        /// <param name="settings">The reader settings.</param>
        protected BsonReader(BsonReaderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _settings = settings.FrozenCopy();
            _state = BsonReaderState.Initial;
        }

        // public properties
        /// <summary>
        /// Gets the current BsonType.
        /// </summary>
        public BsonType CurrentBsonType
        {
            get { return _currentBsonType; }
            protected set { _currentBsonType = value; }
        }

        /// <summary>
        /// Gets the settings of the reader.
        /// </summary>
        public BsonReaderSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Gets the current state of the reader.
        /// </summary>
        public BsonReaderState State
        {
            get { return _state; }
            protected set { _state = value; }
        }

        // protected properties
        /// <summary>
        /// Gets the current name.
        /// </summary>
        protected string CurrentName
        {
            get { return _currentName; }
            set { _currentName = value; }
        }

        /// <summary>
        /// Gets whether the BsonReader has been disposed.
        /// </summary>
        protected bool Disposed
        {
            get { return _disposed; }
        }

        // public methods
        /// <summary>
        /// Closes the reader.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Disposes of any resources used by the reader.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
        }

        /// <summary>
        /// Gets a bookmark to the reader's current position and state.
        /// </summary>
        /// <returns>A bookmark.</returns>
        public abstract BsonReaderBookmark GetBookmark();

        /// <summary>
        /// Gets the current BsonType (calls ReadBsonType if necessary).
        /// </summary>
        /// <returns>The current BsonType.</returns>
        public BsonType GetCurrentBsonType()
        {
            if (_state == BsonReaderState.Initial || _state == BsonReaderState.ScopeDocument || _state == BsonReaderState.Type)
            {
                ReadBsonType();
            }
            if (_state != BsonReaderState.Value)
            {
                ThrowInvalidState("GetCurrentBsonType", BsonReaderState.Value);
            }
            return _currentBsonType;
        }

        /// <summary>
        /// Determines whether this reader is at end of file.
        /// </summary>
        /// <returns>
        /// Whether this reader is at end of file.
        /// </returns>
        public abstract bool IsAtEndOfFile();

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A BsonBinaryData.</returns>
        public abstract BsonBinaryData ReadBinaryData();

        /// <summary>
        /// Reads a BSON boolean from the reader.
        /// </summary>
        /// <returns>A Boolean.</returns>
        public abstract bool ReadBoolean();

        /// <summary>
        /// Reads a BsonType from the reader.
        /// </summary>
        /// <returns>A BsonType.</returns>
        public abstract BsonType ReadBsonType();

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A byte array.</returns>
        public abstract byte[] ReadBytes();

        /// <summary>
        /// Reads a BSON DateTime from the reader.
        /// </summary>
        /// <returns>The number of milliseconds since the Unix epoch.</returns>
        public abstract long ReadDateTime();

        /// <inheritdoc />
        public abstract Decimal128 ReadDecimal128();

        /// <summary>
        /// Reads a BSON Double from the reader.
        /// </summary>
        /// <returns>A Double.</returns>
        public abstract double ReadDouble();

        /// <summary>
        /// Reads the end of a BSON array from the reader.
        /// </summary>
        public abstract void ReadEndArray();

        /// <summary>
        /// Reads the end of a BSON document from the reader.
        /// </summary>
        public abstract void ReadEndDocument();

        /// <summary>
        /// Reads a BSON Int32 from the reader.
        /// </summary>
        /// <returns>An Int32.</returns>
        public abstract int ReadInt32();

        /// <summary>
        /// Reads a BSON Int64 from the reader.
        /// </summary>
        /// <returns>An Int64.</returns>
        public abstract long ReadInt64();

        /// <summary>
        /// Reads a BSON JavaScript from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        public abstract string ReadJavaScript();

        /// <summary>
        /// Reads a BSON JavaScript with scope from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <returns>A string.</returns>
        public abstract string ReadJavaScriptWithScope();

        /// <summary>
        /// Reads a BSON MaxKey from the reader.
        /// </summary>
        public abstract void ReadMaxKey();

        /// <summary>
        /// Reads a BSON MinKey from the reader.
        /// </summary>
        public abstract void ReadMinKey();

        /// <summary>
        /// Reads the name of an element from the reader.
        /// </summary>
        /// <returns>The name of the element.</returns>
        public virtual string ReadName()
        {
            return ReadName(Utf8NameDecoder.Instance);
        }

        /// <summary>
        /// Reads the name of an element from the reader (using the provided name decoder).
        /// </summary>
        /// <param name="nameDecoder">The name decoder.</param>
        /// <returns>
        /// The name of the element.
        /// </returns>
        public abstract string ReadName(INameDecoder nameDecoder);

        /// <summary>
        /// Reads a BSON null from the reader.
        /// </summary>
        public abstract void ReadNull();

        /// <summary>
        /// Reads a BSON ObjectId from the reader.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        public abstract ObjectId ReadObjectId();

        /// <summary>
        /// Reads a raw BSON array.
        /// </summary>
        /// <returns>The raw BSON array.</returns>
        public virtual IByteBuffer ReadRawBsonArray()
        {
            // overridden in BsonBinaryReader to read the raw bytes from the stream
            // for all other streams, deserialize the array and reserialize it using a BsonBinaryWriter to get the raw bytes

            var deserializationContext = BsonDeserializationContext.CreateRoot(this);
            var array = BsonArraySerializer.Instance.Deserialize(deserializationContext);

            using (var memoryStream = new MemoryStream())
            using (var bsonWriter = new BsonBinaryWriter(memoryStream, BsonBinaryWriterSettings.Defaults))
            {
                var serializationContext = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                var startPosition = memoryStream.Position + 3; // just past BsonType, "x" and null byte
                bsonWriter.WriteName("x");
                BsonArraySerializer.Instance.Serialize(serializationContext, array);
                var endPosition = memoryStream.Position;
                bsonWriter.WriteEndDocument();

                byte[] memoryStreamBuffer;
#if NETSTANDARD1_5 || NETSTANDARD1_6
                memoryStreamBuffer = memoryStream.ToArray();
#else
                memoryStreamBuffer = memoryStream.GetBuffer();
#endif
                var buffer = new ByteArrayBuffer(memoryStreamBuffer, (int)memoryStream.Length, isReadOnly: true);
                return new ByteBufferSlice(buffer, (int)startPosition, (int)(endPosition - startPosition));
            }
        }

        /// <summary>
        /// Reads a raw BSON document.
        /// </summary>
        /// <returns>The raw BSON document.</returns>
        public virtual IByteBuffer ReadRawBsonDocument()
        {
            // overridden in BsonBinaryReader to read the raw bytes from the stream
            // for all other streams, deserialize the document and use ToBson to get the raw bytes

            var deserializationContext = BsonDeserializationContext.CreateRoot(this);
            var document = BsonDocumentSerializer.Instance.Deserialize(deserializationContext);
            var bytes = document.ToBson();
            return new ByteArrayBuffer(bytes, isReadOnly: true);
        }

        /// <summary>
        /// Reads a BSON regular expression from the reader.
        /// </summary>
        /// <returns>A BsonRegularExpression.</returns>
        public abstract BsonRegularExpression ReadRegularExpression();

        /// <summary>
        /// Reads the start of a BSON array.
        /// </summary>
        public abstract void ReadStartArray();

        /// <summary>
        /// Reads the start of a BSON document.
        /// </summary>
        public abstract void ReadStartDocument();

        /// <summary>
        /// Reads a BSON string from the reader.
        /// </summary>
        /// <returns>A String.</returns>
        public abstract string ReadString();

        /// <summary>
        /// Reads a BSON symbol from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        public abstract string ReadSymbol();

        /// <summary>
        /// Reads a BSON timestamp from the reader.
        /// </summary>
        /// <returns>The combined timestamp/increment.</returns>
        public abstract long ReadTimestamp();

        /// <summary>
        /// Reads a BSON undefined from the reader.
        /// </summary>
        public abstract void ReadUndefined();

        /// <summary>
        /// Returns the reader to previously bookmarked position and state.
        /// </summary>
        /// <param name="bookmark">The bookmark.</param>
        public abstract void ReturnToBookmark(BsonReaderBookmark bookmark);

        /// <summary>
        /// Skips the name (reader must be positioned on a name).
        /// </summary>
        public abstract void SkipName();

        /// <summary>
        /// Skips the value (reader must be positioned on a value).
        /// </summary>
        public abstract void SkipValue();

        // protected methods
        /// <summary>
        /// Disposes of any resources used by the reader.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Throws an InvalidOperationException when the method called is not valid for the current ContextType.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="actualContextType">The actual ContextType.</param>
        /// <param name="validContextTypes">The valid ContextTypes.</param>
        protected void ThrowInvalidContextType(
            string methodName,
            ContextType actualContextType,
            params ContextType[] validContextTypes)
        {
            var validContextTypesString = string.Join(" or ", validContextTypes.Select(c => c.ToString()).ToArray());
            var message = string.Format(
                "{0} can only be called when ContextType is {1}, not when ContextType is {2}.",
                methodName, validContextTypesString, actualContextType);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Throws an InvalidOperationException when the method called is not valid for the current state.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="validStates">The valid states.</param>
        protected void ThrowInvalidState(string methodName, params BsonReaderState[] validStates)
        {
            var validStatesString = string.Join(" or ", validStates.Select(s => s.ToString()).ToArray());
            var message = string.Format(
                "{0} can only be called when State is {1}, not when State is {2}.",
                methodName, validStatesString, _state);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Throws an ObjectDisposedException.
        /// </summary>
        protected void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(this.GetType().Name);
        }

        /// <summary>
        /// Verifies the current state and BsonType of the reader.
        /// </summary>
        /// <param name="methodName">The name of the method calling this one.</param>
        /// <param name="requiredBsonType">The required BSON type.</param>
        protected void VerifyBsonType(string methodName, BsonType requiredBsonType)
        {
            if (_state == BsonReaderState.Initial || _state == BsonReaderState.ScopeDocument || _state == BsonReaderState.Type)
            {
                ReadBsonType();
            }
            if (_state == BsonReaderState.Name)
            {
                // ignore name
                SkipName();
            }
            if (_state != BsonReaderState.Value)
            {
                ThrowInvalidState(methodName, BsonReaderState.Value);
            }
            if (_currentBsonType != requiredBsonType)
            {
                var message = string.Format(
                    "{0} can only be called when CurrentBsonType is {1}, not when CurrentBsonType is {2}.",
                    methodName, requiredBsonType, _currentBsonType);
                throw new InvalidOperationException(message);
            }
        }
    }
}
