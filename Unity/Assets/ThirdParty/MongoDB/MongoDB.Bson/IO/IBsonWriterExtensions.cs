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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Contains extension methods for IBsonWriter.
    /// </summary>
    public static class IBsonWriterExtensions
    {
        /// <summary>
        /// Writes a BSON binary data element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="binaryData">The binary data.</param>
        public static void WriteBinaryData(this IBsonWriter writer, string name, BsonBinaryData binaryData)
        {
            writer.WriteName(name);
            writer.WriteBinaryData(binaryData);
        }

        /// <summary>
        /// Writes a BSON Boolean element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The Boolean value.</param>
        public static void WriteBoolean(this IBsonWriter writer, string name, bool value)
        {
            writer.WriteName(name);
            writer.WriteBoolean(value);
        }

        /// <summary>
        /// Writes a BSON binary data element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="bytes">The bytes.</param>
        public static void WriteBytes(this IBsonWriter writer, string name, byte[] bytes)
        {
            writer.WriteName(name);
            writer.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a BSON DateTime element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        public static void WriteDateTime(this IBsonWriter writer, string name, long value)
        {
            writer.WriteName(name);
            writer.WriteDateTime(value);
        }

        /// <summary>
        /// Writes a BSON Decimal128 element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The <see cref="Decimal128"/> value.</param>
        public static void WriteDecimal128(this IBsonWriter writer, string name, Decimal128 value)
        {
            writer.WriteName(name);
            writer.WriteDecimal128(value);
        }

        /// <summary>
        /// Writes a BSON Double element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The Double value.</param>
        public static void WriteDouble(this IBsonWriter writer, string name, double value)
        {
            writer.WriteName(name);
            writer.WriteDouble(value);
        }

        /// <summary>
        /// Writes a BSON Int32 element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The Int32 value.</param>
        public static void WriteInt32(this IBsonWriter writer, string name, int value)
        {
            writer.WriteName(name);
            writer.WriteInt32(value);
        }

        /// <summary>
        /// Writes a BSON Int64 element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The Int64 value.</param>
        public static void WriteInt64(this IBsonWriter writer, string name, long value)
        {
            writer.WriteName(name);
            writer.WriteInt64(value);
        }

        /// <summary>
        /// Writes a BSON JavaScript element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="code">The JavaScript code.</param>
        public static void WriteJavaScript(this IBsonWriter writer, string name, string code)
        {
            writer.WriteName(name);
            writer.WriteJavaScript(code);
        }

        /// <summary>
        /// Writes a BSON JavaScript element to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="code">The JavaScript code.</param>
        public static void WriteJavaScriptWithScope(this IBsonWriter writer, string name, string code)
        {
            writer.WriteName(name);
            writer.WriteJavaScriptWithScope(code);
        }

        /// <summary>
        /// Writes a BSON MaxKey element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteMaxKey(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteMaxKey();
        }

        /// <summary>
        /// Writes a BSON MinKey element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteMinKey(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteMinKey();
        }

        /// <summary>
        /// Writes a BSON null element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteNull(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteNull();
        }

        /// <summary>
        /// Writes a BSON ObjectId element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="objectId">The ObjectId.</param>
        public static void WriteObjectId(this IBsonWriter writer, string name, ObjectId objectId)
        {
            writer.WriteName(name);
            writer.WriteObjectId(objectId);
        }

        /// <summary>
        /// Writes a raw BSON array.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name.</param>
        /// <param name="slice">The byte buffer containing the raw BSON array.</param>
        public static void WriteRawBsonArray(this IBsonWriter writer, string name, IByteBuffer slice)
        {
            writer.WriteName(name);
            writer.WriteRawBsonArray(slice);
        }

        /// <summary>
        /// Writes a raw BSON document.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name.</param>
        /// <param name="slice">The byte buffer containing the raw BSON document.</param>
        public static void WriteRawBsonDocument(this IBsonWriter writer, string name, IByteBuffer slice)
        {
            writer.WriteName(name);
            writer.WriteRawBsonDocument(slice);
        }

        /// <summary>
        /// Writes a BSON regular expression element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="regex">A BsonRegularExpression.</param>
        public static void WriteRegularExpression(this IBsonWriter writer, string name, BsonRegularExpression regex)
        {
            writer.WriteName(name);
            writer.WriteRegularExpression(regex);
        }

        /// <summary>
        /// Writes the start of a BSON array element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteStartArray(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteStartArray();
        }

        /// <summary>
        /// Writes the start of a BSON document element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteStartDocument(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteStartDocument();
        }

        /// <summary>
        /// Writes a BSON String element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The String value.</param>
        public static void WriteString(this IBsonWriter writer, string name, string value)
        {
            writer.WriteName(name);
            writer.WriteString(value);
        }

        /// <summary>
        /// Writes a BSON Symbol element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The symbol.</param>
        public static void WriteSymbol(this IBsonWriter writer, string name, string value)
        {
            writer.WriteName(name);
            writer.WriteSymbol(value);
        }

        /// <summary>
        /// Writes a BSON timestamp element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The combined timestamp/increment value.</param>
        public static void WriteTimestamp(this IBsonWriter writer, string name, long value)
        {
            writer.WriteName(name);
            writer.WriteTimestamp(value);
        }

        /// <summary>
        /// Writes a BSON undefined element to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The name of the element.</param>
        public static void WriteUndefined(this IBsonWriter writer, string name)
        {
            writer.WriteName(name);
            writer.WriteUndefined();
        }
    }
}
