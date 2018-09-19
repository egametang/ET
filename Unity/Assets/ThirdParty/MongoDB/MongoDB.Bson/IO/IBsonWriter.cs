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
    /// Represents a BSON writer.
    /// </summary>
    public interface IBsonWriter : IDisposable
    {
        // properties
        /// <summary>
        /// Gets the current serialization depth.
        /// </summary>
        int SerializationDepth { get; }

        /// <summary>
        /// Gets the settings of the writer.
        /// </summary>
        BsonWriterSettings Settings { get; }

        // methods
        /// <summary>
        /// Gets the current state of the writer.
        /// </summary>
        BsonWriterState State { get; }

        // methods
        /// <summary>
        /// Closes the writer.
        /// </summary>
        void Close();

        /// <summary>
        /// Flushes any pending data to the output destination.
        /// </summary>
        void Flush();

        /// <summary>
        /// Pops the element name validator.
        /// </summary>
        /// <returns>The popped element validator.</returns>
        void PopElementNameValidator();

        /// <summary>
        /// Pushes the element name validator.
        /// </summary>
        /// <param name="validator">The validator.</param>
        void PushElementNameValidator(IElementNameValidator validator);

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        void WriteBinaryData(BsonBinaryData binaryData);

        /// <summary>
        /// Writes a BSON Boolean to the writer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        void WriteBytes(byte[] bytes);

        /// <summary>
        /// Writes a BSON DateTime to the writer.
        /// </summary>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        void WriteDateTime(long value);

        /// <summary>
        /// Writes a BSON Decimal128 to the writer.
        /// </summary>
        /// <param name="value">The <see cref="Decimal128"/> value.</param>
        void WriteDecimal128(Decimal128 value);

        /// <summary>
        /// Writes a BSON Double to the writer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes the end of a BSON array to the writer.
        /// </summary>
        void WriteEndArray();

        /// <summary>
        /// Writes the end of a BSON document to the writer.
        /// </summary>
        void WriteEndDocument();

        /// <summary>
        /// Writes a BSON Int32 to the writer.
        /// </summary>
        /// <param name="value">The Int32 value.</param>
        void WriteInt32(int value);

        /// <summary>
        /// Writes a BSON Int64 to the writer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        void WriteInt64(long value);

        /// <summary>
        /// Writes a BSON JavaScript to the writer.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        void WriteJavaScript(string code);

        /// <summary>
        /// Writes a BSON JavaScript to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        void WriteJavaScriptWithScope(string code);

        /// <summary>
        /// Writes a BSON MaxKey to the writer.
        /// </summary>
        void WriteMaxKey();

        /// <summary>
        /// Writes a BSON MinKey to the writer.
        /// </summary>
        void WriteMinKey();

        /// <summary>
        /// Writes the name of an element to the writer.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        void WriteName(string name);

        /// <summary>
        /// Writes a BSON null to the writer.
        /// </summary>
        void WriteNull();

        /// <summary>
        /// Writes a BSON ObjectId to the writer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        void WriteObjectId(ObjectId objectId);

        /// <summary>
        /// Writes a raw BSON array.
        /// </summary>
        /// <param name="slice">The byte buffer containing the raw BSON array.</param>
        void WriteRawBsonArray(IByteBuffer slice);

        /// <summary>
        /// Writes a raw BSON document.
        /// </summary>
        /// <param name="slice">The byte buffer containing the raw BSON document.</param>
        void WriteRawBsonDocument(IByteBuffer slice);

        /// <summary>
        /// Writes a BSON regular expression to the writer.
        /// </summary>
        /// <param name="regex">A BsonRegularExpression.</param>
        void WriteRegularExpression(BsonRegularExpression regex);

        /// <summary>
        /// Writes the start of a BSON array to the writer.
        /// </summary>
        void WriteStartArray();

        /// <summary>
        /// Writes the start of a BSON document to the writer.
        /// </summary>
        void WriteStartDocument();

        /// <summary>
        /// Writes a BSON String to the writer.
        /// </summary>
        /// <param name="value">The String value.</param>
        void WriteString(string value);

        /// <summary>
        /// Writes a BSON Symbol to the writer.
        /// </summary>
        /// <param name="value">The symbol.</param>
        void WriteSymbol(string value);

        /// <summary>
        /// Writes a BSON timestamp to the writer.
        /// </summary>
        /// <param name="value">The combined timestamp/increment value.</param>
        void WriteTimestamp(long value);

        /// <summary>
        /// Writes a BSON undefined to the writer.
        /// </summary>
        void WriteUndefined();
    }
}
