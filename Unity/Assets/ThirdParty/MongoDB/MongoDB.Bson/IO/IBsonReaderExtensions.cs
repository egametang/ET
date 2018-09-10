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
    /// Contains extensions methods for IBsonReader.
    /// </summary>
    public static class IBsonReaderExtensions
    {
        /// <summary>
        /// Positions the reader to an element by name.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>True if the element was found.</returns>
        public static bool FindElement(this IBsonReader reader, string name)
        {
            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var elementName = reader.ReadName();
                if (elementName == name)
                {
                    return true;
                }
                reader.SkipValue();
            }

            return false;
        }

        /// <summary>
        /// Positions the reader to a string element by name.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>True if the element was found.</returns>
        public static string FindStringElement(this IBsonReader reader, string name)
        {
            BsonType bsonType;
            while ((bsonType = reader.ReadBsonType()) != BsonType.EndOfDocument)
            {
                if (bsonType == BsonType.String)
                {
                    var elementName = reader.ReadName();
                    if (elementName == name)
                    {
                        return reader.ReadString();
                    }
                    else
                    {
                        reader.SkipValue();
                    }
                }
                else
                {
                    reader.SkipName();
                    reader.SkipValue();
                }
            }

            return null;
        }

        /// <summary>
        /// Reads a BSON binary data element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A BsonBinaryData.</returns>
        public static BsonBinaryData ReadBinaryData(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadBinaryData();
        }

        /// <summary>
        /// Reads a BSON boolean element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A Boolean.</returns>
        public static bool ReadBoolean(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadBoolean();
        }

        /// <summary>
        /// Reads a BSON binary data element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReadBytes(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadBytes();
        }

        /// <summary>
        /// Reads a BSON DateTime element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>The number of milliseconds since the Unix epoch.</returns>
        public static long ReadDateTime(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadDateTime();
        }

        /// <summary>
        /// Reads a BSON Decimal128 element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A <see cref="Decimal128"/>.</returns>
        public static Decimal128 ReadDecimal128(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadDecimal128();
        }

        /// <summary>
        /// Reads a BSON Double element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A Double.</returns>
        public static double ReadDouble(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadDouble();
        }

        /// <summary>
        /// Reads a BSON Int32 element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>An Int32.</returns>
        public static int ReadInt32(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadInt32();
        }

        /// <summary>
        /// Reads a BSON Int64 element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>An Int64.</returns>
        public static long ReadInt64(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadInt64();
        }

        /// <summary>
        /// Reads a BSON JavaScript element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A string.</returns>
        public static string ReadJavaScript(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadJavaScript();
        }

        /// <summary>
        /// Reads a BSON JavaScript with scope element from the reader (call ReadStartDocument next to read the scope).
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A string.</returns>
        public static string ReadJavaScriptWithScope(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadJavaScriptWithScope();
        }

        /// <summary>
        /// Reads a BSON MaxKey element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static void ReadMaxKey(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            reader.ReadMaxKey();
        }

        /// <summary>
        /// Reads a BSON MinKey element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static void ReadMinKey(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            reader.ReadMinKey();
        }

        /// <summary>
        /// Reads the name of an element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The name of the element.</returns>
        public static string ReadName(this IBsonReader reader)
        {
            return reader.ReadName(Utf8NameDecoder.Instance);
        }

        /// <summary>
        /// Reads the name of an element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static void ReadName(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
        }

        /// <summary>
        /// Reads a BSON null element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static void ReadNull(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            reader.ReadNull();
        }

        /// <summary>
        /// Reads a BSON ObjectId element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>An ObjectId.</returns>
        public static ObjectId ReadObjectId(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadObjectId();
        }

        /// <summary>
        /// Reads a raw BSON array.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The raw BSON array.
        /// </returns>
        public static IByteBuffer ReadRawBsonArray(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadRawBsonArray();
        }

        /// <summary>
        /// Reads a raw BSON document.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <returns>The raw BSON document.</returns>
        public static IByteBuffer ReadRawBsonDocument(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadRawBsonDocument();
        }

        /// <summary>
        /// Reads a BSON regular expression element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A BsonRegularExpression.</returns>
        public static BsonRegularExpression ReadRegularExpression(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadRegularExpression();
        }

        /// <summary>
        /// Reads a BSON string element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A String.</returns>
        public static string ReadString(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadString();
        }

        /// <summary>
        /// Reads a BSON symbol element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        /// <returns>A string.</returns>
        public static string ReadSymbol(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadSymbol();
        }

        /// <summary>
        /// Reads a BSON timestamp element from the reader.
        /// </summary>
        /// <returns>The combined timestamp/increment.</returns>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static long ReadTimestamp(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            return reader.ReadTimestamp();
        }

        /// <summary>
        /// Reads a BSON undefined element from the reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the element.</param>
        public static void ReadUndefined(this IBsonReader reader, string name)
        {
            VerifyName(reader, name);
            reader.ReadUndefined();
        }

        private static void VerifyName(IBsonReader reader, string expectedName)
        {
            var actualName = reader.ReadName();
            if (actualName != expectedName)
            {
                var message = string.Format(
                    "Expected element name to be '{0}', not '{1}'.",
                    expectedName, actualName);
                throw new FormatException(message);
            }
        }
    }
}
