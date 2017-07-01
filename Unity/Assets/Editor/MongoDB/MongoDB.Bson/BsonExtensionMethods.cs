/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson
{
    /// <summary>
    /// A static class containing BSON extension methods.
    /// </summary>
    public static class BsonExtensionMethods
    {
        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson<TNominalType>(this TNominalType obj)
        {
            return ToBson(obj, typeof(TNominalType));
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson<TNominalType>(this TNominalType obj, IBsonSerializationOptions options)
        {
            return ToBson(obj, typeof(TNominalType), options);
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <param name="settings">The BsonBinaryWriter settings.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson<TNominalType>(
            this TNominalType obj,
            IBsonSerializationOptions options,
            BsonBinaryWriterSettings settings)
        {
            return ToBson(obj, typeof(TNominalType), options, settings);
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="settings">The BsonBinaryWriter settings.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson<TNominalType>(this TNominalType obj, BsonBinaryWriterSettings settings)
        {
            return ToBson(obj, typeof(TNominalType), settings);
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson(this object obj, Type nominalType)
        {
            return ToBson(obj, nominalType, BsonBinaryWriterSettings.Defaults);
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson(this object obj, Type nominalType, IBsonSerializationOptions options)
        {
            return ToBson(obj, nominalType, options, BsonBinaryWriterSettings.Defaults);
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <param name="settings">The BsonBinaryWriter settings.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson(
            this object obj,
            Type nominalType,
            IBsonSerializationOptions options,
            BsonBinaryWriterSettings settings)
        {
            using (var buffer = new BsonBuffer())
            {
                using (var bsonWriter = BsonWriter.Create(buffer, settings))
                {
                    BsonSerializer.Serialize(bsonWriter, nominalType, obj, options);
                }
                return buffer.ToByteArray();
            }
        }

        /// <summary>
        /// Converts an object to a BSON document byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="settings">The BsonBinaryWriter settings.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToBson(this object obj, Type nominalType, BsonBinaryWriterSettings settings)
        {
            return ToBson(obj, nominalType, null, settings);
        }

        /// <summary>
        /// Converts an object to a BsonDocument.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument ToBsonDocument<TNominalType>(this TNominalType obj)
        {
            return ToBsonDocument(obj, typeof(TNominalType));
        }

        /// <summary>
        /// Converts an object to a BsonDocument.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument ToBsonDocument<TNominalType>(
            this TNominalType obj,
            IBsonSerializationOptions options)
        {
            return ToBsonDocument(obj, typeof(TNominalType), options);
        }

        /// <summary>
        /// Converts an object to a BsonDocument.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument ToBsonDocument(this object obj, Type nominalType)
        {
            return ToBsonDocument(obj, nominalType, null);
        }

        /// <summary>
        /// Converts an object to a BsonDocument.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A BsonDocument.</returns>
        public static BsonDocument ToBsonDocument(
            this object obj,
            Type nominalType,
            IBsonSerializationOptions options)
        {
            if (obj == null)
            {
                return null;
            }

            var bsonDocument = obj as BsonDocument;
            if (bsonDocument != null)
            {
                return bsonDocument; // it's already a BsonDocument
            }

            var convertibleToBsonDocument = obj as IConvertibleToBsonDocument;
            if (convertibleToBsonDocument != null)
            {
                return convertibleToBsonDocument.ToBsonDocument(); // use the provided ToBsonDocument method
            }

            // otherwise serialize into a new BsonDocument
            var document = new BsonDocument();
            using (var writer = BsonWriter.Create(document))
            {
                BsonSerializer.Serialize(writer, nominalType, obj, options);
            }
            return document;
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson<TNominalType>(this TNominalType obj)
        {
            return ToJson(obj, typeof(TNominalType));
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson<TNominalType>(this TNominalType obj, IBsonSerializationOptions options)
        {
            return ToJson(obj, typeof(TNominalType), options);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <param name="settings">The JsonWriter settings.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson<TNominalType>(
            this TNominalType obj,
            IBsonSerializationOptions options,
            JsonWriterSettings settings)
        {
            return ToJson(obj, typeof(TNominalType), options, settings);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <typeparam name="TNominalType">The nominal type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="settings">The JsonWriter settings.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson<TNominalType>(this TNominalType obj, JsonWriterSettings settings)
        {
            return ToJson(obj, typeof(TNominalType), settings);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson(this object obj, Type nominalType)
        {
            return ToJson(obj, nominalType, JsonWriterSettings.Defaults);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson(this object obj, Type nominalType, IBsonSerializationOptions options)
        {
            return ToJson(obj, nominalType, options, JsonWriterSettings.Defaults);
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <param name="settings">The JsonWriter settings.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson(
            this object obj,
            Type nominalType,
            IBsonSerializationOptions options,
            JsonWriterSettings settings)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var bsonWriter = BsonWriter.Create(stringWriter, settings))
                {
                    BsonSerializer.Serialize(bsonWriter, nominalType, obj, options);
                }
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="settings">The JsonWriter settings.</param>
        /// <returns>A JSON string.</returns>
        public static string ToJson(this object obj, Type nominalType, JsonWriterSettings settings)
        {
            return ToJson(obj, nominalType, null, settings);
        }
    }
}
