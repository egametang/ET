/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents an abstract base class for serializers.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class SerializerBase<TValue> : IBsonSerializer<TValue>
    {
        // public properties
        /// <summary>
        /// Gets the type of the values.
        /// </summary>
        /// <value>
        /// The type of the values.
        /// </value>
        public Type ValueType
        {
            get { return typeof(TValue); }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        public virtual TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw CreateCannotBeDeserializedException();
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The value.</param>
        public virtual void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            throw CreateCannotBeSerializedException();
        }

        // protected methods
        /// <summary>
        /// Creates an exception to throw when a type cannot be deserialized.
        /// </summary>
        /// <returns>An exception.</returns>
        protected Exception CreateCannotBeDeserializedException()
        {
            var message = string.Format(
                "Values of type '{0}' cannot be deserialized using a serializer of type '{1}'.",
                BsonUtils.GetFriendlyTypeName(typeof(TValue)),
                BsonUtils.GetFriendlyTypeName(GetType()));
            return new NotSupportedException(message);
        }

        /// <summary>
        /// Creates an exception to throw when a type cannot be deserialized.
        /// </summary>
        /// <returns>An exception.</returns>
        protected Exception CreateCannotBeSerializedException()
        {
            var message = string.Format(
                "Values of type '{0}' cannot be serialized using a serializer of type '{1}'.",
                BsonUtils.GetFriendlyTypeName(typeof(TValue)),
                BsonUtils.GetFriendlyTypeName(GetType()));
            return new NotSupportedException(message);
        }

        /// <summary>
        /// Creates an exception to throw when a type cannot be deserialized from a BsonType.
        /// </summary>
        /// <param name="bsonType">The BSON type.</param>
        /// <returns>An exception.</returns>
        protected Exception CreateCannotDeserializeFromBsonTypeException(BsonType bsonType)
        {
            var message = string.Format("Cannot deserialize a '{0}' from BsonType '{1}'.",
                BsonUtils.GetFriendlyTypeName(ValueType),
                bsonType);
            return new FormatException(message);
        }

        /// <summary>
        /// Ensures that the BsonType equals the expected type.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="bsonType">The expected type.</param>
        protected void EnsureBsonTypeEquals(IBsonReader reader, BsonType bsonType)
        {
            if (reader.GetCurrentBsonType() != bsonType)
            {
                throw CreateCannotDeserializeFromBsonTypeException(reader.GetCurrentBsonType());
            }
        }

        // explicit interface implementations
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, (TValue)value);
        }
    }
}
