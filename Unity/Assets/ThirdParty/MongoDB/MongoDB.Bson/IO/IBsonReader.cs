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
    /// Represents a BSON reader.
    /// </summary>
    public interface IBsonReader : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the current BsonType.
        /// </summary>
        BsonType CurrentBsonType { get; }

        /// <summary>
        /// Gets the current state of the reader.
        /// </summary>
        BsonReaderState State { get; }

        // methods
        /// <summary>
        /// Closes the reader.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets a bookmark to the reader's current position and state.
        /// </summary>
        /// <returns>A bookmark.</returns>
        BsonReaderBookmark GetBookmark();

        /// <summary>
        /// Gets the current BsonType (calls ReadBsonType if necessary).
        /// </summary>
        /// <returns>The current BsonType.</returns>
        BsonType GetCurrentBsonType();

        /// <summary>
        /// Determines whether this reader is at end of file.
        /// </summary>
        /// <returns>
        /// Whether this reader is at end of file.
        /// </returns>
        bool IsAtEndOfFile();

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A BsonBinaryData.</returns>
        BsonBinaryData ReadBinaryData();

        /// <summary>
        /// Reads a BSON boolean from the reader.
        /// </summary>
        /// <returns>A Boolean.</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads a BsonType from the reader.
        /// </summary>
        /// <returns>A BsonType.</returns>
        BsonType ReadBsonType();

        /// <summary>
        /// Reads BSON binary data from the reader.
        /// </summary>
        /// <returns>A byte array.</returns>
        byte[] ReadBytes();

        /// <summary>
        /// Reads a BSON DateTime from the reader.
        /// </summary>
        /// <returns>The number of milliseconds since the Unix epoch.</returns>
        long ReadDateTime();

        /// <summary>
        /// Reads a BSON Decimal128 from the reader.
        /// </summary>
        /// <returns>A <see cref="Decimal128" />.</returns>
        Decimal128 ReadDecimal128();

        /// <summary>
        /// Reads a BSON Double from the reader.
        /// </summary>
        /// <returns>A Double.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads the end of a BSON array from the reader.
        /// </summary>
        void ReadEndArray();

        /// <summary>
        /// Reads the end of a BSON document from the reader.
        /// </summary>
        void ReadEndDocument();

        /// <summary>
        /// Reads a BSON Int32 from the reader.
        /// </summary>
        /// <returns>An Int32.</returns>
        int ReadInt32();

        /// <summary>
        /// Reads a BSON Int64 from the reader.
        /// </summary>
        /// <returns>An Int64.</returns>
        long ReadInt64();

        /// <summary>
        /// Reads a BSON JavaScript from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        string ReadJavaScript();

        /// <summary>
        /// Reads a BSON JavaScript with scope from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <returns>A string.</returns>
        string ReadJavaScriptWithScope();

        /// <summary>
        /// Reads a BSON MaxKey from the reader.
        /// </summary>
        void ReadMaxKey();

        /// <summary>
        /// Reads a BSON MinKey from the reader.
        /// </summary>
        void ReadMinKey();

        /// <summary>
        /// Reads the name of an element from the reader (using the provided name decoder).
        /// </summary>
        /// <param name="nameDecoder">The name decoder.</param>
        /// <returns>
        /// The name of the element.
        /// </returns>
        string ReadName(INameDecoder nameDecoder);

        /// <summary>
        /// Reads a BSON null from the reader.
        /// </summary>
        void ReadNull();

        /// <summary>
        /// Reads a BSON ObjectId from the reader.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        ObjectId ReadObjectId();

        /// <summary>
        /// Reads a raw BSON array.
        /// </summary>
        /// <returns>The raw BSON array.</returns>
        IByteBuffer ReadRawBsonArray();

        /// <summary>
        /// Reads a raw BSON document.
        /// </summary>
        /// <returns>The raw BSON document.</returns>
        IByteBuffer ReadRawBsonDocument();

        /// <summary>
        /// Reads a BSON regular expression from the reader.
        /// </summary>
        /// <returns>A BsonRegularExpression.</returns>
        BsonRegularExpression ReadRegularExpression();

        /// <summary>
        /// Reads the start of a BSON array.
        /// </summary>
        void ReadStartArray();

        /// <summary>
        /// Reads the start of a BSON document.
        /// </summary>
        void ReadStartDocument();

        /// <summary>
        /// Reads a BSON string from the reader.
        /// </summary>
        /// <returns>A String.</returns>
        string ReadString();

        /// <summary>
        /// Reads a BSON symbol from the reader.
        /// </summary>
        /// <returns>A string.</returns>
        string ReadSymbol();

        /// <summary>
        /// Reads a BSON timestamp from the reader.
        /// </summary>
        /// <returns>The combined timestamp/increment.</returns>
        long ReadTimestamp();

        /// <summary>
        /// Reads a BSON undefined from the reader.
        /// </summary>
        void ReadUndefined();

        /// <summary>
        /// Returns the reader to previously bookmarked position and state.
        /// </summary>
        /// <param name="bookmark">The bookmark.</param>
        void ReturnToBookmark(BsonReaderBookmark bookmark);

        /// <summary>
        /// Skips the name (reader must be positioned on a name).
        /// </summary>
        void SkipName();

        /// <summary>
        /// Skips the value (reader must be positioned on a value).
        /// </summary>
        void SkipValue();
    }
}
