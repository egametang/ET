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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Extensions methods for IBsonSerializer.
    /// </summary>
    public static class IBsonSerializerExtensions
    {
        // methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="context">The deserialization context.</param>
        /// <returns>A deserialized value.</returns>
        public static object Deserialize(this IBsonSerializer serializer, BsonDeserializationContext context)
        {
            var args = new BsonDeserializationArgs { NominalType = serializer.ValueType };
            return serializer.Deserialize(context, args);
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <typeparam name="TValue">The type that this serializer knows how to serialize.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="context">The deserialization context.</param>
        /// <returns>A deserialized value.</returns>
        public static TValue Deserialize<TValue>(this IBsonSerializer<TValue> serializer, BsonDeserializationContext context)
        {
            var args = new BsonDeserializationArgs { NominalType = serializer.ValueType };
            return serializer.Deserialize(context, args);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        public static void Serialize(this IBsonSerializer serializer, BsonSerializationContext context, object value)
        {
            var args = new BsonSerializationArgs { NominalType = serializer.ValueType };
            serializer.Serialize(context, args, value);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <typeparam name="TValue">The type that this serializer knows how to serialize.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        public static void Serialize<TValue>(this IBsonSerializer<TValue> serializer, BsonSerializationContext context, TValue value)
        {
            var args = new BsonSerializationArgs { NominalType = serializer.ValueType };
            serializer.Serialize(context, args, value);
        }

        /// <summary>
        /// Converts a value to a BsonValue by serializing it.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="value">The value.</param>
        /// <returns>The serialized value.</returns>
        public static BsonValue ToBsonValue(this IBsonSerializer serializer, object value)
        {
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                writer.WriteStartDocument();
                writer.WriteName("x");
                serializer.Serialize(context, value);
                writer.WriteEndDocument();
            }
            return document[0];
        }

        /// <summary>
        /// Converts a value to a BsonValue by serializing it.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="value">The value.</param>
        /// <returns>The serialized value.</returns>
        public static BsonValue ToBsonValue<TValue>(this IBsonSerializer<TValue> serializer, TValue value)
        {
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                writer.WriteStartDocument();
                writer.WriteName("x");
                serializer.Serialize(context, value);
                writer.WriteEndDocument();
            }
            return document[0];
        }
    }
}
